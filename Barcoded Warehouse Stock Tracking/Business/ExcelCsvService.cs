using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Barcoded_Warehouse_Stock_Tracking.Entities;

namespace Barcoded_Warehouse_Stock_Tracking.Business
{
    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public int UpdatedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public static class ExcelCsvService
    {
        private const char Separator = ';';

        public static bool ExportProductsToCsv(string filePath, List<Product> products, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                var sb = new StringBuilder();
                // Header line
                sb.AppendLine(string.Join(Separator.ToString(), new[]
                {
                    "Barkod", "Ürün Adı", "Kategori", "Birim", "Malzeme Türü",
                    "Kalite Standartı", "Birim Fiyat", "Maliyet Fiyatı", "KDV Oranı",
                    "Stok Miktarı", "Kritik Stok", "Depo / Raf Konumu"
                }));

                foreach (var p in products)
                {
                    sb.AppendLine(string.Join(Separator.ToString(), new[]
                    {
                        EscapeCsv(p.Barcode),
                        EscapeCsv(p.Name),
                        EscapeCsv(p.Category),
                        EscapeCsv(p.Unit),
                        EscapeCsv(p.MaterialType),
                        EscapeCsv(p.QualityStandard),
                        p.UnitPrice.ToString("0.00"),
                        p.CostPrice.ToString("0.00"),
                        p.VatRate.ToString("0"),
                        p.StockQty.ToString("0.##"),
                        p.CriticalStock.ToString("0.##"),
                        EscapeCsv(p.ShelfLocation)
                    }));
                }

                // Write UTF-8 with BOM so Excel opens Turkish characters correctly
                File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public static ImportResult ImportProductsFromCsv(string filePath, ProductService productService, bool updateExisting)
        {
            var result = new ImportResult();
            if (!File.Exists(filePath))
            {
                result.Errors.Add("Dosya bulunamadı.");
                return result;
            }

            try
            {
                // Read lines with UTF-8
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length <= 1)
                {
                    result.Errors.Add("Dosya boş veya sadece başlık satırı içeriyor.");
                    return result;
                }

                // Check header delimiter (comma or semicolon)
                char delimiter = lines[0].Contains(";") ? ';' : ',';

                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] cols = ParseCsvLine(line, delimiter);
                    if (cols.Length < 2)
                    {
                        result.Errors.Add($"Satır {i + 1}: Geçersiz sütun sayısı.");
                        continue;
                    }

                    string barcode = UnescapeCsv(cols[0]);
                    string name = UnescapeCsv(cols[1]);

                    if (string.IsNullOrWhiteSpace(barcode) || string.IsNullOrWhiteSpace(name))
                    {
                        result.Errors.Add($"Satır {i + 1}: Barkod veya Ürün Adı boş olamaz.");
                        continue;
                    }

                    var existing = productService.GetProductByBarcode(barcode);

                    if (existing != null)
                    {
                        if (!updateExisting)
                        {
                            result.SkippedCount++;
                            continue;
                        }

                        // Update existing product
                        if (cols.Length > 2) existing.Category = UnescapeCsv(cols[2]);
                        if (cols.Length > 3 && !string.IsNullOrWhiteSpace(cols[3])) existing.Unit = UnescapeCsv(cols[3]);
                        if (cols.Length > 4) existing.MaterialType = UnescapeCsv(cols[4]);
                        if (cols.Length > 5) existing.QualityStandard = UnescapeCsv(cols[5]);
                        if (cols.Length > 6 && double.TryParse(cols[6], out double up)) existing.UnitPrice = up;
                        if (cols.Length > 7 && double.TryParse(cols[7], out double cp)) existing.CostPrice = cp;
                        if (cols.Length > 8 && double.TryParse(cols[8], out double vat)) existing.VatRate = vat;
                        if (cols.Length > 9 && double.TryParse(cols[9], out double qty)) existing.StockQty = qty;
                        if (cols.Length > 10 && double.TryParse(cols[10], out double cs)) existing.CriticalStock = cs;
                        if (cols.Length > 11) existing.ShelfLocation = UnescapeCsv(cols[11]);

                        existing.Name = name;
                        productService.UpdateProduct(existing);
                        result.UpdatedCount++;
                    }
                    else
                    {
                        // Add new product
                        var p = new Product
                        {
                            Barcode = barcode,
                            Name = name,
                            Category = cols.Length > 2 ? UnescapeCsv(cols[2]) : "",
                            Unit = (cols.Length > 3 && !string.IsNullOrWhiteSpace(cols[3])) ? UnescapeCsv(cols[3]) : "Adet",
                            MaterialType = cols.Length > 4 ? UnescapeCsv(cols[4]) : "",
                            QualityStandard = cols.Length > 5 ? UnescapeCsv(cols[5]) : "",
                            UnitPrice = (cols.Length > 6 && double.TryParse(cols[6], out double up)) ? up : 0,
                            CostPrice = (cols.Length > 7 && double.TryParse(cols[7], out double cp)) ? cp : 0,
                            VatRate = (cols.Length > 8 && double.TryParse(cols[8], out double vat)) ? vat : 20,
                            StockQty = (cols.Length > 9 && double.TryParse(cols[9], out double qty)) ? qty : 0,
                            CriticalStock = (cols.Length > 10 && double.TryParse(cols[10], out double cs)) ? cs : 5,
                            ShelfLocation = cols.Length > 11 ? UnescapeCsv(cols[11]) : "",
                            IsActive = 1
                        };

                        productService.AddProduct(p);
                        result.SuccessCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add("Dosya okuma hatası: " + ex.Message);
            }

            return result;
        }

        public static bool ExportDataTableToCsv(string filePath, DataTable dt, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                var sb = new StringBuilder();
                var headers = new List<string>();
                foreach (DataColumn col in dt.Columns)
                {
                    headers.Add(EscapeCsv(col.ColumnName));
                }
                sb.AppendLine(string.Join(Separator.ToString(), headers));

                foreach (DataRow row in dt.Rows)
                {
                    var fields = new List<string>();
                    foreach (var item in row.ItemArray)
                    {
                        fields.Add(EscapeCsv(item?.ToString() ?? ""));
                    }
                    sb.AppendLine(string.Join(Separator.ToString(), fields));
                }

                File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private static string EscapeCsv(string val)
        {
            if (string.IsNullOrEmpty(val)) return "";
            val = val.Replace("\"", "\"\"");
            if (val.Contains(";") || val.Contains(",") || val.Contains("\n") || val.Contains("\""))
            {
                return $"\"{val}\"";
            }
            return val;
        }

        private static string UnescapeCsv(string val)
        {
            if (string.IsNullOrEmpty(val)) return "";
            val = val.Trim();
            if (val.StartsWith("\"") && val.EndsWith("\"") && val.Length >= 2)
            {
                val = val.Substring(1, val.Length - 2).Replace("\"\"", "\"");
            }
            return val;
        }

        private static string[] ParseCsvLine(string line, char delimiter)
        {
            var list = new List<string>();
            bool inQuotes = false;
            var sb = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    list.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            list.Add(sb.ToString());
            return list.ToArray();
        }
    }
}
