using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Barcoded_Warehouse_Stock_Tracking.DataAccess;
using Barcoded_Warehouse_Stock_Tracking.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Barcoded_Warehouse_Stock_Tracking.Business
{
    public class QuoteService
    {
        private readonly WarehouseContext _context;

        public QuoteService(WarehouseContext context)
        {
            _context = context;
        }

        public string GenerateQuoteNo()
        {
            string datePrefix = "TEK-" + DateTime.Now.ToString("yyyyMMdd") + "-";
            var lastQuote = _context.Quotes
                .Where(q => q.QuoteNo.StartsWith(datePrefix))
                .OrderByDescending(q => q.Id)
                .FirstOrDefault();

            int nextNum = 1;
            if (lastQuote != null && lastQuote.QuoteNo.Length >= datePrefix.Length + 4)
            {
                string numStr = lastQuote.QuoteNo.Substring(datePrefix.Length);
                if (int.TryParse(numStr, out int num))
                {
                    nextNum = num + 1;
                }
            }
            return datePrefix + nextNum.ToString("D4");
        }

        public bool SaveQuote(Quote quote, List<QuoteItem> items, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(quote.QuoteNo))
                {
                    quote.QuoteNo = GenerateQuoteNo();
                }

                _context.Quotes.Add(quote);
                _context.SaveChanges();

                foreach (var item in items)
                {
                    item.QuoteId = quote.Id;
                    _context.QuoteItems.Add(item);
                }
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public List<Quote> GetQuotes(string statusFilter = null)
        {
            var query = _context.Quotes.AsQueryable();
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Tümü")
            {
                query = query.Where(q => q.Status == statusFilter);
            }
            return query.OrderByDescending(q => q.CreatedAt).ToList();
        }

        public Quote GetQuoteById(long id)
        {
            return _context.Quotes
                .Include(q => q.QuoteItems)
                .FirstOrDefault(q => q.Id == id);
        }

        public bool UpdateQuoteStatus(long quoteId, string status)
        {
            var q = _context.Quotes.Find(quoteId);
            if (q != null)
            {
                q.Status = status;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool GenerateQuotePdf(Quote quote, List<QuoteItem> items, string pdfPath, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                Document doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(pdfPath, FileMode.Create));
                doc.Open();

                // Turkish font support using Arial/Helvetica standard Windows font
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                BaseFont bf;
                if (File.Exists(fontPath))
                    bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                else
                    bf = BaseFont.CreateFont(BaseFont.HELVETICA, "CP1254", BaseFont.NOT_EMBEDDED);

                Font fontTitle = new Font(bf, 18, Font.BOLD, BaseColor.DARK_GRAY);
                Font fontHeader = new Font(bf, 12, Font.BOLD, BaseColor.BLACK);
                Font fontSub = new Font(bf, 10, Font.NORMAL, BaseColor.GRAY);
                Font fontBody = new Font(bf, 9, Font.NORMAL, BaseColor.BLACK);
                Font fontBodyBold = new Font(bf, 9, Font.BOLD, BaseColor.BLACK);

                // Title Banner
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 60f, 40f });

                PdfPCell cellLeft = new PdfPCell();
                cellLeft.Border = Rectangle.NO_BORDER;
                cellLeft.AddElement(new Paragraph("FİYAT TEKLİF FORMU", fontTitle));
                cellLeft.AddElement(new Paragraph("Poseidon Yazılım - Barkodlu Depo & Stok Sistemleri", fontSub));
                headerTable.AddCell(cellLeft);

                PdfPCell cellRight = new PdfPCell();
                cellRight.Border = Rectangle.NO_BORDER;
                cellRight.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellRight.AddElement(new Paragraph($"Teklif No: {quote.QuoteNo}", fontBodyBold));
                cellRight.AddElement(new Paragraph($"Tarih: {quote.CreatedAt:dd.MM.yyyy HH:mm}", fontBody));
                cellRight.AddElement(new Paragraph($"Geçerlilik Tarihi: {quote.ValidUntil:dd.MM.yyyy}", fontBody));
                headerTable.AddCell(cellRight);

                doc.Add(headerTable);
                doc.Add(new Paragraph("\n"));

                // Customer info box
                PdfPTable custTable = new PdfPTable(1);
                custTable.WidthPercentage = 100;
                PdfPCell custCell = new PdfPCell();
                custCell.BackgroundColor = new BaseColor(245, 247, 250);
                custCell.Padding = 8;
                custCell.BorderColor = new BaseColor(200, 200, 200);

                custCell.AddElement(new Paragraph("MÜŞTERİ / FİRMA BİLGİLERİ", fontHeader));
                custCell.AddElement(new Paragraph($"Müşteri / Firma Adı: {quote.CustomerName ?? "Genel Müşteri"}", fontBodyBold));
                if (!string.IsNullOrEmpty(quote.CustomerPhone))
                    custCell.AddElement(new Paragraph($"Telefon: {quote.CustomerPhone}", fontBody));
                if (!string.IsNullOrEmpty(quote.CustomerEmail))
                    custCell.AddElement(new Paragraph($"E-Posta: {quote.CustomerEmail}", fontBody));
                custTable.AddCell(custCell);

                doc.Add(custTable);
                doc.Add(new Paragraph("\n"));

                // Items Table
                PdfPTable itemsTable = new PdfPTable(5);
                itemsTable.WidthPercentage = 100;
                itemsTable.SetWidths(new float[] { 15f, 45f, 12f, 14f, 14f });

                string[] headers = { "Barkod", "Ürün Adı", "Miktar / Birim", "Birim Fiyat", "Toplam" };
                foreach (string h in headers)
                {
                    PdfPCell hCell = new PdfPCell(new Phrase(h, fontBodyBold));
                    hCell.BackgroundColor = new BaseColor(50, 60, 75);
                    hCell.Phrase.Font.Color = BaseColor.WHITE;
                    hCell.Padding = 6;
                    hCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    itemsTable.AddCell(hCell);
                }

                foreach (var item in items)
                {
                    itemsTable.AddCell(new PdfPCell(new Phrase(item.Barcode ?? "", fontBody)) { Padding = 5 });
                    itemsTable.AddCell(new PdfPCell(new Phrase(item.ProductName ?? "", fontBody)) { Padding = 5 });
                    itemsTable.AddCell(new PdfPCell(new Phrase($"{item.Quantity} {item.Unit}", fontBody)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    itemsTable.AddCell(new PdfPCell(new Phrase($"{item.UnitPrice:N2} ₺", fontBody)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                    itemsTable.AddCell(new PdfPCell(new Phrase($"{item.LineTotal:N2} ₺", fontBody)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                doc.Add(itemsTable);
                doc.Add(new Paragraph("\n"));

                // Summary / Totals Table
                PdfPTable totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 40;
                totalTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalTable.SetWidths(new float[] { 50f, 50f });

                totalTable.AddCell(new PdfPCell(new Phrase("Ara Toplam:", fontBodyBold)) { Border = Rectangle.NO_BORDER });
                totalTable.AddCell(new PdfPCell(new Phrase($"{quote.Subtotal:N2} ₺", fontBody)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                if (quote.DiscountTotal > 0)
                {
                    totalTable.AddCell(new PdfPCell(new Phrase("İskonto:", fontBodyBold)) { Border = Rectangle.NO_BORDER });
                    totalTable.AddCell(new PdfPCell(new Phrase($"-{quote.DiscountTotal:N2} ₺", fontBody)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                PdfPCell gCell1 = new PdfPCell(new Phrase("Genel Toplam:", fontHeader)) { Border = Rectangle.TOP_BORDER };
                PdfPCell gCell2 = new PdfPCell(new Phrase($"{quote.GrandTotal:N2} ₺", fontHeader)) { Border = Rectangle.TOP_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
                totalTable.AddCell(gCell1);
                totalTable.AddCell(gCell2);

                doc.Add(totalTable);

                if (!string.IsNullOrEmpty(quote.Notes))
                {
                    doc.Add(new Paragraph("\nNotlar & Şartlar:", fontHeader));
                    doc.Add(new Paragraph(quote.Notes, fontBody));
                }

                doc.Add(new Paragraph("\n\n* Bu belge bilgi amaçlı teklif niteliğindedir.", fontSub));

                doc.Close();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
