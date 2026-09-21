using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Barcoded_Warehouse_Stock_Tracking.Business;
using Barcoded_Warehouse_Stock_Tracking.DataAccess;
using Barcoded_Warehouse_Stock_Tracking.Entities;
using System.IO;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class FrmSalesAnalysis : Form
    {
        private readonly WarehouseContext _context;
        private readonly ProductService _productService;

        private Guna2Button _btnTopSellers;
        private Guna2Button _btnLowSellers;
        private Guna2Button _btnNeverSold;
        private Guna2DataGridView _grid;

        private Guna2Button _btnQuickStockIn;
        private Guna2Button _btnQuickPriceEdit;
        private Guna2Button _btnExportCsv;

        private List<ProductAnalysisItem> _currentData = new List<ProductAnalysisItem>();
        private string _currentTab = "Top"; // Top, Low, Never

        public FrmSalesAnalysis()
        {
            _context = new WarehouseContext();
            _productService = new ProductService(_context);

            InitializeUI();
            LoadTab("Top");
        }

        private void InitializeUI()
        {
            Text = "Poseidon Yazılım — Satış & Stok Analizi";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1100; Height = 650;
            BackColor = UiTheme.MainBackground;
            DoubleBuffered = true;

            // ── ÜST BAŞLIK & SEKMELER PANELİ ──────────────────────────────────────
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = UiTheme.Surface,
                Padding = new Padding(20, 15, 20, 10)
            };

            var lblTitle = new Label
            {
                Text = "📊  Satış & Stok Hareket Analizi",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = UiTheme.Primary,
                AutoSize = true,
                Location = new Point(20, 12)
            };
            pnlTop.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = "En çok satan, az satan ve hiç satılmayan ürünlerinizi analiz edin; hızlı stok veya fiyat müdahalesi yapın.",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(22, 42)
            };
            pnlTop.Controls.Add(lblSub);

            // Tab Butonları
            _btnTopSellers = CreateTabButton("🔥 En Çok Satanlar", 20, 50, () => LoadTab("Top"));
            _btnLowSellers = CreateTabButton("❄️ Az Satanlar", 200, 50, () => LoadTab("Low"));
            _btnNeverSold = CreateTabButton("⛔ Hiç Satılmayanlar", 350, 50, () => LoadTab("Never"));

            // Tab butonlarını sağ üste konumlandır
            _btnTopSellers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnLowSellers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnNeverSold.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            _btnTopSellers.Location = new Point(620, 20);
            _btnLowSellers.Location = new Point(770, 20);
            _btnNeverSold.Location = new Point(920, 20);

            pnlTop.Controls.Add(_btnTopSellers);
            pnlTop.Controls.Add(_btnLowSellers);
            pnlTop.Controls.Add(_btnNeverSold);

            Controls.Add(pnlTop);

            // ── ALT AKSİYON PANELİ ────────────────────────────────────────────────
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = UiTheme.Surface,
                Padding = new Padding(20, 12, 20, 12)
            };

            _btnQuickStockIn = new Guna2Button
            {
                Text = "📦  Seçili Ürüne Hızlı Stok Ekle",
                Size = new Size(240, 40),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FillColor = UiTheme.Success,
                ForeColor = Color.White,
                Location = new Point(20, 12)
            };
            _btnQuickStockIn.Click += (s, e) => QuickStockInAction();

            _btnQuickPriceEdit = new Guna2Button
            {
                Text = "🏷️  Seçili Ürün Fiyatını Değiştir",
                Size = new Size(240, 40),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FillColor = UiTheme.PrimaryDark,
                ForeColor = Color.White,
                Location = new Point(270, 12)
            };
            _btnQuickPriceEdit.Click += (s, e) => QuickPriceEditAction();

            _btnExportCsv = new Guna2Button
            {
                Text = "📤  Excel / CSV Aktar",
                Size = new Size(180, 40),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FillColor = Color.FromArgb(40, 140, 90),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(880, 12)
            };
            _btnExportCsv.Click += (s, e) => ExportCsvAction();

            pnlBottom.Controls.Add(_btnQuickStockIn);
            pnlBottom.Controls.Add(_btnQuickPriceEdit);
            pnlBottom.Controls.Add(_btnExportCsv);

            Controls.Add(pnlBottom);

            // ── ORTA DATA GRID ──────────────────────────────────────────────────
            var pnlCenter = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            _grid = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = UiTheme.MainBackground,
                BorderStyle = BorderStyle.None
            };
            StyleModernGrid(_grid);

            pnlCenter.Controls.Add(_grid);
            Controls.Add(pnlCenter);

            this.FormClosed += (s, e) => _context?.Dispose();
        }

        private Guna2Button CreateTabButton(string text, int x, int y, Action onClick)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Size = new Size(140, 42),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FillColor = UiTheme.InputFill,
                ForeColor = UiTheme.TextMuted
            };
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private void LoadTab(string tab)
        {
            _currentTab = tab;

            // Update Tab styles
            _btnTopSellers.FillColor = tab == "Top" ? UiTheme.Primary : UiTheme.InputFill;
            _btnTopSellers.ForeColor = tab == "Top" ? Color.White : UiTheme.TextMuted;

            _btnLowSellers.FillColor = tab == "Low" ? UiTheme.Primary : UiTheme.InputFill;
            _btnLowSellers.ForeColor = tab == "Low" ? Color.White : UiTheme.TextMuted;

            _btnNeverSold.FillColor = tab == "Never" ? UiTheme.Primary : UiTheme.InputFill;
            _btnNeverSold.ForeColor = tab == "Never" ? Color.White : UiTheme.TextMuted;

            if (tab == "Top")
                _currentData = _productService.GetTopSellingProductsAnalysis(_context, 100);
            else if (tab == "Low")
                _currentData = _productService.GetLowSellingProductsAnalysis(_context, 100);
            else
                _currentData = _productService.GetNeverSoldProductsAnalysis(_context);

            BindGrid();
        }

        private void BindGrid()
        {
            _grid.Columns.Clear();
            _grid.AutoGenerateColumns = false;

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductId", DataPropertyName = "ProductId", Visible = false });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barcode", DataPropertyName = "Barcode", HeaderText = "Barkod", FillWeight = 15 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", DataPropertyName = "Name", HeaderText = "Ürün Adı", FillWeight = 30 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", DataPropertyName = "Category", HeaderText = "Kategori", FillWeight = 15 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "StockQty", DataPropertyName = "StockQty", HeaderText = "Mevcut Stok", FillWeight = 12 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", DataPropertyName = "UnitPrice", HeaderText = "Satış Fiyatı (₺)", FillWeight = 13 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalSoldQty", DataPropertyName = "TotalSoldQty", HeaderText = "Toplam Satılan", FillWeight = 12 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalRevenue", DataPropertyName = "TotalRevenue", HeaderText = "Toplam Ciro (₺)", FillWeight = 15 });

            _grid.DataSource = _currentData;
        }

        private ProductAnalysisItem GetSelectedProduct()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            return _grid.SelectedRows[0].DataBoundItem as ProductAnalysisItem;
        }

        private void QuickStockInAction()
        {
            var item = GetSelectedProduct();
            if (item == null)
            {
                MessageBox.Show("Lütfen stok eklenecek ürünü seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"{item.Name} için eklenecek stok miktarını girin:",
                "Hızlı Stok Girişi",
                "10");

            if (string.IsNullOrWhiteSpace(input)) return;

            if (double.TryParse(input, out double addQty) && addQty > 0)
            {
                bool success = _productService.QuickStockIn(_context, item.ProductId, addQty, "Analiz Ekranı Hızlı Stok Girişi", Session.UserId);
                if (success)
                {
                    MessageBox.Show($"'{item.Name}' stoğuna +{addQty} adet eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTab(_currentTab);
                }
            }
            else
            {
                MessageBox.Show("Geçerli bir miktar giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void QuickPriceEditAction()
        {
            var item = GetSelectedProduct();
            if (item == null)
            {
                MessageBox.Show("Lütfen fiyatı değiştirilecek ürünü seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string inputPrice = Microsoft.VisualBasic.Interaction.InputBox(
                $"{item.Name} için YENİ Satış Fiyatını (₺) girin:",
                "Fiyat Değiştir",
                item.UnitPrice.ToString("0.00"));

            if (string.IsNullOrWhiteSpace(inputPrice)) return;

            if (double.TryParse(inputPrice, out double newPrice) && newPrice >= 0)
            {
                bool success = _productService.QuickPriceUpdate(_context, item.ProductId, newPrice, item.CostPrice);
                if (success)
                {
                    MessageBox.Show($"'{item.Name}' satış fiyatı {newPrice:N2} ₺ olarak güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTab(_currentTab);
                }
            }
            else
            {
                MessageBox.Show("Geçerli bir fiyat giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportCsvAction()
        {
            if (_currentData == null || _currentData.Count == 0)
            {
                MessageBox.Show("Dışa aktarılacak veri bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Dosyası (*.csv)|*.csv";
                sfd.FileName = $"Satis_Analizi_{_currentTab}_{DateTime.Now:yyyyMMdd}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var products = new List<Product>();
                    foreach (var d in _currentData)
                    {
                        var p = _context.Products.Find(d.ProductId);
                        if (p != null) products.Add(p);
                    }

                    if (ExcelCsvService.ExportProductsToCsv(sfd.FileName, products, out string err))
                    {
                        MessageBox.Show("Analiz verileri başarıyla CSV dosyasına aktarıldı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Dışa aktarma hatası: " + err, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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
