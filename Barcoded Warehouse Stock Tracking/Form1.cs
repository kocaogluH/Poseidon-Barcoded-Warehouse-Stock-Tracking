using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Guna.UI2.WinForms;
using Barcoded_Warehouse_Stock_Tracking.Business;
using Barcoded_Warehouse_Stock_Tracking.DataAccess;
using Barcoded_Warehouse_Stock_Tracking.Entities;   
using System.Windows.Forms.DataVisualization.Charting;
using System.Data;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public partial class Form1 : Form
    {
        private DashboardService _dashboardService;
        private ProductService _productService;
        private WarehouseContext _context;
        
        // Guna Dashboard Cards
        private Guna2Panel pnlTotalProducts;
        private Label lblTotalProductsVal;
        
        private Guna2Panel pnlDailySales;
        private Label lblDailySalesVal;

        private Guna2Panel pnlLowStock;
        private Label lblLowStockVal;

        private Guna2Panel pnlPendingBalance;
        private Label lblPendingBalanceVal;

        private Chart chartTopSellers;

        private Guna2Panel _pnlSidebar;
        private readonly List<Guna2Button> _navButtons = new List<Guna2Button>();
        private Guna2Button _navReports;

        // HATAY METAL Özelleştirme Alanları
        private Guna2ComboBox cmbCategory;
        private Guna2ComboBox cmbMaterial;
        private Guna2TextBox txtShelfLocation;
        private NumericUpDown nudBoxQty;
        private NumericUpDown nudCriticalStock;
        private Guna2ComboBox cmbReason;

        // Kategori filtresi
        private string _selectedCategoryFilter = "Tümü";
        private FlowLayoutPanel _flpCategoryTabs;
        private List<Guna2Button> _categoryButtons = new List<Guna2Button>();

        public Form1()
        {
            InitializeComponent();
            _context = new WarehouseContext();
            _dashboardService = new DashboardService(_context);
            _productService = new ProductService(_context);

            this.FormClosed += (s, e) => _context?.Dispose();
            this.FormClosing += (s, e) => Database.BackupDatabase();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Session.IsLoggedIn)
            {
                var login = new LoginForm();
                if (login.ShowDialog() != DialogResult.OK)
                {
                    Application.Exit();
                    return;
                }
            }

            SetupRoleAccess();
            InitializeDashboardCards();
            SetupModernShell();
            InitializeGlasswareControls();
            InitializeContextMenu();

            // Veritabanı değiştikçe tüm UI'ı yenile (Satış, İade vb.)
            Database.DataChanged += () => {
                if (this.InvokeRequired) this.Invoke(new Action(RefreshAll));
                else RefreshAll();
            };

            if (cmbType.Items.Count > 0)
                cmbType.SelectedIndex = 0;

            // Events
            txtBarcode.TextChanged += TxtBarcode_TextChanged;
            dgvProducts.RowPrePaint += DgvProducts_RowPrePaint;
            btnReports.Click += BtnReports_Click;
            btnPos.Click += BtnPos_Click;
            btnReturns.Click += BtnReturns_Click;
            btnCustomers.Click += BtnCustomers_Click;

            btnPos.Visible = false;
            btnReturns.Visible = false;
            btnCustomers.Visible = false;
            btnReports.Visible = false;

            // Global Grid Styling
            StyleModernGrid(dgvProducts);
            StyleModernGrid(dgvMovements);

            // Yeni merkezi silme butonu
            var btnDeleteProduct = new Guna2Button
            {
                Text = "🗑  Ürünü Sil",
                Size = new Size(200, 38),
                BorderRadius = 10,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FillColor = UiTheme.Danger,
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                HoverState = { FillColor = ControlPaint.Dark(UiTheme.Danger, 0.08f) }
            };
            btnDeleteProduct.Click += (s, ev) =>
            {
                if (dgvProducts.CurrentRow == null) return;
                if (!Session.IsAdmin)
                {
                    MessageBox.Show("Bu işlem için yönetici yetkisi gereklidir.", "Yetki Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var row = dgvProducts.CurrentRow;
                var barcode = row.Cells["Barkod"].Value.ToString();
                var name = row.Cells["Urun"].Value.ToString();

                if (MessageBox.Show($"'{name}' isimli ürünü silmek istediğinize emin misiniz?", "Ürün Sil", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var product = _productService.GetProductByBarcode(barcode);
                    if (product != null)
                    {
                        _productService.DeleteProduct(product.Id);
                        RefreshAll();
                    }
                }
            };
            tabProducts.Controls.Add(btnDeleteProduct);
            void placeDeleteBtn(object __, EventArgs ___) =>
                btnDeleteProduct.Left = tabProducts.ClientSize.Width - btnDeleteProduct.Width - 18;
            tabProducts.Resize += placeDeleteBtn;
            placeDeleteBtn(null, null);

            ApplyLightChromeToDataTabs();

            RefreshAll();
            CheckForUpdatesOnStartup();
        }

        private void ApplyLightChromeToDataTabs()
        {
            tabProducts.BackColor = UiTheme.MainBackground;
            tabMovements.BackColor = UiTheme.MainBackground;

            foreach (var tb in new[] { txtBarcode, txtName, txtPrice, txtInitialStock })
            {
                if (tb == null) continue;
                tb.FillColor = UiTheme.InputFill;
                tb.ForeColor = UiTheme.TextPrimary;
                tb.BorderColor = UiTheme.InputBorder;
                tb.PlaceholderForeColor = UiTheme.TextMuted;
            }
            lblBarcode.ForeColor = lblName.ForeColor = lblPrice.ForeColor = UiTheme.TextMuted;
            btnAdd.FillColor = UiTheme.Success;
            btnAdd.HoverState.FillColor = ControlPaint.Dark(UiTheme.Success, 0.08f);

            txtBarcodeMovement.FillColor = UiTheme.InputFill;
            txtBarcodeMovement.ForeColor = UiTheme.TextPrimary;
            txtBarcodeMovement.BorderColor = UiTheme.InputBorder;
            txtBarcodeMovement.PlaceholderForeColor = UiTheme.TextMuted;

            cmbType.FillColor = UiTheme.InputFill;
            cmbType.ForeColor = UiTheme.TextPrimary;
            cmbType.BorderColor = UiTheme.InputBorder;

            nudQuantity.BackColor = UiTheme.InputFill;
            nudQuantity.ForeColor = UiTheme.TextPrimary;
            lblBarcodeMovement.ForeColor = lblQuantity.ForeColor = lblType.ForeColor = UiTheme.TextMuted;
            btnAddMovement.FillColor = UiTheme.Success;
            btnAddMovement.HoverState.FillColor = ControlPaint.Dark(UiTheme.Success, 0.08f);
        }

        private void SetupModernShell()
        {
            BackColor = UiTheme.MainBackground;
            Text = "Poseidon — Depo & Stok";

            try
            {
                string exe = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                string proj = System.IO.Path.GetFullPath(System.IO.Path.Combine(exe, "..", ".."));
                foreach (var p in new[]
                {
                    System.IO.Path.Combine(proj, "app_logo.ico"),
                    System.IO.Path.Combine(exe, "app_logo.ico"),
                    System.IO.Path.Combine(exe, "Resources", "app_logo.ico")
                })
                {
                    if (System.IO.File.Exists(p))
                    {
                        this.Icon = new Icon(p);
                        break;
                    }
                }
            }
            catch { }

            var pnlFill = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.MainBackground
            };
            Controls.Remove(tabControl);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Margin = new Padding(0);
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.ItemSize = new Size(0, 1);
            tabControl.Multiline = true;
            pnlFill.Controls.Add(tabControl);

            _pnlSidebar = new Guna2Panel
            {
                Dock = DockStyle.Left,
                Width = 236,
                FillColor = UiTheme.Sidebar,
                BorderRadius = 0
            };

            // ── Firma Adı ──
            var lblCompanyName = new Label
            {
                Text = FeatureManager.CompanyName,
                Font = new System.Drawing.Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(212, 40),
                Location = new Point(12, 20),
                BackColor = Color.Transparent
            };
            var lblCompanySub = new Label
            {
                Text = FeatureManager.CompanySubTitle,
                Font = new System.Drawing.Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 200, 230),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(212, 22),
                Location = new Point(12, 58),
                BackColor = Color.Transparent
            };
            _pnlSidebar.Controls.Add(lblCompanyName);
            _pnlSidebar.Controls.Add(lblCompanySub);

            int y = 100;
            Guna2Button nav(string caption, Action act, bool transientAction = false)
            {
                var b = new Guna2Button
                {
                    Text = caption,
                    Location = new Point(12, y),
                    Size = new Size(212, 44),
                    BorderRadius = 12,
                    FillColor = UiTheme.Sidebar, // Match sidebar color exactly
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 10f, FontStyle.Bold),
                    TextAlign = HorizontalAlignment.Left,
                    TextOffset = new Point(14, 0),
                    Cursor = Cursors.Hand,
                    BorderThickness = 0, // Ensure no borders
                    BackColor = Color.Transparent,
                    UseTransparentBackground = true,
                    Animated = true
                };
                b.HoverState.FillColor = UiTheme.SidebarHover;
                b.HoverState.ForeColor = Color.White;
                b.Click += (_, __) =>
                {
                    if (!transientAction)
                        SetNavActive(b);
                    act();
                    if (transientAction && _navButtons.Count > 0)
                    {
                        tabControl.SelectedIndex = 0;
                        SetNavActive(_navButtons[0]);
                    }
                };
                _pnlSidebar.Controls.Add(b);
                _navButtons.Add(b);
                y += 48;
                return b;
            }

            nav("  🏠  Özet", () => { tabControl.SelectedIndex = 0; });
            nav("  🛒  Satış / POS", () => { OpenChildPage("Satış / POS", new FrmPos()); });
            nav("  ↩  İade / İptal", () => { OpenChildPage("İade / İptal", new FrmReturns()); });
            nav("  📦  Ürünler", () => { tabControl.SelectedIndex = 1; });
            nav("  🏷  Kategoriler", () => { OpenChildPage("Kategoriler", new FrmCategories()); });
            nav("  📋  Stok Hareketleri", () => { tabControl.SelectedIndex = 2; });
            nav("  👤  Müşteriler", () => { OpenChildPage("Müşteriler", new FrmCustomers()); });
            _navReports = nav("  📊  Raporlar", () => { OpenChildPage("Raporlar", new FrmReports()); });
            nav("  ⚙  Ayarlar", () =>
            {
                OpenChildPage("Ayarlar", new FrmSettings());
            });

            var lblFoot = new Label
            {
                Text = Session.Username ?? "",
                ForeColor = Color.FromArgb(190, 180, 230),
                Font = new System.Drawing.Font("Segoe UI", 8f),
                AutoSize = false,
                Size = new Size(212, 36),
                Location = new Point(12, _pnlSidebar.Height - 48),
                TextAlign = ContentAlignment.BottomLeft,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.Transparent
            };
            _pnlSidebar.Controls.Add(lblFoot);
            _pnlSidebar.Resize += (_, __) => lblFoot.Top = _pnlSidebar.ClientSize.Height - lblFoot.Height - 12;

            Controls.Add(pnlFill);
            Controls.Add(_pnlSidebar);

            if (_navButtons.Count > 0)
                SetNavActive(_navButtons[0]);

            if (_navReports != null)
                _navReports.Enabled = Session.IsAdmin;

            tabControl.SelectedIndexChanged += (_, __) =>
            {
                if (_navButtons == null || _navButtons.Count < 5) return;
                int i = tabControl.SelectedIndex;
                if (i == 0) SetNavActive(_navButtons[0]);
                else if (i == 1) SetNavActive(_navButtons[3]);
                else if (i == 2) SetNavActive(_navButtons[4]);
            };
        }

        private void SetNavActive(Guna2Button btn)
        {
            foreach (var b in _navButtons)
            {
                b.FillColor = UiTheme.Sidebar;
                b.ForeColor = Color.White;
            }
            if (btn != null)
            {
                btn.FillColor = UiTheme.SidebarSelected;
                btn.ForeColor = Color.White;
            }
        }

        private void OpenChildPage(string title, Form childForm)
        {
            // Eğer admin değilse ve raporlara erişmeye çalışıyorsa engelle
            if (title == "Raporlar" && !Session.IsAdmin)
            {
                MessageBox.Show("Bu işlem için yetkiniz yok.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Önce bu formun daha önce açılıp açılmadığını kontrol et
            foreach (TabPage tp in tabControl.TabPages)
            {
                if (tp.Text == title)
                {
                    tabControl.SelectedTab = tp;
                    return;
                }
            }

            // Yeni bir TabPage oluştur
            var newTab = new TabPage(title) { BackColor = UiTheme.MainBackground };
            
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            
            newTab.Controls.Add(childForm);
            tabControl.TabPages.Add(newTab);
            tabControl.SelectedTab = newTab;
            
            childForm.Show();
            
            // Form kapandığında (eğer form içinde Close() çağrılırsa) tab'ı da kapatabiliriz
            childForm.FormClosed += (s, e) => {
                tabControl.TabPages.Remove(newTab);
                RefreshAll();
            };
        }

        private void StyleModernGrid(DataGridView g)
        {
            g.BackgroundColor = UiTheme.Surface;
            g.BorderStyle = BorderStyle.None;
            g.RowTemplate.Height = 36;
            g.AllowUserToResizeRows = false;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = UiTheme.GridLine;

            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersHeight = 40;
            g.ColumnHeadersDefaultCellStyle.BackColor = UiTheme.GridHeaderBg;
            g.ColumnHeadersDefaultCellStyle.ForeColor = UiTheme.Primary;
            g.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5f, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = UiTheme.GridHeaderBg;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = UiTheme.Primary;

            g.DefaultCellStyle.BackColor = UiTheme.Surface;
            g.DefaultCellStyle.ForeColor = UiTheme.TextPrimary;
            g.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10f);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 254); // Light Sapphire Selection
            g.DefaultCellStyle.SelectionForeColor = UiTheme.TextPrimary;

            g.AlternatingRowsDefaultCellStyle.BackColor = UiTheme.GridRowAlt;
            g.AlternatingRowsDefaultCellStyle.ForeColor = UiTheme.TextPrimary;
        }

        private void SetupRoleAccess()
        {
            if (!Session.IsAdmin)   
            {
                // Personel yetkileri sınırlama
                btnReports.Enabled = false;
            }
        }

        private void InitializeDashboardCards()
        {
            pnlTotalProducts  = CreateCard("📦 Toplam Ürün", UiTheme.CardBlue, UiTheme.TextMuted, UiTheme.Primary, 20, 20, out lblTotalProductsVal);
            pnlDailySales     = CreateCard("💰 Bugünkü Satış", UiTheme.SuccessSoft, UiTheme.TextMuted, UiTheme.Success, 240, 20, out lblDailySalesVal);
            pnlLowStock       = CreateCard("⚠ Kritik Stok (<5)", UiTheme.DangerSoft, UiTheme.TextMuted, UiTheme.Danger, 460, 20, out lblLowStockVal);
            pnlPendingBalance = CreateCard("📋 Toplam Alacak", Color.FromArgb(255, 247, 237), UiTheme.TextMuted, UiTheme.Warning, 680, 20, out lblPendingBalanceVal);

            // ── TOP SELLERS CHART ────────────────────────────────────────────────
            chartTopSellers = new Chart
            {
                BackColor = UiTheme.Surface,
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 20, 10, 10)
            };

            var area = new ChartArea("MainArea") { BackColor = Color.FromArgb(248, 249, 252) };
            area.AxisX.LabelStyle.ForeColor = UiTheme.TextPrimary;
            area.AxisY.LabelStyle.ForeColor = UiTheme.TextMuted;
            area.AxisX.MajorGrid.LineColor = UiTheme.GridLine;
            area.AxisY.MajorGrid.LineColor = UiTheme.GridLine;
            chartTopSellers.ChartAreas.Add(area);

            var series = new Series("Sales")
            {
                ChartType = SeriesChartType.Bar, // Horizontal looks more elegant
                IsValueShownAsLabel = true,
                LabelForeColor = UiTheme.TextMuted,
                Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Regular),
                Palette = ChartColorPalette.None,
                Color = UiTheme.Primary,
                ["BarLabelStyle"] = "Outside"
            };
            chartTopSellers.Series.Add(series);

            // Sleek Axis Styling
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 9);
            area.AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 9);
            area.AxisX.LineColor = Color.Transparent;
            area.AxisY.LineColor = Color.Transparent;

            var title = new Title("🔥 En Çok Satan 5 Ürün", Docking.Top, new System.Drawing.Font("Segoe UI", 14, FontStyle.Bold), UiTheme.Primary);
            chartTopSellers.Titles.Add(title);

            // ── RESPONSIVE LAYOUT ENGINE ─────────────────────────────────────────
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // Fixed height for cards
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Chart fills remaining

            var cardLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 4; i++) cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            // Adjust card sizes for fluid layout
            foreach (var p in new[] { pnlTotalProducts, pnlDailySales, pnlLowStock, pnlPendingBalance })
            {
                p.Dock = DockStyle.Fill;
                p.Margin = new Padding(10);
            }

            cardLayout.Controls.Add(pnlTotalProducts, 0, 0);
            cardLayout.Controls.Add(pnlDailySales, 1, 0);
            cardLayout.Controls.Add(pnlLowStock, 2, 0);
            cardLayout.Controls.Add(pnlPendingBalance, 3, 0);

            mainLayout.Controls.Add(cardLayout, 0, 0);
            mainLayout.Controls.Add(chartTopSellers, 0, 1);

            var dashTab = new TabPage("🏠  Dashboard");
            dashTab.BackColor = UiTheme.MainBackground;
            dashTab.Controls.Add(mainLayout);

            tabControl.TabPages.Insert(0, dashTab);
            tabControl.SelectedIndex = 0;
        }

        private Guna.UI2.WinForms.Guna2Panel CreateCard(string title, Color bg, Color titleColor, Color valueColor, int x, int y, out Label valLabel)
        {
            var pnl = new Guna.UI2.WinForms.Guna2Panel
            {
                Location = new Point(x, y),
                Size = new Size(200, 110),
                BorderRadius = 20,
                FillColor = bg,
                ShadowDecoration = { Enabled = true, Color = Color.FromArgb(30, UiTheme.Primary.R, UiTheme.Primary.G, UiTheme.Primary.B), Depth = 12, BorderRadius = 20 }
            };

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = titleColor,
                BackColor = Color.Transparent,
                Font = new System.Drawing.Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(14, 12),
                AutoSize = true
            };

            valLabel = new Label
            {
                Text = "0",
                ForeColor = valueColor,
                BackColor = Color.Transparent,
                Font = new System.Drawing.Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(14, 44),
                AutoSize = true
            };

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(valLabel);
            return pnl;
        }

        private void InitializeContextMenu()
        {
            var menu = new ContextMenuStrip();
            var mnuEdit = new ToolStripMenuItem("Düzenle");
            var mnuDelete = new ToolStripMenuItem("Sil");
            var mnuDetails = new ToolStripMenuItem("Detay Gör");

            mnuEdit.Click += (s, e) =>
            {
                if (dgvProducts.CurrentRow != null)
                {
                    var barcode = dgvProducts.CurrentRow.Cells["Barkod"].Value.ToString();
                    var exact = _productService.GetProductByBarcode(barcode);
                    if (exact != null)
                    {
                        txtBarcode.Text = exact.Barcode;
                        txtName.Text = exact.Name;
                        txtPrice.Text = exact.UnitPrice.ToString("0.00");
                        if (txtInitialStock != null) txtInitialStock.Text = exact.StockQty.ToString();
                        txtName.Focus();
                    }
                }
            };

            mnuDelete.Click += (s, e) =>
            {
                if (!Session.IsAdmin)
                {
                    MessageBox.Show("Bu işlem için yetkiniz yok (Personel yetkisi).", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (dgvProducts.CurrentRow != null)
                {
                    long id = Convert.ToInt64(dgvProducts.CurrentRow.Cells["No"].Value);
                    string name = dgvProducts.CurrentRow.Cells["Urun"].Value.ToString();
                    if (MessageBox.Show($"'{name}' isimli ürünü silmek istediğinize emin misiniz?", "Ürün Sil", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _productService.DeleteProduct(id);
                        RefreshAll();
                    }
                }
            };

            mnuDetails.Click += (s, e) =>
            {
                if (dgvProducts.CurrentRow != null)
                {
                    var barcode = dgvProducts.CurrentRow.Cells["Barkod"].Value.ToString();
                    var exact = _productService.GetProductByBarcode(barcode);
                    if (exact != null)
                    {
                        string detailText = $"Ürün Detay Bilgileri:\n\n" +
                                            $"Barkod: {exact.Barcode}\n" +
                                            $"Ürün Adı: {exact.Name}\n" +
                                            $"Fiyat: {exact.UnitPrice:C2}\n" +
                                            $"Mevcut Stok: {exact.StockQty} adet\n" +
                                            $"Alış Fiyatı: {exact.CostPrice:C2}\n" +
                                            $"KDV Oranı: %{exact.VatRate}\n";

                        MessageBox.Show(detailText, "Ürün Detay Kartı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            menu.Items.Add(mnuEdit);
            menu.Items.Add(mnuDelete);
            menu.Items.Add(mnuDetails);

            dgvProducts.ContextMenuStrip = menu;
        }

        private void TxtBarcode_TextChanged(object sender, EventArgs e)
        {
            var term = txtBarcode.Text.Trim();
            if (string.IsNullOrWhiteSpace(term))
            {
                dgvProducts.DataSource = _productService.GetAllActiveProducts()
                    .OrderBy(p => p.Id)
                    .Select(p => new { No = p.Id, Barkod = p.Barcode, Urun = p.Name, Kategori = p.Category, Malzeme = p.Material, Raf = p.ShelfLocation, Koli = p.BoxQty, Kritik = p.CriticalStock, Fiyat = p.UnitPrice, Stok = p.StockQty })
                    .ToList();
            }
            else
            {
                var filtered = _productService.SearchProducts(term);
                dgvProducts.DataSource = filtered
                    .OrderBy(p => p.Id)
                    .Select(p => new { No = p.Id, Barkod = p.Barcode, Urun = p.Name, Kategori = p.Category, Malzeme = p.Material, Raf = p.ShelfLocation, Koli = p.BoxQty, Kritik = p.CriticalStock, Fiyat = p.UnitPrice, Stok = p.StockQty })
                    .ToList();

                var exact = _productService.GetProductByBarcode(term);
                if (exact != null)
                {
                    txtName.Text = exact.Name;
                    txtPrice.Text = exact.UnitPrice.ToString("0.00");

                    // Zücaciye alanlarının yüklenmesi
                    if (cmbCategory != null)
                    {
                        if (cmbCategory.Items.Contains(exact.Category)) cmbCategory.SelectedItem = exact.Category;
                        else cmbCategory.SelectedIndex = 0;
                    }
                    if (cmbMaterial != null)
                    {
                        if (cmbMaterial.Items.Contains(exact.Material)) cmbMaterial.SelectedItem = exact.Material;
                        else cmbMaterial.SelectedIndex = 0;
                    }
                    if (txtShelfLocation != null) txtShelfLocation.Text = exact.ShelfLocation ?? "";
                    if (nudBoxQty != null) nudBoxQty.Value = exact.BoxQty > 0 ? exact.BoxQty : 1;
                    if (nudCriticalStock != null) nudCriticalStock.Value = exact.CriticalStock >= 0 ? (decimal)exact.CriticalStock : 5;
                }
            }
        }

        private void DgvProducts_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dgvProducts.Rows[e.RowIndex].Cells["Stok"].Value != null)
            {
                int stock = Convert.ToInt32(dgvProducts.Rows[e.RowIndex].Cells["Stok"].Value);
                int critical = 5;
                if (dgvProducts.Rows[e.RowIndex].Cells["Kritik"].Value != null)
                {
                    critical = Convert.ToInt32(dgvProducts.Rows[e.RowIndex].Cells["Kritik"].Value);
                }
                if (stock < critical)
                {
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor = UiTheme.DangerSoft;
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.ForeColor = UiTheme.Danger;
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 254);
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.SelectionForeColor = UiTheme.TextPrimary;
                }
                else
                {
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                        e.RowIndex % 2 == 0 ? UiTheme.Surface : UiTheme.GridRowAlt;
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.ForeColor = UiTheme.TextPrimary;
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = UiTheme.CardLavender;
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.SelectionForeColor = UiTheme.TextPrimary;
                }
            }
        }

        private void RefreshAll()
        {
            // EF DbContext cache'ini temizleyip veritabanından en güncel (raw sql ile güncellenmiş) verileri çekmesi için yeniden yükle
            _context?.Dispose();
            _context = new WarehouseContext();
            _productService = new ProductService(_context);
            _dashboardService = new DashboardService(_context);

            // DataGridView güncellemesi
            IEnumerable<Product> query = _productService.GetAllActiveProducts();
            if (_selectedCategoryFilter != "Tümü")
            {
                query = query.Where(p => p.Category == _selectedCategoryFilter);
            }

            dgvProducts.DataSource = query
                .OrderBy(p => p.Id)
                .Select(p => new { No = p.Id, Barkod = p.Barcode, Urun = p.Name, Kategori = p.Category, Malzeme = p.Material, Raf = p.ShelfLocation, Koli = p.BoxQty, Kritik = p.CriticalStock, Fiyat = p.UnitPrice, Stok = p.StockQty })
                .ToList();
            
            dgvMovements.DataSource = _context.StockMovements
                .OrderByDescending(m => m.Id)
                .Take(100)
                .Select(m => new
                {
                    Barkod     = m.BarcodeSnapshot,
                    İşlemTipi  = m.Type,
                    Miktar     = m.Quantity,
                    Neden      = m.Reason,
                    Tarih      = m.CreatedAt
                })
                .ToList();

            // Sütun genişliklerinin tüm boşlukları kaplayacak şekilde esnemesi
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMovements.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Kart güncellemesi
            if (lblTotalProductsVal != null) lblTotalProductsVal.Text = _dashboardService.GetTotalActiveProducts().ToString();
            if (lblDailySalesVal != null) lblDailySalesVal.Text = _dashboardService.GetTodaySalesTotal().ToString("C2");
            if (lblPendingBalanceVal != null) lblPendingBalanceVal.Text = _dashboardService.GetTotalPendingBalance().ToString("C2");
            
            if (lblLowStockVal != null) 
            {
                var lowStockProducts = _productService.GetAllActiveProducts().Where(p => p.StockQty < p.CriticalStock).ToList();
                int lStock = lowStockProducts.Count;
                if (lStock > 0)
                {
                    var names = string.Join(", ", lowStockProducts.Select(p => p.Name).Take(3));
                    if (lStock > 3) names += "...";
                    lblLowStockVal.Text = $"{lStock} Adet\n({names})";
                    lblLowStockVal.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);
                }
                else
                {
                    lblLowStockVal.Text = "0";
                    lblLowStockVal.Font = new System.Drawing.Font("Segoe UI", 24, FontStyle.Bold);
                }
            }

            // Grafik Güncellemesi
            if (chartTopSellers != null)
            {
                var dt = _dashboardService.GetTopSellingProducts(5);
                chartTopSellers.Series["Sales"].Points.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    int index = chartTopSellers.Series["Sales"].Points.AddXY(row["Name"], row["TotalQty"]);
                    chartTopSellers.Series["Sales"].Points[index].Color = UiTheme.Primary;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var barcode = txtBarcode.Text.Trim();
            var name = txtName.Text.Trim();
            var priceText = txtPrice.Text.Trim();

            if (string.IsNullOrWhiteSpace(barcode) || string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Barkod ve ürün adı zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(priceText, out var price) || price < 0)
            {
                MessageBox.Show("Geçerli bir birim fiyat girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string category = cmbCategory != null ? (cmbCategory.SelectedItem?.ToString() ?? "") : "";
            string material = cmbMaterial != null ? (cmbMaterial.SelectedItem?.ToString() ?? "") : "";
            string shelfLocation = txtShelfLocation != null ? txtShelfLocation.Text.Trim() : "";
            int boxQty = nudBoxQty != null ? (int)nudBoxQty.Value : 1;
            int criticalStock = nudCriticalStock != null ? (int)nudCriticalStock.Value : 5;

            var existingProduct = _productService.GetProductByBarcode(barcode);
            if (existingProduct != null)
            {
                existingProduct.Name = name;
                existingProduct.UnitPrice = (double)price;
                existingProduct.Category = category;
                existingProduct.Material = material;
                existingProduct.ShelfLocation = shelfLocation;
                existingProduct.BoxQty = boxQty;
                existingProduct.CriticalStock = criticalStock;

                _productService.UpdateProduct(existingProduct);
                MessageBox.Show("Mevcut ürün sistemde bulundu ve bilgileri başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshAll();
                
                txtBarcode.Clear();
                txtName.Clear();
                txtPrice.Clear();
                if (txtInitialStock != null) txtInitialStock.Clear();
                if (txtShelfLocation != null) txtShelfLocation.Clear();
                if (cmbCategory != null) cmbCategory.SelectedIndex = 0;
                if (cmbMaterial != null) cmbMaterial.SelectedIndex = 0;
                if (nudBoxQty != null) nudBoxQty.Value = 1;
                if (nudCriticalStock != null) nudCriticalStock.Value = 5;

                txtBarcode.Focus();
                return;
            }

            var p = new Product 
            { 
                Barcode = barcode, 
                Name = name, 
                UnitPrice = (double)price,
                Category = category,
                Material = material,
                ShelfLocation = shelfLocation,
                BoxQty = boxQty,
                CriticalStock = criticalStock
            };
            _productService.AddProduct(p);
            
            // Eğer başlangıç stoğu girilmişse, otomatik hareket ekle
            if (txtInitialStock != null && int.TryParse(txtInitialStock.Text.Trim(), out int initialQty) && initialQty > 0)
            {
                p = _productService.GetProductByBarcode(barcode); // ID'sini almak için tekrar çekiyoruz
                p.StockQty = initialQty;
                _productService.UpdateProduct(p);
                
                _context.StockMovements.Add(new StockMovement
                {
                    ProductId = p.Id,
                    BarcodeSnapshot = barcode,
                    Quantity = initialQty,
                    Type = "Giriş",
                    Reason = "Açılış Stoğu",
                    RefType = "Manual",
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = Session.UserId
                });
                _context.SaveChanges();
            }
            
            // Metin kutularını hemen temizle ve Grid'i yenile
            txtBarcode.Clear();
            txtName.Clear();
            txtPrice.Clear();
            if (txtInitialStock != null) txtInitialStock.Clear();
            if (txtShelfLocation != null) txtShelfLocation.Clear();
            if (cmbCategory != null) cmbCategory.SelectedIndex = 0;
            if (cmbMaterial != null) cmbMaterial.SelectedIndex = 0;
            if (nudBoxQty != null) nudBoxQty.Value = 1;
            if (nudCriticalStock != null) nudCriticalStock.Value = 5;
            
            RefreshAll();
            txtBarcode.Focus();

            // Zen.Barcode Yazdırma ve Modern Önizleme Modülü
            try
            {
                Zen.Barcode.Code128BarcodeDraw br = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                System.Drawing.Image img = br.Draw(barcode, 60);

                using (var f = new FrmBarcodePreview(barcode, name, priceText, img))
                {
                    f.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Barkod gösteriminde hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnAddMovement_Click(object sender, EventArgs e)
        {
            var barcode = txtBarcodeMovement.Text.Trim();
            var prod = _productService.GetProductByBarcode(barcode);
            if (prod == null)
            {
                MessageBox.Show("Bu barkoda sahip bir ürün bulunamadı. Önce ürün kartını oluşturun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int qty = (int)nudQuantity.Value;
            string type = cmbType.SelectedItem?.ToString() ?? "Giriş";

            if (type == "Çıkış")
            {
                if (prod.StockQty < qty)
                {
                    MessageBox.Show("Yetersiz stok.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                prod.StockQty -= qty;
            }
            else
            {
                prod.StockQty += qty;
            }

            _productService.UpdateProduct(prod);

            int storedQty = (type == "Çıkış") ? -qty : qty;
            string reason = cmbReason != null ? (cmbReason.SelectedItem?.ToString() ?? ("Manuel " + type)) : ("Manuel " + type);

            _context.StockMovements.Add(new StockMovement
            {
                ProductId        = prod.Id,
                BarcodeSnapshot  = barcode,
                Quantity         = storedQty,
                Type             = type,
                Reason           = reason,
                RefType          = "Manual",
                CreatedAt        = DateTime.Now,
                CreatedByUserId  = Session.UserId
            });
            _context.SaveChanges();

            // Log ekleme
            var ls = new LogService(_context);
            ls.Info("Stok Hareketi: " + type, $"Barkod: {barcode}, Miktar: {qty}, Neden: {reason}", Session.UserId);

            RefreshAll();
            txtBarcodeMovement.Clear();
            nudQuantity.Value = 1;
            if (cmbReason != null) cmbReason.SelectedIndex = 4;
            txtBarcodeMovement.Focus();
        }

        private void BtnPos_Click(object sender, EventArgs e) => OpenChildPage("Satış / POS", new FrmPos());
        private void BtnReturns_Click(object sender, EventArgs e) => OpenChildPage("İade / İptal", new FrmReturns());
        private void BtnCustomers_Click(object sender, EventArgs e) => OpenChildPage("Müşteriler", new FrmCustomers());
        private void BtnReports_Click(object sender, EventArgs e) => OpenChildPage("Raporlar", new FrmReports());

        private async void CheckForUpdatesOnStartup()
        {
            try
            {
                var updateInfo = await AutoUpdater.CheckForUpdatesAsync();
                if (updateInfo != null && updateInfo.IsUpdateAvailable)
                {
                    string notes = string.IsNullOrWhiteSpace(updateInfo.ReleaseNotes) ? "Detay belirtilmemiş." : updateInfo.ReleaseNotes;
                    string msg = $"Poseidon için yeni bir güncelleme mevcut!\n\n" +
                                 $"Mevcut Sürüm: v{updateInfo.CurrentVersion}\n" +
                                 $"Yeni Sürüm: v{updateInfo.LatestVersion}\n\n" +
                                 $"Güncelleme Notları:\n{notes}\n\n" +
                                 $"Güncelleme dosyasını şimdi indirip kurmak istiyor musunuz?";

                    if (MessageBox.Show(msg, "Yeni Güncelleme Mevcut", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        var dlForm = new FrmUpdateDownload(updateInfo.DownloadUrl, updateInfo.LatestVersion);
                        dlForm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Startup update check failed: " + ex.Message);
            }
        }

        private void InitializeGlasswareControls()
        {
            // ─────────────────────────────────────────────────────────────────
            // LAYOUT:
            // Sol sütun (X=18..370): Barkod, Ürün Adı, Fiyat, Başl.Stok (Designer'dan)
            // Orta sütun (X=390..590): btnAdd, btnPos, btnReturns (Designer'dan)
            //   → Bunları sağa kaydırıyoruz (X=390 → 830)
            // Sağ sütun (X=610..810): btnCustomers, btnReports (Designer'dan)
            //   → Bunları da sağa kaydırıyoruz (X=610 → 870)
            // Yeni sağ sütun (X=390..600): Zücaciye alanları — ÜST ÜSTE BİNMEZ
            // ─────────────────────────────────────────────────────────────────

            // ── Mevcut butonları sağa taşı (zücaciye alanlarına yer aç) ──
            if (btnAdd != null)
            {
                btnAdd.Location = new Point(830, 18);
                btnAdd.Size = new Size(150, 38);
            }
            if (btnPos != null)
            {
                btnPos.Location = new Point(830, 62);
                btnPos.Size = new Size(150, 38);
            }
            if (btnReturns != null)
            {
                btnReturns.Location = new Point(830, 106);
                btnReturns.Size = new Size(150, 38);
            }
            if (btnCustomers != null)
            {
                btnCustomers.Location = new Point(830, 150);
                btnCustomers.Size = new Size(150, 38);
            }
            if (btnReports != null)
            {
                btnReports.Location = new Point(830, 194);
                btnReports.Size = new Size(150, 38);
            }

            // ── HATAY METAL Bölüm Başlığı ──
            var lblGlassSection = new Label
            {
                Text = "── HATAY METAL Bilgileri ──",
                Font = new System.Drawing.Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = UiTheme.Primary,
                Location = new Point(390, 5),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Row Y positions (compact: 26, 60, 94, 128)
            int colLbl = 390, colCtrl = 480;
            int row1 = 24, row2 = 58, row3 = 92, row4 = 150;
            int ctrlH = 32;

            // ── Kategori ──
            var lblCategory = new Label
            {
                Text = "Kategori:",
                Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(colLbl, row1 + 6),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            cmbCategory = new Guna2ComboBox
            {
                Location = new Point(colCtrl, row1),
                Size = new Size(185, ctrlH),
                BorderRadius = 8,
                FillColor = UiTheme.InputFill,
                ForeColor = UiTheme.TextPrimary,
                BorderColor = UiTheme.InputBorder,
                Font = new System.Drawing.Font("Segoe UI", 9.5F)
            };
            cmbCategory.Items.AddRange(new object[] { "Yemek Takımı", "Bardak/Kadeh", "Tencere/Tava", "Çatal Bıçak", "Dekorasyon/Aksesuar", "Diğer" });
            cmbCategory.SelectedIndex = 0;

            // ── Malzeme ──
            var lblMaterial = new Label
            {
                Text = "Malzeme:",
                Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(colLbl, row2 + 6),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            cmbMaterial = new Guna2ComboBox
            {
                Location = new Point(colCtrl, row2),
                Size = new Size(185, ctrlH),
                BorderRadius = 8,
                FillColor = UiTheme.InputFill,
                ForeColor = UiTheme.TextPrimary,
                BorderColor = UiTheme.InputBorder,
                Font = new System.Drawing.Font("Segoe UI", 9.5F)
            };
            cmbMaterial.Items.AddRange(new object[] { "Cam", "Porselen", "Seramik", "Çelik", "Ahşap", "Melamin", "Diğer" });
            cmbMaterial.SelectedIndex = 0;

            // ── Raf Konumu ──
            var lblShelfLocation = new Label
            {
                Text = "Raf Konumu:",
                Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(colLbl, row3 + 6),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            txtShelfLocation = new Guna2TextBox
            {
                Location = new Point(colCtrl, row3),
                Size = new Size(185, ctrlH),
                BorderRadius = 8,
                FillColor = UiTheme.InputFill,
                ForeColor = UiTheme.TextPrimary,
                BorderColor = UiTheme.InputBorder,
                Font = new System.Drawing.Font("Segoe UI", 9.5F),
                PlaceholderText = "Örn: A-12"
            };

            // ── Koli Adedi & Kritik Stok (yan yana) ──
            var lblBoxQty = new Label
            {
                Text = "Koli Adedi:",
                Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(colLbl, row4 + 5),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            nudBoxQty = new NumericUpDown
            {
                Location = new Point(colCtrl, row4),
                Size = new Size(60, ctrlH),
                Font = new System.Drawing.Font("Segoe UI", 9.5F),
                BackColor = UiTheme.InputFill,
                ForeColor = UiTheme.TextPrimary,
                Minimum = 1,
                Maximum = 1000,
                Value = 1
            };

            var lblCriticalStock = new Label
            {
                Text = "Kritik Stok:",
                Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UiTheme.Danger,
                Location = new Point(colCtrl + 70, row4 + 5),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            nudCriticalStock = new NumericUpDown
            {
                Location = new Point(colCtrl + 155, row4),
                Size = new Size(60, ctrlH),
                Font = new System.Drawing.Font("Segoe UI", 9.5F),
                BackColor = UiTheme.DangerSoft,
                ForeColor = UiTheme.Danger,
                Minimum = 0,
                Maximum = 10000,
                Value = 5
            };

            // ── Hepsini tabProducts'a ekle ──
            tabProducts.Controls.Add(lblGlassSection);
            tabProducts.Controls.Add(lblCategory);
            tabProducts.Controls.Add(cmbCategory);
            tabProducts.Controls.Add(lblMaterial);
            tabProducts.Controls.Add(cmbMaterial);
            tabProducts.Controls.Add(lblShelfLocation);
            tabProducts.Controls.Add(txtShelfLocation);
            tabProducts.Controls.Add(lblBoxQty);
            tabProducts.Controls.Add(nudBoxQty);
            tabProducts.Controls.Add(lblCriticalStock);
            tabProducts.Controls.Add(nudCriticalStock);

            // Kontrolleri en üste getir (diğer kontrollerle binişmesin diye)
            foreach (Control c in new Control[] { lblGlassSection, cmbCategory, cmbMaterial, txtShelfLocation, nudBoxQty, nudCriticalStock })
                c.BringToFront();

            // ── Kategori Filtreleme Sekmeleri (dgvProducts Üzerinde) ──
            _flpCategoryTabs = new FlowLayoutPanel
            {
                Location = new Point(18, 218),
                Size = new Size(1050, 38),
                BackColor = Color.Transparent,
                WrapContents = false,
                AutoScroll = true
            };

            // RefreshCategoriesUI() will populate cmbCategory and _flpCategoryTabs
            RefreshCategoriesUI();

            tabProducts.Controls.Add(_flpCategoryTabs);
            _flpCategoryTabs.BringToFront();
        }

        public void RefreshCategoriesUI()
        {
            if (cmbCategory == null || _flpCategoryTabs == null) return;

            using (var ctx = new Barcoded_Warehouse_Stock_Tracking.DataAccess.WarehouseContext())
            {
                var cats = ctx.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToList();
                
                string currentSelection = cmbCategory.SelectedItem?.ToString();
                cmbCategory.Items.Clear();
                foreach (var c in cats) cmbCategory.Items.Add(c);
                if (cats.Contains(currentSelection)) cmbCategory.SelectedItem = currentSelection;
                else if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;

                _flpCategoryTabs.Controls.Clear();
                _categoryButtons.Clear();

                var tabCats = new System.Collections.Generic.List<string> { "Tümü" };
                tabCats.AddRange(cats);

                foreach (var cat in tabCats)
                {
                    var btn = new Guna.UI2.WinForms.Guna2Button
                    {
                        Text = cat,
                        Size = new Size(140, 32),
                        BorderRadius = 16,
                        Margin = new Padding(0, 0, 8, 0),
                        Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold),
                        Cursor = Cursors.Hand,
                        Tag = cat
                    };

                    btn.FillColor = cat == _selectedCategoryFilter ? UiTheme.Primary : UiTheme.GridHeaderBg;
                    btn.ForeColor = cat == _selectedCategoryFilter ? Color.White : UiTheme.TextMuted;

                    btn.Click += (s, ev) =>
                    {
                        _selectedCategoryFilter = cat;
                        foreach (var b in _categoryButtons)
                        {
                            b.FillColor = UiTheme.GridHeaderBg;
                            b.ForeColor = UiTheme.TextMuted;
                        }
                        btn.FillColor = UiTheme.Primary;
                        btn.ForeColor = Color.White;
                        RefreshAll();
                    };

                    _categoryButtons.Add(btn);
                    _flpCategoryTabs.Controls.Add(btn);
                }
        }
            // ─────────────────────────────────────────────────────────────────
            // Stok Hareketleri Sekmesi — "Neden" ComboBox'ı
            // ─────────────────────────────────────────────────────────────────
            if (txtBarcodeMovement != null) txtBarcodeMovement.Width = 200;
            if (lblQuantity != null) lblQuantity.Location = new Point(325, 24);
            if (nudQuantity != null)
            {
                nudQuantity.Location = new Point(380, 18);
                nudQuantity.Width = 70;
            }
            if (lblType != null) lblType.Location = new Point(465, 24);
            if (cmbType != null)
            {
                cmbType.Location = new Point(500, 18);
                cmbType.Width = 95;
            }

            var lblReason = new Label
            {
                Text = "Neden:",
                Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(605, 24),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            cmbReason = new Guna2ComboBox
            {
                Location = new Point(660, 18),
                Size = new Size(145, 36),
                BorderRadius = 8,
                FillColor = UiTheme.InputFill,
                ForeColor = UiTheme.TextPrimary,
                BorderColor = UiTheme.InputBorder,
                Font = new System.Drawing.Font("Segoe UI", 10F)
            };
            cmbReason.Items.AddRange(new object[] { "Kırık/Hasar (Zayiat)", "Satış", "Transfer", "Sayım Farkı", "Manuel Giriş/Çıkış", "Diğer" });
            cmbReason.SelectedIndex = 4;

            if (btnAddMovement != null)
            {
                btnAddMovement.Location = new Point(815, 18);
                btnAddMovement.Width = 180;
            }

            tabMovements.Controls.Add(lblReason);
            tabMovements.Controls.Add(cmbReason);
        }

    }
}
