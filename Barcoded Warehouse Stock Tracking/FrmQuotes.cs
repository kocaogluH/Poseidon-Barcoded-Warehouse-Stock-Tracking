using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Barcoded_Warehouse_Stock_Tracking.Business;
using Barcoded_Warehouse_Stock_Tracking.DataAccess;
using Barcoded_Warehouse_Stock_Tracking.Entities;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class FrmQuotes : Form
    {
        private readonly WarehouseContext _context;
        private readonly QuoteService _quoteService;
        private readonly ProductService _productService;

        // UI Controls - Yeni Teklif
        private Guna2ComboBox _cmbCustomer;
        private Guna2TextBox _txtCustomerName;
        private Guna2TextBox _txtCustomerPhone;
        private Guna2TextBox _txtCustomerEmail;
        private Guna2DateTimePicker _dtpValidUntil;
        private Guna2TextBox _txtNotes;

        private Guna2TextBox _txtProductSearch;
        private Guna2TextBox _txtQty;
        private Guna2TextBox _txtUnitPrice;
        private Guna2Button _btnAddItem;
        private Guna2DataGridView _gridItems;

        private Label _lblSubtotalVal;
        private Guna2TextBox _txtDiscount;
        private Label _lblGrandTotalVal;

        private Guna2Button _btnSaveQuote;

        // UI Controls - Geçmiş Teklifler
        private Guna2ComboBox _cmbStatusFilter;
        private Guna2DataGridView _gridQuotes;
        private Guna2Button _btnPdfExport;
        private Guna2ComboBox _cmbChangeStatus;

        private List<QuoteItem> _currentQuoteItems = new List<QuoteItem>();
        private Product _selectedProduct;

        public FrmQuotes()
        {
            _context = new WarehouseContext();
            _quoteService = new QuoteService(_context);
            _productService = new ProductService(_context);

            InitializeUI();
            LoadCustomers();
            LoadQuotesList();
        }

        private void InitializeUI()
        {
            Text = "Poseidon Yazılım — Fiyat Teklifleri Yönetimi";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1150; Height = 700;
            BackColor = UiTheme.MainBackground;
            DoubleBuffered = true;

            var tabControl = new Guna2TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            tabControl.TabButtonHoverState.FillColor = UiTheme.Surface;
            tabControl.TabButtonIdleState.FillColor = UiTheme.MainBackground;
            tabControl.TabButtonSelectedState.FillColor = UiTheme.Primary;
            tabControl.TabButtonSelectedState.ForeColor = Color.White;

            // Tab 1: Yeni Teklif
            TabPage tabNew = new TabPage { Text = " ＋ Yeni Fiyat Teklifi Oluştur ", BackColor = UiTheme.MainBackground };
            BuildNewQuoteTab(tabNew);

            // Tab 2: Kayıtlı Teklifler
            TabPage tabList = new TabPage { Text = " 📋 Kayıtlı Teklifler & Geçmiş ", BackColor = UiTheme.MainBackground };
            BuildQuoteListTab(tabList);

            tabControl.TabPages.Add(tabNew);
            tabControl.TabPages.Add(tabList);

            Controls.Add(tabControl);

            this.FormClosed += (s, e) => _context?.Dispose();
        }

        private void BuildNewQuoteTab(TabPage page)
        {
            // Sol Taraf: Müşteri & Teklif Detayları Panel
            Panel pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 360,
                BackColor = UiTheme.Surface,
                Padding = new Padding(15)
            };

            Label lblHeaderCust = new Label
            {
                Text = "👤  Müşteri & Teklif Bilgileri",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UiTheme.Primary,
                Dock = DockStyle.Top,
                Height = 30
            };
            pnlLeft.Controls.Add(lblHeaderCust);

            _cmbCustomer = new Guna2ComboBox
            {
                Dock = DockStyle.Top,
                Height = 38,
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9.5f),
                Margin = new Padding(0, 5, 0, 5)
            };
            _cmbCustomer.SelectedIndexChanged += CustomerSelected;

            _txtCustomerName = CreateInput("Müşteri / Firma Adı *", DockStyle.Top);
            _txtCustomerPhone = CreateInput("Telefon", DockStyle.Top);
            _txtCustomerEmail = CreateInput("E-Posta", DockStyle.Top);

            Label lblValDate = new Label { Text = "Geçerlilik Tarihi:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, Dock = DockStyle.Top, Height = 22 };
            _dtpValidUntil = new Guna2DateTimePicker
            {
                Dock = DockStyle.Top,
                Height = 36,
                BorderRadius = 6,
                Value = DateTime.Now.AddDays(15),
                Margin = new Padding(0, 0, 0, 10)
            };

            Label lblNotes = new Label { Text = "Notlar & Şartlar:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, Dock = DockStyle.Top, Height = 22 };
            _txtNotes = new Guna2TextBox
            {
                Dock = DockStyle.Top,
                Height = 60,
                Multiline = true,
                PlaceholderText = "Teklör ile ilgili özel şartlar, teslimat süresi vb.",
                BorderRadius = 6,
                Margin = new Padding(0, 0, 0, 10)
            };

            pnlLeft.Controls.Add(_txtNotes);
            pnlLeft.Controls.Add(lblNotes);
            pnlLeft.Controls.Add(_dtpValidUntil);
            pnlLeft.Controls.Add(lblValDate);
            pnlLeft.Controls.Add(_txtCustomerEmail);
            pnlLeft.Controls.Add(_txtCustomerPhone);
            pnlLeft.Controls.Add(_txtCustomerName);
            pnlLeft.Controls.Add(_cmbCustomer);

            page.Controls.Add(pnlLeft);

            // Sağ Taraf: Ürün Ekleme & Kalem Listesi
            Panel pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            // Ürün Arama & Ekleme Barı
            Panel pnlAddBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = UiTheme.Surface,
                Padding = new Padding(10)
            };

            _txtProductSearch = new Guna2TextBox
            {
                PlaceholderText = "Barkod veya Ürün Adı Arayın...",
                Size = new Size(240, 38),
                BorderRadius = 6,
                Location = new Point(10, 12)
            };
            _txtProductSearch.TextChanged += ProductSearch_TextChanged;

            _txtQty = new Guna2TextBox
            {
                PlaceholderText = "Miktar",
                Text = "1",
                Size = new Size(80, 38),
                BorderRadius = 6,
                Location = new Point(260, 12)
            };

            _txtUnitPrice = new Guna2TextBox
            {
                PlaceholderText = "Teklif Fiyatı (₺)",
                Size = new Size(110, 38),
                BorderRadius = 6,
                Location = new Point(350, 12)
            };

            _btnAddItem = new Guna2Button
            {
                Text = "＋ Kalem Ekle",
                Size = new Size(110, 38),
                BorderRadius = 6,
                FillColor = UiTheme.Success,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(470, 12)
            };
            _btnAddItem.Click += (s, e) => AddItemToQuote();

            pnlAddBar.Controls.Add(_txtProductSearch);
            pnlAddBar.Controls.Add(_txtQty);
            pnlAddBar.Controls.Add(_txtUnitPrice);
            pnlAddBar.Controls.Add(_btnAddItem);

            pnlRight.Controls.Add(pnlAddBar);

            // Kalemler Grid
            _gridItems = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = UiTheme.MainBackground,
                BorderStyle = BorderStyle.None
            };
            StyleModernGrid(_gridItems);

            pnlRight.Controls.Add(_gridItems);

            // Alt Toplamlar Barı
            Panel pnlTotals = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = UiTheme.Surface,
                Padding = new Padding(15, 10, 15, 10)
            };

            _lblSubtotalVal = new Label { Text = "Ara Toplam: 0.00 ₺", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, AutoSize = true, Location = new Point(15, 22) };
            
            Label lblDisc = new Label { Text = "İskonto (₺):", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UiTheme.TextMuted, AutoSize = true, Location = new Point(220, 24) };
            _txtDiscount = new Guna2TextBox
            {
                Text = "0",
                Size = new Size(80, 32),
                BorderRadius = 6,
                Location = new Point(295, 18)
            };
            _txtDiscount.TextChanged += (s, e) => CalculateTotals();

            _lblGrandTotalVal = new Label { Text = "Genel Toplam: 0.00 ₺", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = UiTheme.Primary, AutoSize = true, Location = new Point(410, 20) };

            _btnSaveQuote = new Guna2Button
            {
                Text = "💾  Teklifi Kaydet & PDF Çıkar",
                Size = new Size(220, 44),
                BorderRadius = 8,
                FillColor = UiTheme.Primary,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(510, 12)
            };
            _btnSaveQuote.Click += (s, e) => SaveAndExportQuote();

            pnlTotals.Controls.Add(_lblSubtotalVal);
            pnlTotals.Controls.Add(lblDisc);
            pnlTotals.Controls.Add(_txtDiscount);
            pnlTotals.Controls.Add(_lblGrandTotalVal);
            pnlTotals.Controls.Add(_btnSaveQuote);

            pnlRight.Controls.Add(pnlTotals);

            page.Controls.Add(pnlRight);
            SetupItemsGridColumns();
        }

        private void BuildQuoteListTab(TabPage page)
        {
            Panel pnlTopFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = UiTheme.Surface,
                Padding = new Padding(15, 10, 15, 10)
            };

            Label lblF = new Label { Text = "Durum Filtresi:", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, AutoSize = true, Location = new Point(15, 18) };
            _cmbStatusFilter = new Guna2ComboBox
            {
                Size = new Size(160, 36),
                BorderRadius = 6,
                Location = new Point(115, 12)
            };
            _cmbStatusFilter.Items.AddRange(new object[] { "Tümü", "Beklemede", "Kabul Edildi", "Reddedildi" });
            _cmbStatusFilter.SelectedIndex = 0;
            _cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadQuotesList();

            _btnPdfExport = new Guna2Button
            {
                Text = "📄  Seçili Teklifi PDF Yazdır",
                Size = new Size(200, 38),
                BorderRadius = 6,
                FillColor = UiTheme.PrimaryDark,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(300, 12)
            };
            _btnPdfExport.Click += (s, e) => PrintSelectedQuotePdf();

            Label lblChg = new Label { Text = "Durum Değiştir:", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, AutoSize = true, Location = new Point(520, 18) };
            _cmbChangeStatus = new Guna2ComboBox
            {
                Size = new Size(160, 36),
                BorderRadius = 6,
                Location = new Point(630, 12)
            };
            _cmbChangeStatus.Items.AddRange(new object[] { "Beklemede", "Kabul Edildi", "Reddedildi" });
            _cmbChangeStatus.SelectedIndexChanged += ChangeQuoteStatus;

            pnlTopFilter.Controls.Add(lblF);
            pnlTopFilter.Controls.Add(_cmbStatusFilter);
            pnlTopFilter.Controls.Add(_btnPdfExport);
            pnlTopFilter.Controls.Add(lblChg);
            pnlTopFilter.Controls.Add(_cmbChangeStatus);

            page.Controls.Add(pnlTopFilter);

            _gridQuotes = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = UiTheme.MainBackground,
                BorderStyle = BorderStyle.None
            };
            StyleModernGrid(_gridQuotes);

            page.Controls.Add(_gridQuotes);
            SetupQuotesGridColumns();
        }

        private Guna2TextBox CreateInput(string placeholder, DockStyle dock)
        {
            return new Guna2TextBox
            {
                Dock = dock,
                Height = 38,
                PlaceholderText = placeholder,
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9.5f),
                Margin = new Padding(0, 0, 0, 8)
            };
        }

        private void LoadCustomers()
        {
            _cmbCustomer.Items.Clear();
            _cmbCustomer.Items.Add("-- Müşteri Seçin (Veya Manuel Girin) --");
            var customers = _context.Customers.OrderBy(c => c.Name).ToList();
            foreach (var c in customers)
            {
                _cmbCustomer.Items.Add(c);
            }
            _cmbCustomer.DisplayMember = "Name";
            _cmbCustomer.SelectedIndex = 0;
        }

        private void CustomerSelected(object sender, EventArgs e)
        {
            if (_cmbCustomer.SelectedItem is Customer c)
            {
                _txtCustomerName.Text = c.Name;
                _txtCustomerPhone.Text = c.Phone;
                _txtCustomerEmail.Text = c.Email;
            }
        }

        private void ProductSearch_TextChanged(object sender, EventArgs e)
        {
            string term = _txtProductSearch.Text.Trim();
            if (term.Length >= 2)
            {
                var p = _productService.GetProductByBarcode(term) ??
                        _context.Products.FirstOrDefault(x => x.IsActive == 1 && x.Name.ToLower().Contains(term.ToLower()));
                if (p != null)
                {
                    _selectedProduct = p;
                    _txtUnitPrice.Text = p.UnitPrice.ToString("0.00");
                }
            }
        }

        private void AddItemToQuote()
        {
            if (_selectedProduct == null)
            {
                string term = _txtProductSearch.Text.Trim();
                _selectedProduct = _context.Products.FirstOrDefault(x => x.IsActive == 1 && (x.Barcode == term || x.Name.ToLower().Contains(term.ToLower())));
            }

            if (_selectedProduct == null)
            {
                MessageBox.Show("Lütfen geçerli bir ürün seçin veya arayın.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(_txtQty.Text, out double qty) || qty <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir miktar girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!double.TryParse(_txtUnitPrice.Text, out double price) || price < 0)
            {
                MessageBox.Show("Lütfen geçerli bir birim fiyat girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var item = new QuoteItem
            {
                ProductId = _selectedProduct.Id,
                Barcode = _selectedProduct.Barcode,
                ProductName = _selectedProduct.Name,
                Unit = _selectedProduct.Unit,
                Quantity = qty,
                UnitPrice = price,
                LineTotal = qty * price
            };

            _currentQuoteItems.Add(item);
            RefreshItemsGrid();
            CalculateTotals();

            // Clear inputs
            _txtProductSearch.Clear();
            _txtQty.Text = "1";
            _txtUnitPrice.Clear();
            _selectedProduct = null;
        }

        private void SetupItemsGridColumns()
        {
            _gridItems.Columns.Clear();
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barcode", DataPropertyName = "Barcode", HeaderText = "Barkod", FillWeight = 15 });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", DataPropertyName = "ProductName", HeaderText = "Ürün Adı", FillWeight = 35 });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", DataPropertyName = "Quantity", HeaderText = "Miktar", FillWeight = 12 });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unit", DataPropertyName = "Unit", HeaderText = "Birim", FillWeight = 12 });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", DataPropertyName = "UnitPrice", HeaderText = "Birim Fiyat (₺)", FillWeight = 13 });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "LineTotal", DataPropertyName = "LineTotal", HeaderText = "Toplam (₺)", FillWeight = 15 });
        }

        private void RefreshItemsGrid()
        {
            _gridItems.DataSource = null;
            _gridItems.DataSource = _currentQuoteItems.ToList();
        }

        private void CalculateTotals()
        {
            double subtotal = _currentQuoteItems.Sum(x => x.LineTotal);
            double.TryParse(_txtDiscount.Text, out double discount);

            double grandTotal = Math.Max(0, subtotal - discount);

            _lblSubtotalVal.Text = $"Ara Toplam: {subtotal:N2} ₺";
            _lblGrandTotalVal.Text = $"Genel Toplam: {grandTotal:N2} ₺";
        }

        private void SaveAndExportQuote()
        {
            if (string.IsNullOrWhiteSpace(_txtCustomerName.Text))
            {
                MessageBox.Show("Lütfen müşteri/firma adını girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentQuoteItems.Count == 0)
            {
                MessageBox.Show("Teklife en az bir ürün kalemi eklemelisiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double subtotal = _currentQuoteItems.Sum(x => x.LineTotal);
            double.TryParse(_txtDiscount.Text, out double discount);
            double grandTotal = Math.Max(0, subtotal - discount);

            var quote = new Quote
            {
                QuoteNo = _quoteService.GenerateQuoteNo(),
                CustomerName = _txtCustomerName.Text.Trim(),
                CustomerPhone = _txtCustomerPhone.Text.Trim(),
                CustomerEmail = _txtCustomerEmail.Text.Trim(),
                CustomerId = (_cmbCustomer.SelectedItem is Customer c) ? (long?)c.Id : null,
                Subtotal = subtotal,
                DiscountTotal = discount,
                GrandTotal = grandTotal,
                ValidUntil = _dtpValidUntil.Value,
                Notes = _txtNotes.Text.Trim(),
                Status = "Beklemede",
                CreatedAt = DateTime.Now,
                CreatedByUserId = Session.UserId
            };

            if (_quoteService.SaveQuote(quote, _currentQuoteItems, out string err))
            {
                MessageBox.Show($"Teklif {quote.QuoteNo} başarıyla kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Ask to export PDF
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "PDF Dosyası (*.pdf)|*.pdf";
                    sfd.FileName = $"Teklif_{quote.QuoteNo}.pdf";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        if (_quoteService.GenerateQuotePdf(quote, _currentQuoteItems, sfd.FileName, out string pdfErr))
                        {
                            MessageBox.Show("Teklif PDF belgesi oluşturuldu.", "PDF Hazır", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            try { System.Diagnostics.Process.Start(sfd.FileName); } catch { }
                        }
                        else
                        {
                            MessageBox.Show("PDF Hatası: " + pdfErr, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                // Reset Form
                _currentQuoteItems.Clear();
                RefreshItemsGrid();
                CalculateTotals();
                LoadQuotesList();
            }
            else
            {
                MessageBox.Show("Teklif kaydedilemedi: " + err, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupQuotesGridColumns()
        {
            _gridQuotes.Columns.Clear();
            _gridQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", Visible = false });
            _gridQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "QuoteNo", DataPropertyName = "QuoteNo", HeaderText = "Teklif No", FillWeight = 15 });
            _gridQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "CustomerName", DataPropertyName = "CustomerName", HeaderText = "Müşteri / Firma", FillWeight = 30 });
            _gridQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "GrandTotal", DataPropertyName = "GrandTotal", HeaderText = "Genel Toplam (₺)", FillWeight = 15 });
            _gridQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "Status", HeaderText = "Durum", FillWeight = 15 });
            _gridQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ValidUntil", DataPropertyName = "ValidUntil", HeaderText = "Geçerlilik", FillWeight = 15 });
            _gridQuotes.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedAt", DataPropertyName = "CreatedAt", HeaderText = "Oluşturulma", FillWeight = 15 });
        }

        private void LoadQuotesList()
        {
            string filter = _cmbStatusFilter?.SelectedItem?.ToString();
            var list = _quoteService.GetQuotes(filter);
            _gridQuotes.DataSource = list;
        }

        private void PrintSelectedQuotePdf()
        {
            if (_gridQuotes.SelectedRows.Count == 0) return;
            var qRow = _gridQuotes.SelectedRows[0].DataBoundItem as Quote;
            if (qRow == null) return;

            var fullQuote = _quoteService.GetQuoteById(qRow.Id);
            if (fullQuote == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF Dosyası (*.pdf)|*.pdf";
                sfd.FileName = $"Teklif_{fullQuote.QuoteNo}.pdf";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (_quoteService.GenerateQuotePdf(fullQuote, fullQuote.QuoteItems.ToList(), sfd.FileName, out string pdfErr))
                    {
                        MessageBox.Show("Teklif PDF belgesi oluşturuldu.", "PDF Hazır", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        try { System.Diagnostics.Process.Start(sfd.FileName); } catch { }
                    }
                    else
                    {
                        MessageBox.Show("PDF Hatası: " + pdfErr, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ChangeQuoteStatus(object sender, EventArgs e)
        {
            if (_gridQuotes.SelectedRows.Count == 0) return;
            var qRow = _gridQuotes.SelectedRows[0].DataBoundItem as Quote;
            if (qRow == null) return;

            string newStatus = _cmbChangeStatus.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(newStatus)) return;

            if (_quoteService.UpdateQuoteStatus(qRow.Id, newStatus))
            {
                MessageBox.Show($"Teklif durumu '{newStatus}' olarak güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadQuotesList();
            }
        }

        private void StyleModernGrid(Guna2DataGridView grid)
        {
            grid.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Dark;
            grid.ThemeStyle.BackColor = UiTheme.MainBackground;
            grid.ThemeStyle.GridColor = Color.FromArgb(45, 55, 72);
            grid.ThemeStyle.HeaderStyle.BackColor = UiTheme.Surface;
            grid.ThemeStyle.HeaderStyle.ForeColor = UiTheme.Primary;
            grid.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ThemeStyle.RowsStyle.BackColor = UiTheme.MainBackground;
            grid.ThemeStyle.RowsStyle.ForeColor = UiTheme.TextPrimary;
            grid.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(50, 70, 110);
            grid.ThemeStyle.RowsStyle.SelectionForeColor = Color.White;
            grid.RowTemplate.Height = 35;
        }
    }
}
