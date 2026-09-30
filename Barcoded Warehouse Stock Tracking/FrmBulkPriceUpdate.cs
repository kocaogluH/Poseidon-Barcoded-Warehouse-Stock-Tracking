using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Barcoded_Warehouse_Stock_Tracking.Business;
using Barcoded_Warehouse_Stock_Tracking.DataAccess;
using Barcoded_Warehouse_Stock_Tracking.Entities;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class PricePreviewItem
    {
        public long ProductId { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public double CurrentPrice { get; set; }
        public double NewPrice { get; set; }
        public double Difference => NewPrice - CurrentPrice;
    }

    public class FrmBulkPriceUpdate : Form
    {
        private readonly WarehouseContext _context;
        private readonly ProductService _productService;

        private Guna2ComboBox _cmbScope;
        private Guna2ComboBox _cmbCategory;
        private Guna2ComboBox _cmbActionType;
        private Guna2TextBox _txtPercentage;
        private Guna2ComboBox _cmbTargetField;
        private Guna2CheckBox _chkRound;

        private Guna2Button _btnPreview;
        private Guna2Button _btnApply;
        private Guna2Button _btnImportPriceExcel;
        private Guna2DataGridView _gridPreview;

        private List<PricePreviewItem> _previewList = new List<PricePreviewItem>();

        public FrmBulkPriceUpdate()
        {
            _context = new WarehouseContext();
            _productService = new ProductService(_context);

            InitializeUI();
            LoadCategories();
            GeneratePreview();
        }

        private void InitializeUI()
        {
            Text = "Poseidon Yazılım — Toplu Fiyat Güncelleme & % Zam/İndirim";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1100; Height = 680;
            BackColor = UiTheme.MainBackground;
            DoubleBuffered = true;

            // ── ÜST BAŞLIK PANELİ ──────────────────────────────────────────────────
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = UiTheme.Surface,
                Padding = new Padding(20, 12, 20, 10)
            };

            Label lblTitle = new Label
            {
                Text = "🏷️  Toplu Fiyat Güncelleme ve Yüzdesel (%) Zam / İndirim Modülü",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = UiTheme.Primary,
                AutoSize = true,
                Location = new Point(20, 10)
            };
            pnlHeader.Controls.Add(lblTitle);

            Label lblSub = new Label
            {
                Text = "Bütün ürünlerinize veya kategori bazında %'lik zam/indirim uygulayın ya da Excel fiyat listenizi içeri aktarın.",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(22, 38)
            };
            pnlHeader.Controls.Add(lblSub);

            Controls.Add(pnlHeader);

            // ── FİLTRE & PARAMETRE BAR (Üst Altı) ─────────────────────────────────
            Panel pnlParams = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = UiTheme.Surface,
                Padding = new Padding(15, 10, 15, 10)
            };

            // Kapsam
            Label lblScope = new Label { Text = "Kapsam:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, Location = new Point(15, 12), AutoSize = true };
            _cmbScope = new Guna2ComboBox
            {
                Size = new Size(160, 36),
                BorderRadius = 6,
                Location = new Point(15, 34),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbScope.Items.AddRange(new object[] { "🌐 Tüm Aktif Ürünler", "📂 Kategori Bazlı" });
            _cmbScope.SelectedIndex = 0;
            _cmbScope.SelectedIndexChanged += (s, e) =>
            {
                _cmbCategory.Enabled = (_cmbScope.SelectedIndex == 1);
                GeneratePreview();
            };

            // Kategori
            Label lblCat = new Label { Text = "Kategori:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, Location = new Point(185, 12), AutoSize = true };
            _cmbCategory = new Guna2ComboBox
            {
                Size = new Size(160, 36),
                BorderRadius = 6,
                Location = new Point(185, 34),
                Enabled = false,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbCategory.SelectedIndexChanged += (s, e) => GeneratePreview();

            // İşlem Tipi
            Label lblAct = new Label { Text = "İşlem Türü:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, Location = new Point(355, 12), AutoSize = true };
            _cmbActionType = new Guna2ComboBox
            {
                Size = new Size(160, 36),
                BorderRadius = 6,
                Location = new Point(355, 34),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbActionType.Items.AddRange(new object[] { "📈 % Zam (Artış)", "📉 % İndirim (Azalış)" });
            _cmbActionType.SelectedIndex = 0;
            _cmbActionType.SelectedIndexChanged += (s, e) => GeneratePreview();

            // Oran
            Label lblPerc = new Label { Text = "Oran (%):", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, Location = new Point(525, 12), AutoSize = true };
            _txtPercentage = new Guna2TextBox
            {
                Text = "10",
                Size = new Size(80, 36),
                BorderRadius = 6,
                Location = new Point(525, 34),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            _txtPercentage.TextChanged += (s, e) => GeneratePreview();

            // Hedef Fiyat
            Label lblTgt = new Label { Text = "Uygulanacak Fiyat:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, Location = new Point(615, 12), AutoSize = true };
            _cmbTargetField = new Guna2ComboBox
            {
                Size = new Size(150, 36),
                BorderRadius = 6,
                Location = new Point(615, 34),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbTargetField.Items.AddRange(new object[] { "Satış Fiyatı", "Maliyet Fiyatı", "Her İkisi" });
            _cmbTargetField.SelectedIndex = 0;
            _cmbTargetField.SelectedIndexChanged += (s, e) => GeneratePreview();

            // Önizleme Butonu
            _btnPreview = new Guna2Button
            {
                Text = "📊 Önizle",
                Size = new Size(110, 36),
                BorderRadius = 6,
                Location = new Point(775, 34),
                FillColor = UiTheme.PrimaryDark,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            _btnPreview.Click += (s, e) => GeneratePreview();

            pnlParams.Controls.Add(lblScope); pnlParams.Controls.Add(_cmbScope);
            pnlParams.Controls.Add(lblCat); pnlParams.Controls.Add(_cmbCategory);
            pnlParams.Controls.Add(lblAct); pnlParams.Controls.Add(_cmbActionType);
            pnlParams.Controls.Add(lblPerc); pnlParams.Controls.Add(_txtPercentage);
            pnlParams.Controls.Add(lblTgt); pnlParams.Controls.Add(_cmbTargetField);
            pnlParams.Controls.Add(_btnPreview);

            Controls.Add(pnlParams);

            // ── ALT AKSİYON BAR ───────────────────────────────────────────────────
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = UiTheme.Surface,
                Padding = new Padding(20, 12, 20, 12)
            };

            _chkRound = new Guna2CheckBox
            {
                Text = "Fiyatları 2 haneli kuruşa yuvarla (Örn: 12.35 TL)",
                Checked = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            _chkRound.CheckedChanged += (s, e) => GeneratePreview();

            _btnImportPriceExcel = new Guna2Button
            {
                Text = "📥  Excel Fiyat Listesi Yükle",
                Size = new Size(220, 42),
                BorderRadius = 8,
                FillColor = Color.FromArgb(40, 140, 90),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(570, 12)
            };
            _btnImportPriceExcel.Click += (s, e) => ImportPriceExcelAction();

            _btnApply = new Guna2Button
            {
                Text = "⚡  Fiyat Değişikliklerini Uygula",
                Size = new Size(260, 42),
                BorderRadius = 8,
                FillColor = UiTheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(805, 12)
            };
            _btnApply.Click += (s, e) => ApplyPriceAdjustment();

            pnlBottom.Controls.Add(_chkRound);
            pnlBottom.Controls.Add(_btnImportPriceExcel);
            pnlBottom.Controls.Add(_btnApply);

            Controls.Add(pnlBottom);

            // ── ORTA DATA GRID (Önizleme) ─────────────────────────────────────────
            Panel pnlCenter = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            _gridPreview = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = UiTheme.MainBackground,
                BorderStyle = BorderStyle.None
            };
            StyleModernGrid(_gridPreview);

            pnlCenter.Controls.Add(_gridPreview);
            Controls.Add(pnlCenter);

            this.FormClosed += (s, e) => _context?.Dispose();
        }

        private void LoadCategories()
        {
            _cmbCategory.Items.Clear();
            _cmbCategory.Items.Add("Tümü");
            var categories = _context.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToList();
            foreach (var cat in categories)
            {
                _cmbCategory.Items.Add(cat);
            }
            if (_cmbCategory.Items.Count > 0) _cmbCategory.SelectedIndex = 0;
        }

        private void GeneratePreview()
        {
            if (!double.TryParse(_txtPercentage.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double perc) || perc < 0)
            {
                perc = 0;
            }

            bool isDiscount = (_cmbActionType.SelectedIndex == 1);
            double multiplier = isDiscount ? (1.0 - (perc / 100.0)) : (1.0 + (perc / 100.0));

            string targetField = _cmbTargetField.SelectedItem?.ToString() ?? "Satış Fiyatı";
            string catFilter = (_cmbScope.SelectedIndex == 1 && _cmbCategory.SelectedItem != null) ? _cmbCategory.SelectedItem.ToString() : null;

            var query = _context.Products.Where(p => p.IsActive == 1);
            if (!string.IsNullOrEmpty(catFilter) && catFilter != "Tümü")
            {
                query = query.Where(p => p.Category == catFilter);
            }

            var products = query.OrderBy(p => p.Name).ToList();
            _previewList = new List<PricePreviewItem>();

            foreach (var p in products)
            {
                double curP = (targetField == "Maliyet Fiyatı") ? p.CostPrice : p.UnitPrice;
                double newP = curP * multiplier;

                if (_chkRound.Checked) newP = Math.Round(newP, 2);

                _previewList.Add(new PricePreviewItem
                {
                    ProductId = p.Id,
                    Barcode = p.Barcode,
                    Name = p.Name,
                    Category = p.Category,
                    CurrentPrice = curP,
                    NewPrice = newP
                });
            }

            BindGrid();
        }

        private void BindGrid()
        {
            _gridPreview.Columns.Clear();
            _gridPreview.AutoGenerateColumns = false;

            _gridPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductId", DataPropertyName = "ProductId", Visible = false });
            _gridPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barcode", DataPropertyName = "Barcode", HeaderText = "Barkod", FillWeight = 15 });
            _gridPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", DataPropertyName = "Name", HeaderText = "Ürün Adı", FillWeight = 35 });
            _gridPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", DataPropertyName = "Category", HeaderText = "Kategori", FillWeight = 18 });
            _gridPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentPrice", DataPropertyName = "CurrentPrice", HeaderText = "Mevcut Fiyat (₺)", FillWeight = 15 });
            _gridPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "NewPrice", DataPropertyName = "NewPrice", HeaderText = "YENİ Fiyat (₺)", FillWeight = 15 });
            _gridPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "Difference", DataPropertyName = "Difference", HeaderText = "Fark (₺)", FillWeight = 12 });

            _gridPreview.CellFormatting += GridPreview_CellFormatting;
            _gridPreview.DataSource = _previewList;
        }

        private void GridPreview_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string colName = _gridPreview.Columns[e.ColumnIndex].Name;
            if (colName == "CurrentPrice" || colName == "NewPrice" || colName == "Difference")
            {
                if (e.Value != null && double.TryParse(e.Value.ToString(), out double val))
                {
                    e.Value = val.ToString("N2") + " ₺";
                    if (colName == "Difference")
                    {
                        if (val > 0)
                        {
                            e.CellStyle.ForeColor = Color.SeaGreen;
                            e.CellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                        }
                        else if (val < 0)
                        {
                            e.CellStyle.ForeColor = Color.Crimson;
                            e.CellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                        }
                    }
                }
            }
        }

        private void ApplyPriceAdjustment()
        {
            if (_previewList.Count == 0)
            {
                MessageBox.Show("Fiyatı güncellenecek ürün bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(_txtPercentage.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double perc))
            {
                MessageBox.Show("Geçerli bir oran girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool isDiscount = (_cmbActionType.SelectedIndex == 1);
            string actionName = isDiscount ? $"%{perc} İndirim" : $"%{perc} Zam";
            string targetField = _cmbTargetField.SelectedItem?.ToString() ?? "Satış Fiyatı";
            string catFilter = (_cmbScope.SelectedIndex == 1 && _cmbCategory.SelectedItem != null) ? _cmbCategory.SelectedItem.ToString() : "Tüm Ürünler";

            var confirm = MessageBox.Show(
                $"{catFilter} kapsamındaki {_previewList.Count} ürüne [{actionName}] uygulanacaktır.\n\nHedef Alan: {targetField}\n\nBu işlemi onaylıyor musunuz?",
                "Fiyat Değişikliğini Onayla",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var productIds = _previewList.Select(x => x.ProductId).ToList();
            int updatedCount = _productService.ApplyBulkPriceAdjustment(_context, productIds, catFilter, perc, isDiscount, targetField, _chkRound.Checked);

            MessageBox.Show($"Toplam {updatedCount} ürünün fiyatı başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

            GeneratePreview();
        }

        private void ImportPriceExcelAction()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV Fiyat Listesi (*.csv)|*.csv|Tüm Dosyalar (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var res = ExcelCsvService.UpdatePricesFromCsv(ofd.FileName, _productService, updateUnitPrice: true, updateCostPrice: false);

                    string msg = $"Excel Fiyat Listesi İçe Aktarıldı:\n\n" +
                                 $"• Güncellenen Ürün Fiyat Sayısı: {res.UpdatedCount}\n" +
                                 $"• Eşleşmeyen / Atlanan Ürün Sayısı: {res.SkippedCount}";

                    if (res.Errors.Count > 0)
                    {
                        msg += $"\n\nKarşılaşılan Uyarılar ({res.Errors.Count}):\n" + string.Join("\n", res.Errors.Take(5));
                    }

                    MessageBox.Show(msg, "Fiyat Aktarım Sonucu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    GeneratePreview();
                }
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
