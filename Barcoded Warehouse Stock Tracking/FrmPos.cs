using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Drawing.Printing;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class FrmPos : Form
    {
        private static readonly Color BgDark = UiTheme.MainBackground;
        private static readonly Color BgMid = UiTheme.Surface;
        private static readonly Color BgInput = UiTheme.InputFill;
        private static readonly Color Accent = UiTheme.Primary;
        private static readonly Color AccentBlu = UiTheme.PrimaryDark;
        private static readonly Color AccentGrn = UiTheme.Success;
        private static readonly Color TextMain = UiTheme.TextPrimary;
        private static readonly Color TextDim = UiTheme.TextMuted;

        private readonly Guna2TextBox _txtBarcode = new Guna2TextBox();
        private readonly Guna2NumericUpDown _numQty = new Guna2NumericUpDown();
        private readonly Guna2DataGridView _grid = new Guna2DataGridView();
        private readonly Label _lblTotal = new Label();
        private readonly Guna2TextBox _txtDiscount = new Guna2TextBox();
        private readonly Guna2TextBox _txtCash = new Guna2TextBox();
        private readonly Guna2TextBox _txtCard = new Guna2TextBox();
        private readonly Guna2TextBox _txtSaleNo = new Guna2TextBox();
        private readonly Guna2ComboBox _cmbCustomer = new Guna2ComboBox();
        private readonly CheckBox _chkCustomer = new CheckBox();
        private readonly CheckBox _chkAutoPrint = new CheckBox();
        private readonly Guna2Button _btnCustomers = new Guna2Button();

        private List<Database.SaleItemInput> _cartItems = new List<Database.SaleItemInput>();

        // ── Yardımcı: düzgün etiket oluştur ─────────────────────────────────────
        private Label MakeLbl(string text, int x, int y) => new Label
        {
            Text = text,
            ForeColor = TextDim,
            Location = new Point(x, y),
            AutoSize = true,
            Font = new Font("Segoe UI", 8, FontStyle.Regular)
        };

        // ── Yardımcı: düğme oluştur ───────────────────────────────────────────
        private Guna2Button MakeBtn(string text, Color fill, int x, int y, int w, int h)
        {
            return new Guna2Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BorderRadius = 10,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FillColor = fill,
                ForeColor = Color.White,
                HoverState = { FillColor = ControlPaint.Dark(fill, 0.1f) }
            };
        }

        public FrmPos()
        {
            Text = "Poseidon Yazılım — Kasa / POS";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1100; Height = 680;
            WindowState = FormWindowState.Normal;
            BackColor = BgDark;
            DoubleBuffered = true;

            // ══════════════════════════════════════════════════════════════════════
            // TOP PANEL  (yükseklik artırıldı, tüm elemanlar dikey ortada)
            // ══════════════════════════════════════════════════════════════════════
            const int TOP_H = 80;
            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = TOP_H,
                BackColor = BgMid,
                Padding = new Padding(0)
            };

            // Sol: Başlık — dikey ortalanmış
            var lblTitle = new Label
            {
                Text = "Hızlı Satış (F1: Barkod, F5: Nakit, F6: Kart, F10: Temizle)",
                ForeColor = Accent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            lblTitle.Location = new Point(25, (TOP_H - lblTitle.PreferredHeight) / 2);

            // Sağ: arama kutusu grubu — 3 sütun: Barkod(geniş) | Adet | Ekle
            var tlpSearch = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 2,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            tlpSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300)); // Barkod
            tlpSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); // Adet
            tlpSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130)); // Ekle
            tlpSearch.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));        // etiketler
            tlpSearch.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));        // inputlar
            tlpSearch.Size = new Size(560, TOP_H);

            var lblSearch = new Label { Text = "Barkod", ForeColor = TextDim, Font = new Font("Segoe UI", 8), Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(2, 0, 0, 2) };
            var lblQtyLbl = new Label { Text = "Adet", ForeColor = TextDim, Font = new Font("Segoe UI", 8), Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(2, 0, 0, 2) };

            _txtBarcode.Dock = DockStyle.Fill;
            _txtBarcode.PlaceholderText = "Barkod okut veya yaz...";
            _txtBarcode.BorderRadius = 8; _txtBarcode.FillColor = BgInput;
            _txtBarcode.BorderColor = UiTheme.InputBorder; _txtBarcode.ForeColor = TextMain;
            _txtBarcode.Font = new Font("Segoe UI", 11);
            _txtBarcode.Margin = new Padding(2, 2, 8, 2);
            _txtBarcode.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; AddToCart(); } };

            _numQty.Dock = DockStyle.Fill;
            _numQty.Minimum = 1; _numQty.Value = 1;
            _numQty.BorderRadius = 8; _numQty.FillColor = BgInput;
            _numQty.ForeColor = TextMain; _numQty.Font = new Font("Segoe UI", 11);
            _numQty.Margin = new Padding(0, 2, 8, 2);

            var btnAdd = MakeBtn("+ Ekle", AccentGrn, 0, 0, 120, 44);
            btnAdd.Dock = DockStyle.Fill;
            btnAdd.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnAdd.Margin = new Padding(0, 2, 12, 2);
            btnAdd.Click += (_, __) => AddToCart();

            tlpSearch.Controls.Add(lblSearch, 0, 0);
            tlpSearch.Controls.Add(lblQtyLbl, 1, 0);
            tlpSearch.Controls.Add(new Label { BackColor = Color.Transparent }, 2, 0);
            tlpSearch.Controls.Add(_txtBarcode, 0, 1);
            tlpSearch.Controls.Add(_numQty, 1, 1);
            tlpSearch.Controls.Add(btnAdd, 2, 1);

            top.Controls.Add(lblTitle);
            top.Controls.Add(tlpSearch);
            top.Resize += (_, __) =>
            {
                lblTitle.Location = new Point(25, (top.Height - lblTitle.PreferredHeight) / 2);
                tlpSearch.Location = new Point(top.Width - tlpSearch.Width - 12,
                                               (top.Height - tlpSearch.Height) / 2);
            };

            // ══════════════════════════════════════════════════════════════════════
            // CENTER (Grid)
            // ══════════════════════════════════════════════════════════════════════
            var center = new Panel { Dock = DockStyle.Fill, BackColor = BgDark, Padding = new Padding(12) };
            _grid.Dock = DockStyle.Fill;
            _grid.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            _grid.ThemeStyle.BackColor = UiTheme.Surface;
            _grid.ThemeStyle.GridColor = UiTheme.GridLine;

            _grid.ThemeStyle.HeaderStyle.BackColor = UiTheme.GridHeaderBg;
            _grid.ThemeStyle.HeaderStyle.ForeColor = Accent;
            _grid.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            _grid.ThemeStyle.RowsStyle.BackColor = UiTheme.Surface;
            _grid.ThemeStyle.RowsStyle.ForeColor = TextMain;
            _grid.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9);
            _grid.ThemeStyle.RowsStyle.SelectionBackColor = UiTheme.Selection;
            _grid.ThemeStyle.RowsStyle.SelectionForeColor = TextMain;

            _grid.BorderStyle = BorderStyle.None;
            _grid.RowHeadersVisible = false;
            _grid.RowTemplate.Height = 35;
            _grid.ColumnHeadersHeight = 40;
            
            // Ghosting sorununu önlemek için
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(_grid, true, null);

            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Barkod", DataPropertyName = "BarcodeSnapshot", Width = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ürün", DataPropertyName = "NameSnapshot", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fiyat", DataPropertyName = "UnitPrice", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Adet", DataPropertyName = "Quantity", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tutar", Width = 110 });

            center.Controls.Add(_grid);

            // ══════════════════════════════════════════════════════════════════════
            // BOTTOM PANEL  — TableLayoutPanel ile tam hizalama
            // ══════════════════════════════════════════════════════════════════════
            const int BOT_H = 185;
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = BOT_H, BackColor = UiTheme.Surface };

            // Sol blok: ödeme alanları (TableLayoutPanel)
            var tlpPay = new TableLayoutPanel
            {
                ColumnCount = 4,
                RowCount = 4,
                Location = new Point(15, 10),
                Size = new Size(590, BOT_H - 20),
                BackColor = Color.Transparent
            };
            tlpPay.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            tlpPay.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            tlpPay.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            tlpPay.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // etiketler
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // inputlar
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));  // etiketler (2. satır)
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // inputlar (2. satır)

            // — Etiketler satır 0 —
            tlpPay.Controls.Add(MakeLbl("İndirim (TL)", 0, 0), 0, 0);
            tlpPay.Controls.Add(MakeLbl("Nakit (Oto)", 0, 0), 1, 0);
            tlpPay.Controls.Add(MakeLbl("Kart", 0, 0), 2, 0);
            tlpPay.Controls.Add(MakeLbl("Dış Fiş No", 0, 0), 3, 0);

            // Etiketlerin Dock'u
            foreach (Control c in tlpPay.Controls) if (c is Label) { c.Dock = DockStyle.Fill; ((Label)c).TextAlign = ContentAlignment.BottomLeft; }

            // — Input'lar satır 1 —
            void StyleInput(Guna2TextBox t, bool ro = false)
            {
                t.Dock = DockStyle.Fill; t.Height = 38; t.BorderRadius = 8;
                t.FillColor = ro ? UiTheme.SurfaceMuted : BgInput;
                t.ForeColor = ro ? AccentGrn : TextMain;
                t.Font = new Font("Segoe UI", 10, ro ? FontStyle.Bold : FontStyle.Regular);
                t.ReadOnly = ro; t.Margin = new Padding(0, 2, 8, 2);
            }
            StyleInput(_txtDiscount); _txtDiscount.Text = "0"; _txtDiscount.TextChanged += (_, __) => UpdateTotals();
            StyleInput(_txtCash, ro: true);
            StyleInput(_txtCard); _txtCard.Text = "0"; _txtCard.TextChanged += (_, __) => UpdateTotals();
            StyleInput(_txtSaleNo); _txtSaleNo.PlaceholderText = "Opsiyonel";

            tlpPay.Controls.Add(_txtDiscount, 0, 1);
            tlpPay.Controls.Add(_txtCash, 1, 1);
            tlpPay.Controls.Add(_txtCard, 2, 1);
            tlpPay.Controls.Add(_txtSaleNo, 3, 1);

            // — Satır 2: Cari etiketi —
            var lblCari = new Label { Text = "Cari / Müşteri", ForeColor = TextDim, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            tlpPay.Controls.Add(lblCari, 0, 2);

            // — Satır 3: Cari kontrolleri —
            _chkCustomer.Text = "Veresiye";
            _chkCustomer.ForeColor = TextMain; _chkCustomer.AutoSize = false;
            _chkCustomer.Font = new Font("Segoe UI", 10, FontStyle.Bold); _chkCustomer.Dock = DockStyle.Fill;
            _chkCustomer.CheckedChanged += (_, __) =>
            {
                _cmbCustomer.Enabled = _chkCustomer.Checked;
                _btnCustomers.Enabled = _chkCustomer.Checked;
            };

            _cmbCustomer.Dock = DockStyle.Fill; _cmbCustomer.Height = 38;
            _cmbCustomer.BorderRadius = 8; _cmbCustomer.Enabled = false;
            _cmbCustomer.FillColor = BgInput; _cmbCustomer.ForeColor = TextMain;
            _cmbCustomer.DisabledState.FillColor = UiTheme.SurfaceMuted;
            _cmbCustomer.DisabledState.ForeColor = TextDim;
            _cmbCustomer.Font = new Font("Segoe UI", 10);
            _cmbCustomer.Margin = new Padding(0, 2, 8, 2);

            _btnCustomers.Text = "Müşteriler"; _btnCustomers.Dock = DockStyle.Fill;
            _btnCustomers.BorderRadius = 8;
            _btnCustomers.FillColor = UiTheme.PrimaryDark;
            _btnCustomers.DisabledState.FillColor = UiTheme.SurfaceMuted;
            _btnCustomers.DisabledState.ForeColor = TextDim;
            _btnCustomers.ForeColor = Color.White; _btnCustomers.Enabled = false;
            _btnCustomers.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _btnCustomers.Margin = new Padding(0, 2, 8, 2);
            _btnCustomers.Click += (_, __) => { using (var f = new FrmCustomers()) f.ShowDialog(); LoadCustomers(); };

            tlpPay.Controls.Add(_chkCustomer, 0, 3);
            tlpPay.Controls.Add(_cmbCustomer, 1, 3);
            tlpPay.SetColumnSpan(_cmbCustomer, 2);   // 2 sütun kaplasın
            tlpPay.Controls.Add(_btnCustomers, 3, 3);

            bottom.Controls.Add(tlpPay);

            // — Sağ blok: Toplam + Tamamla (Anchor ile sağa sabitlenir) —
            var pnlRight = new Panel
            {
                Width = 400,
                Height = BOT_H - 20,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlRight.Location = new Point(bottom.Width - pnlRight.Width - 15, 10);

            var lblTotalCap = new Label
            {
                Text = "GENEL TOPLAM",
                ForeColor = TextDim,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 8)
            };

            _lblTotal.Text = "0,00 TL"; _lblTotal.ForeColor = UiTheme.Primary;
            _lblTotal.Font = new Font("Segoe UI", 15, FontStyle.Bold);  // 20 → 15 (daha dengeli)
            _lblTotal.AutoSize = true; _lblTotal.Location = new Point(0, 28);

            var btnComplete = MakeBtn("SATIŞI TAMAMLA", AccentGrn, 210, 20, 185, 60);
            btnComplete.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnComplete.Click += (_, __) => CompleteSale();

            _chkAutoPrint.Text = "Otomatik Fiş Yazdır";
            _chkAutoPrint.ForeColor = TextDim; _chkAutoPrint.AutoSize = true;
            _chkAutoPrint.Font = new Font("Segoe UI", 8);
            _chkAutoPrint.Checked = true;
            _chkAutoPrint.Location = new Point(210, 88);

            pnlRight.Controls.Add(lblTotalCap);
            pnlRight.Controls.Add(_lblTotal);
            pnlRight.Controls.Add(btnComplete);
            pnlRight.Controls.Add(_chkAutoPrint);

            bottom.Controls.Add(pnlRight);
            bottom.Resize += (_, __) => pnlRight.Location = new Point(bottom.Width - pnlRight.Width - 15, 10);

            // ── Form'a ekle ───────────────────────────────────────────────────────
            Controls.Add(center);
            Controls.Add(bottom);
            Controls.Add(top);

            LoadCustomers();
            UpdateTotals();
        }

        private void LoadCustomers()
        {
            try
            {
                _cmbCustomer.DataSource = Database.GetCustomers();
                _cmbCustomer.DisplayMember = "Name";
                _cmbCustomer.ValueMember = "Id";
            }
            catch { }
        }

        private void AddToCart()
        {
            var bc = _txtBarcode.Text.Trim();
            if (string.IsNullOrEmpty(bc)) return;
            if (Database.TryGetProductForSale(bc, out long pid, out string name, out double price, out double stock, out string unit))
            {
                double qty = (double)_numQty.Value;
                _cartItems.Add(new Database.SaleItemInput
                {
                    ProductId = pid,
                    BarcodeSnapshot = bc,
                    NameSnapshot = name,
                    UnitSnapshot = unit,
                    UnitPrice = price,
                    Quantity = qty
                });
                RefreshGrid();
                _txtBarcode.Clear(); _numQty.Value = 1; _txtBarcode.Focus();
            }
            else MessageBox.Show("Ürün bulunamadı!");
        }

        private void RefreshGrid()
        {
            _grid.Rows.Clear();
            foreach (var it in _cartItems)
                _grid.Rows.Add(it.BarcodeSnapshot, it.NameSnapshot, it.UnitPrice, it.Quantity, it.UnitPrice * it.Quantity);
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            double subtotal = 0;
            foreach (var it in _cartItems) subtotal += it.UnitPrice * it.Quantity;
            double.TryParse(_txtDiscount.Text, out double disc);
            double.TryParse(_txtCard.Text, out double card);
            double grand = subtotal - disc;
            double cash = grand - card;
            _txtCash.Text = cash.ToString("N2");
            _lblTotal.Text = $"Toplam:  {grand:N2} TL\nÖdeme:  {(card + cash):N2} TL";
        }

        private void CompleteSale()
        {
            if (_cartItems.Count == 0) return;
            try
            {
                double.TryParse(_txtDiscount.Text, out double disc);
                double.TryParse(_txtCard.Text, out double cardVal);
                double.TryParse(_txtCash.Text, out double cashVal);

                var payments = new List<(string, double)>();
                if (cashVal > 0) payments.Add(("Cash", cashVal));
                if (cardVal > 0) payments.Add(("Card", cardVal));

                string saleNo = string.IsNullOrWhiteSpace(_txtSaleNo.Text)
                    ? "S" + DateTime.Now.Ticks : _txtSaleNo.Text.Trim();
                long? cid = _chkCustomer.Checked ? (long?)_cmbCustomer.SelectedValue : null;

                Database.CreateSale(saleNo, cid, _cartItems, disc, payments, Session.UserId ?? 0);
                if (_chkAutoPrint.Checked) PrintReceipt(saleNo, _cartItems, disc, grand: cashVal + cardVal);

                _cartItems.Clear(); RefreshGrid();
                _txtDiscount.Text = "0"; _txtCard.Text = "0"; _txtSaleNo.Clear();
                MessageBox.Show("Satış tamamlandı.");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void PrintReceipt(string saleNo, List<Database.SaleItemInput> items, double disc, double grand)
        {
            try
            {
                var pd = new PrintDocument();
                pd.PrintPage += (s, ev) =>
                {
                    Graphics g = ev.Graphics;
                    Font fTitle = new Font("Courier New", 12, FontStyle.Bold);
                    Font fBody = new Font("Courier New", 9);
                    Font fSmall = new Font("Courier New", 7);
                    int y = 10;

                    // HEADER
                    g.DrawString("MİYUKİ ATELİER", fTitle, Brushes.Black, 10, y); y += 20;
                    g.DrawString("Yeniçağ Mah. Yarım Tepe Sk. No: 38\\B", fSmall, Brushes.Black, 10, y); y += 12;
                    g.DrawString("Defne / HATAY", fSmall, Brushes.Black, 10, y); y += 12;
                    g.DrawString("Tel: 0530 626 90 99", fSmall, Brushes.Black, 10, y); y += 18;
                    
                    g.DrawString("Fiş No: " + saleNo, fBody, Brushes.Black, 10, y); y += 15;
                    g.DrawString(DateTime.Now.ToString("g"), fBody, Brushes.Black, 10, y); y += 20;
                    g.DrawString("-------------------------------", fBody, Brushes.Black, 10, y); y += 15;

                    // ITEMS
                    foreach (var it in items)
                    {
                        g.DrawString(it.NameSnapshot, fBody, Brushes.Black, 10, y); y += 12;
                        g.DrawString($"{it.Quantity} x {it.UnitPrice:N2} = {it.Quantity * it.UnitPrice:N2}",
                                     fBody, Brushes.Black, 20, y); y += 15;
                    }

                    // FOOTER
                    g.DrawString("-------------------------------", fBody, Brushes.Black, 10, y); y += 15;
                    if (disc > 0) { g.DrawString($"İndirim  : {disc:N2} TL", fBody, Brushes.Black, 10, y); y += 15; }
                    g.DrawString($"TOPLAM   : {grand:N2} TL", fTitle, Brushes.Black, 10, y); y += 30;
                    
                    g.DrawString("Bizi Tercih Ettiğiniz İçin Teşekkürler", fSmall, Brushes.Black, 10, y); y += 12;
                    g.DrawString("İyi Günler Dileriz", fSmall, Brushes.Black, 10, y); y += 12;
                    g.DrawString("www.poseidonyazilim.com", fSmall, Brushes.Black, 10, y);
                };
                pd.Print();
            }
            catch (Exception ex) { MessageBox.Show("Yazıcı hatası: " + ex.Message); }
        }
        private double GetGrandTotal()
        {
            double total = 0;
            foreach (var it in _cartItems) total += it.Quantity * it.UnitPrice;
            double.TryParse(_txtDiscount.Text, out double disc);
            return total - disc;
        }

        private void RemoveSelectedItem()
        {
            if (_grid.CurrentRow == null) return;
            var index = _grid.CurrentRow.Index;
            if (index >= 0 && index < _cartItems.Count)
            {
                _cartItems.RemoveAt(index);
                RefreshGrid();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1) { _txtBarcode.Focus(); return true; }
            if (keyData == Keys.F5) 
            { 
                _txtCard.Text = "0";
                _txtCash.Text = GetGrandTotal().ToString(); 
                CompleteSale(); 
                return true; 
            }
            if (keyData == Keys.F6) 
            { 
                _txtCash.Text = "0";
                _txtCard.Text = GetGrandTotal().ToString(); 
                CompleteSale(); 
                return true; 
            }
            if (keyData == Keys.F10) 
            { 
                if (MessageBox.Show("Sepeti temizlemek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _cartItems.Clear(); RefreshGrid(); 
                }
                return true; 
            }
            if (keyData == Keys.Delete) { RemoveSelectedItem(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
