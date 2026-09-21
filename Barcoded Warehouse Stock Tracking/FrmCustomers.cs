using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Guna.UI2.WinForms;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class FrmCustomers : Form
    {
        private static readonly Color BgDark = UiTheme.MainBackground;
        private static readonly Color BgMid = UiTheme.Surface;
        private static readonly Color BgInput = UiTheme.InputFill;
        private static readonly Color Accent = UiTheme.Primary;
        private static readonly Color AccentBlu = UiTheme.PrimaryDark;
        private static readonly Color AccentGrn = UiTheme.Success;
        private static readonly Color TextMain = UiTheme.TextPrimary;
        private static readonly Color TextDim = UiTheme.TextMuted;

        private readonly Guna2TextBox _txtName = new Guna2TextBox();
        private readonly Guna2TextBox _txtPhone = new Guna2TextBox();
        private readonly Guna2TextBox _txtEmail = new Guna2TextBox();
        private readonly Guna2Button _btnAdd = new Guna2Button();
        private readonly Guna2ComboBox _cmbCustomer = new Guna2ComboBox();
        private readonly Guna2ComboBox _cmbMethod = new Guna2ComboBox();
        private readonly Guna2TextBox _txtAmount = new Guna2TextBox();
        private readonly Guna2Button _btnCollect = new Guna2Button();
        private readonly Guna2DataGridView _grid = new Guna2DataGridView();

        // Yardımcı: input stilini uygula
        private void StyleTxt(Guna2TextBox tb, string ph, Color border)
        {
            tb.Dock = DockStyle.Top;
            tb.Height = 42;
            tb.PlaceholderText = ph;
            tb.BorderRadius = 8;
            tb.Font = new Font("Segoe UI", 10);
            tb.FillColor = BgInput;
            tb.BorderColor = UiTheme.InputBorder;
            tb.ForeColor = TextMain;
            tb.PlaceholderForeColor = TextDim;
            tb.Margin = new Padding(0, 0, 0, 10);
        }

        private readonly Guna2ComboBox _cmbSort = new Guna2ComboBox();

        public FrmCustomers()
        {
            Text = "Poseidon Yazılım — Müşteriler / Cari Takibi";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1080; Height = 650;
            BackColor = BgDark;
            DoubleBuffered = true;

            // ── SOL PANEL ────────────────────────────────────────────────────────
            var left = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                BackColor = BgMid,
                Padding = new Padding(25, 20, 25, 20)
            };

            // TableLayoutPanel ile sol paneli dikey olarak düzenle
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 12,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Sabit yükseklikler
            int[] rowHeights = { 35, 52, 52, 52, 52, 20, 35, 52, 52, 52, 20, 20 };
            foreach (var h in rowHeights)
                tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, h));

            // Başlık 1
            var lblSec1 = new Label
            {
                Text = "👤  Yeni Müşteri Ekle",
                ForeColor = Accent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Input'lar
            StyleTxt(_txtName, "Ad Soyad *", Accent);
            StyleTxt(_txtPhone, "Telefon", Accent);
            StyleTxt(_txtEmail, "E-posta", Accent);

            // Müşteri Ekle butonu
            _btnAdd.Text = "＋  Müşteri Ekle";
            _btnAdd.Dock = DockStyle.Fill;
            _btnAdd.BorderRadius = 10;
            _btnAdd.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            _btnAdd.FillColor = AccentGrn; _btnAdd.ForeColor = Color.White;
            _btnAdd.HoverState.FillColor = ControlPaint.Dark(AccentGrn, 0.1f);
            _btnAdd.Click += (_, __) => AddCustomer();

            // Ayraç
            var pnlSep = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            // Başlık 2
            var lblSec2 = new Label
            {
                Text = "💰  Tahsilat İşlemi",
                ForeColor = AccentBlu,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Müşteri seçimi
            _cmbCustomer.Dock = DockStyle.Fill; _cmbCustomer.Height = 42;
            _cmbCustomer.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbCustomer.BorderRadius = 8; _cmbCustomer.FillColor = BgInput;
            _cmbCustomer.BorderColor = UiTheme.InputBorder; _cmbCustomer.ForeColor = TextMain;
            _cmbCustomer.Font = new Font("Segoe UI", 10);
            _cmbCustomer.Margin = new Padding(0, 0, 0, 10);

            // Yöntem + Tutar yan yana
            var pnlMethodRow = new Panel { Dock = DockStyle.Fill };

            _cmbMethod.Location = new Point(0, 0); _cmbMethod.Width = 150; _cmbMethod.Height = 42;
            _cmbMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbMethod.BorderRadius = 8; _cmbMethod.FillColor = BgInput;
            _cmbMethod.BorderColor = UiTheme.InputBorder; _cmbMethod.ForeColor = TextMain;
            _cmbMethod.Font = new Font("Segoe UI", 10);
            _cmbMethod.Items.AddRange(new object[] { "Cash", "Card" });
            _cmbMethod.SelectedIndex = 0;

            _txtAmount.Location = new Point(165, 0); _txtAmount.Width = 170; _txtAmount.Height = 30;
            _txtAmount.PlaceholderText = "Tutar (TL)"; _txtAmount.BorderRadius = 8;
            _txtAmount.FillColor = BgInput; _txtAmount.BorderColor = UiTheme.InputBorder;
            _txtAmount.ForeColor = TextMain; _txtAmount.Font = new Font("Segoe UI", 10);
            _txtAmount.Text = "0";

            pnlMethodRow.Controls.Add(_cmbMethod);
            pnlMethodRow.Controls.Add(_txtAmount);

            // Tahsilat butonu
            _btnCollect.Text = "✓  Tahsilatı Kaydet";
            _btnCollect.Dock = DockStyle.Fill;
            _btnCollect.BorderRadius = 10;
            _btnCollect.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            _btnCollect.FillColor = UiTheme.Primary; _btnCollect.ForeColor = Color.White;
            _btnCollect.HoverState.FillColor = UiTheme.PrimaryDark;
            _btnCollect.Click += (_, __) => AddCollection();

            // Müşteri Silme butonu
            var btnDeleteCustomer = new Guna2Button
            {
                Text = "🗑  Müşteriyi Sil",
                Dock = DockStyle.Fill,
                BorderRadius = 10,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FillColor = UiTheme.Danger,
                ForeColor = Color.White,
                Margin = new Padding(0, 10, 0, 0)
            };
            btnDeleteCustomer.Click += (s, e) =>
            {
                if (_grid.CurrentRow == null) return;
                var row = ((DataRowView)_grid.CurrentRow.DataBoundItem).Row;
                long id = Convert.ToInt64(row["Id"]);
                string name = row["Name"].ToString();

                if (MessageBox.Show($"'{name}' isimli müşteriyi silmek istediğinize emin misiniz?", "Müşteri Sil",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Database.DeleteCustomer(id);
                    LoadData();
                }
            };

            tlp.Controls.Add(lblSec1, 0, 0);
            tlp.Controls.Add(_txtName, 0, 1);
            tlp.Controls.Add(_txtPhone, 0, 2);
            tlp.Controls.Add(_txtEmail, 0, 3);
            tlp.Controls.Add(_btnAdd, 0, 4);
            tlp.Controls.Add(pnlSep, 0, 5);
            tlp.Controls.Add(lblSec2, 0, 6);
            tlp.Controls.Add(_cmbCustomer, 0, 7);
            tlp.Controls.Add(pnlMethodRow, 0, 8);
            tlp.Controls.Add(_btnCollect, 0, 9);
            tlp.Controls.Add(new Panel(), 0, 10);
            tlp.Controls.Add(btnDeleteCustomer, 0, 11);

            left.Controls.Add(tlp);

            // ── SAĞ PANEL (Grid & Filtre) ────────────────────────────────────────
            var center = new Panel { Dock = DockStyle.Fill, BackColor = BgDark, Padding = new Padding(12) };

            // Üst Borç Filtresi Barı
            var pnlSort = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = BgMid,
                Padding = new Padding(10, 5, 10, 5)
            };

            var lblSort = new Label
            {
                Text = "🔍  Müşteri / Borç Sıralaması:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Accent,
                AutoSize = true,
                Location = new Point(10, 12)
            };

            _cmbSort.Location = new Point(220, 6);
            _cmbSort.Size = new Size(240, 34);
            _cmbSort.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbSort.BorderRadius = 6;
            _cmbSort.FillColor = BgInput;
            _cmbSort.BorderColor = UiTheme.InputBorder;
            _cmbSort.ForeColor = TextMain;
            _cmbSort.Font = new Font("Segoe UI", 9.5f);
            _cmbSort.Items.AddRange(new object[] {
                "Tüm Müşteriler (A-Z)",
                "En Çok Borcu Olanlar (Azalan)",
                "En Az Borcu Olanlar (Artan)",
                "Sadece Borçlu Müşteriler (Borç > 0)"
            });
            _cmbSort.SelectedIndex = 0;
            _cmbSort.SelectedIndexChanged += (s, e) => LoadData();

            pnlSort.Controls.Add(lblSort);
            pnlSort.Controls.Add(_cmbSort);

            center.Controls.Add(pnlSort);

            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false; _grid.ReadOnly = true;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            _grid.ThemeStyle.BackColor = UiTheme.Surface;
            _grid.ThemeStyle.GridColor = UiTheme.GridLine;
            
            _grid.ThemeStyle.HeaderStyle.BackColor = UiTheme.GridHeaderBg;
            _grid.ThemeStyle.HeaderStyle.ForeColor = Accent;
            _grid.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            
            _grid.ThemeStyle.RowsStyle.BackColor = UiTheme.Surface;
            _grid.ThemeStyle.RowsStyle.ForeColor = TextMain;
            _grid.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9);
            _grid.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(224, 242, 254);
            _grid.ThemeStyle.RowsStyle.SelectionForeColor = TextMain;

            _grid.BorderStyle = BorderStyle.None;
            _grid.RowHeadersVisible = false;
            _grid.RowTemplate.Height = 35;
            _grid.ColumnHeadersHeight = 40;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            _grid.CellFormatting += Grid_CellFormatting;

            center.Controls.Add(_grid);

            Controls.Add(center);
            Controls.Add(left);

            LoadData();
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name == "Balance" && e.Value != null)
            {
                if (double.TryParse(e.Value.ToString(), out double bal))
                {
                    if (bal > 0)
                    {
                        e.CellStyle.ForeColor = Color.Crimson;
                        e.CellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.SeaGreen;
                    }
                }
            }
        }

        private void LoadData()
        {
            try
            {
                var dt = Database.GetCustomers();
                DataView dv = dt.DefaultView;

                int sortIndex = _cmbSort.SelectedIndex;
                if (sortIndex == 1) // En çok borcu olanlar (azalan)
                {
                    dv.Sort = "Balance DESC";
                }
                else if (sortIndex == 2) // En az borcu olanlar (artan)
                {
                    dv.Sort = "Balance ASC";
                }
                else if (sortIndex == 3) // Sadece borçlu müşteriler (Borç > 0)
                {
                    dv.RowFilter = "Balance > 0";
                    dv.Sort = "Balance DESC";
                }
                else // Tüm Müşteriler (A-Z)
                {
                    dv.Sort = "Name ASC";
                }

                _grid.DataSource = dv.ToTable();
                
                // Başlıkları Türkçeleştir
                if (_grid.Columns.Count > 0)
                {
                    if (_grid.Columns["Name"] != null) _grid.Columns["Name"].HeaderText = "Müşteri Adı";
                    if (_grid.Columns["Phone"] != null) _grid.Columns["Phone"].HeaderText = "Telefon";
                    if (_grid.Columns["Email"] != null) _grid.Columns["Email"].HeaderText = "E-Posta";
                    if (_grid.Columns["Balance"] != null) _grid.Columns["Balance"].HeaderText = "Borç / Bakiye (₺)";
                }

                _cmbCustomer.DataSource = dt.Copy();
                _cmbCustomer.DisplayMember = "Name";
                _cmbCustomer.ValueMember = "Id";
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void AddCustomer()
        {
            var name = _txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;
            Database.InsertCustomer(name, _txtPhone.Text.Trim(), _txtEmail.Text.Trim());
            _txtName.Clear(); _txtPhone.Clear(); _txtEmail.Clear();
            LoadData();
        }

        private void AddCollection()
        {
            if (_cmbCustomer.SelectedValue == null) return;
            if (!double.TryParse(_txtAmount.Text, out double amt) || amt <= 0) return;
            try
            {
                Database.AddCollection((long)_cmbCustomer.SelectedValue, _cmbMethod.Text, amt, Session.UserId ?? 0);
                _txtAmount.Text = "0";
                LoadData();
                MessageBox.Show("Tahsilat kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
