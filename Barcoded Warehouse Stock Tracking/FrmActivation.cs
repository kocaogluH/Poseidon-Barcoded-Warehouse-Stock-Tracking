using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class FrmActivation : Form
    {
        private readonly Guna2TextBox _txtHwid = new Guna2TextBox();
        private readonly Guna2TextBox _txtKey = new Guna2TextBox();
        private readonly Guna2Button _btnActivate = new Guna2Button();
        private readonly Guna2Button _btnCopyHwid = new Guna2Button();

        public FrmActivation()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Lisans Etkinleştirme Sihirbazı";
            this.Size = new Size(500, 490);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UiTheme.MainBackground;
            this.AutoScaleMode = AutoScaleMode.None;
            this.Font = new Font("Segoe UI", 9.5f);

            // ── Gradient Header ──
            this.Paint += (s, pe) =>
            {
                using (var br = new LinearGradientBrush(
                    new Rectangle(0, 0, this.ClientSize.Width, 85),
                    Color.FromArgb(10, 25, 77),
                    Color.FromArgb(3, 82, 143),
                    0f))
                {
                    pe.Graphics.FillRectangle(br, new Rectangle(0, 0, this.ClientSize.Width, 85));
                }
            };

            var lblTitle = new Label
            {
                Text = "🔐  Yazılım Lisans Etkinleştirme",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                Location = new Point(20, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = "Bu cihaz kilitli yazılımdır. Kullanmak için lisans anahtarı giriniz.",
                ForeColor = Color.FromArgb(180, 210, 255),
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(20, 50),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblSub);

            // ── Cihaz Kodu (HWID) Bölümü ──
            var lblHwidHeader = new Label
            {
                Text = "Bilgisayar Cihaz Kodunuz (HWID):",
                ForeColor = UiTheme.TextPrimary,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(30, 110),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblHwidHeader);

            _txtHwid.Text = LicenseManager.GetHardwareId();
            _txtHwid.ReadOnly = true;
            _txtHwid.Location = new Point(30, 135);
            _txtHwid.Size = new Size(310, 42);
            _txtHwid.BorderRadius = 8;
            _txtHwid.FillColor = Color.FromArgb(241, 245, 249);
            _txtHwid.ForeColor = UiTheme.Primary;
            _txtHwid.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            _txtHwid.TextOffset = new Point(5, 0);
            Controls.Add(_txtHwid);

            _btnCopyHwid.Text = "📋 Kopyala";
            _btnCopyHwid.Location = new Point(350, 135);
            _btnCopyHwid.Size = new Size(115, 42);
            _btnCopyHwid.BorderRadius = 8;
            _btnCopyHwid.FillColor = UiTheme.SidebarSelected;
            _btnCopyHwid.ForeColor = Color.White;
            _btnCopyHwid.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _btnCopyHwid.Cursor = Cursors.Hand;
            _btnCopyHwid.Click += (s, e) =>
            {
                Clipboard.SetText(_txtHwid.Text);
                MessageBox.Show("Cihaz Kodu (HWID) panoya kopyalandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            Controls.Add(_btnCopyHwid);

            // ── Aktivasyon Anahtarı Girişi ──
            var lblKeyHeader = new Label
            {
                Text = "Aktivasyon Anahtarınız:",
                ForeColor = UiTheme.TextPrimary,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(30, 200),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblKeyHeader);

            _txtKey.PlaceholderText = "KEY-XXXX-XXXX-XXXX-XXXX";
            _txtKey.Location = new Point(30, 225);
            _txtKey.Size = new Size(435, 45);
            _txtKey.BorderRadius = 8;
            _txtKey.FillColor = Color.White;
            _txtKey.BorderColor = UiTheme.InputBorder;
            _txtKey.ForeColor = UiTheme.TextPrimary;
            _txtKey.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            _txtKey.PlaceholderForeColor = UiTheme.TextMuted;
            _txtKey.TextOffset = new Point(8, 0);
            Controls.Add(_txtKey);

            // ── Bilgilendirme Kutusu ──
            var pnlInfo = new Guna2Panel
            {
                Location = new Point(30, 290),
                Size = new Size(435, 75),
                FillColor = Color.FromArgb(239, 246, 255),
                BorderColor = Color.FromArgb(191, 219, 254),
                BorderThickness = 1,
                BorderRadius = 10
            };
            Controls.Add(pnlInfo);

            var lblInfoText = new Label
            {
                Text = "💡 Yazılımı etkinleştirmek için yukarıdaki Cihaz Kodunu kopyalayıp geliştiricinize (Halil Kocaoğlu) ileterek Lisans Anahtarınızı alınız.",
                ForeColor = Color.FromArgb(30, 58, 138),
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(12, 12),
                Size = new Size(410, 50),
                BackColor = Color.Transparent
            };
            pnlInfo.Controls.Add(lblInfoText);

            // ── Etkinleştir Butonu ──
            _btnActivate.Text = "🚀  Lisansı Etkinleştir ve Başlat";
            _btnActivate.Location = new Point(30, 385);
            _btnActivate.Size = new Size(435, 45);
            _btnActivate.BorderRadius = 10;
            _btnActivate.FillColor = UiTheme.Success;
            _btnActivate.ForeColor = Color.White;
            _btnActivate.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            _btnActivate.Cursor = Cursors.Hand;
            _btnActivate.Animated = true;
            _btnActivate.Click += BtnActivate_Click;
            Controls.Add(_btnActivate);
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            string enteredKey = _txtKey.Text.Trim();
            if (string.IsNullOrWhiteSpace(enteredKey))
            {
                MessageBox.Show("Lütfen geçerli bir Aktivasyon Anahtarı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (LicenseManager.ValidateKey(enteredKey))
            {
                LicenseManager.SaveLicense(enteredKey);
                MessageBox.Show("Lisansınız başarıyla doğrulandı ve etkinleştirildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Girdiğiniz Aktivasyon Anahtarı bu cihaz için GEÇERSİZDİR!\nLütfen doğru anahtarı girdiğinizden emin olun.", "Lisans Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
