using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class FrmInitialAdminSetup : Form
    {
        private readonly Guna2TextBox _txtUsername = new Guna2TextBox();
        private readonly Guna2TextBox _txtPassword = new Guna2TextBox();
        private readonly Guna2TextBox _txtPasswordRepeat = new Guna2TextBox();
        private readonly Guna2Button _btnCreate = new Guna2Button();
        private readonly Label _lblError = new Label();

        public string CreatedUsername { get; private set; }

        public FrmInitialAdminSetup()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Poseidon — İlk Yönetici Kurulumu";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.None;
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(460, 560);

            // Arka plan gradient
            this.Paint += (s, pe) =>
            {
                using (var br = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(10, 25, 77),
                    Color.FromArgb(3, 82, 143),
                    150f))
                {
                    pe.Graphics.FillRectangle(br, this.ClientRectangle);
                }
            };

            // Başlık ve Açıklama
            var lblTitle = new Label
            {
                Text = "Yönetici Hesabı Kurulumu",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(460, 36),
                Location = new Point(0, 25),
                BackColor = Color.Transparent
            };

            var lblSubtitle = new Label
            {
                Text = "Sistemde yönetici hesabı bulunmamaktadır.\nLütfen ilk yönetici hesabınızı tanımlayın.",
                ForeColor = Color.FromArgb(180, 210, 255),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(460, 40),
                Location = new Point(0, 65),
                BackColor = Color.Transparent
            };

            // Beyaz Kart
            const int CW = 380, CH = 410;
            const int CX = (460 - CW) / 2;
            const int CY = 120;

            var card = new Guna2Panel
            {
                Size = new Size(CW, CH),
                Location = new Point(CX, CY),
                FillColor = Color.White,
                BorderRadius = 18,
                ShadowDecoration =
                {
                    Enabled = true,
                    Color = Color.FromArgb(90, 0, 20, 80),
                    Depth = 20,
                    BorderRadius = 18,
                    Shadow = new Padding(8)
                }
            };

            const int TW = 300;
            const int TX = (CW - TW) / 2;

            var lblCardHeader = new Label
            {
                Text = "Yeni Yönetici Bilgileri",
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(TW, 30),
                Location = new Point(TX, 20),
                BackColor = Color.Transparent
            };

            var divider = new Label
            {
                BackColor = Color.FromArgb(226, 232, 240),
                Size = new Size(TW, 1),
                Location = new Point(TX, 55)
            };

            // Username
            _txtUsername.Size = new Size(TW, 48);
            _txtUsername.Location = new Point(TX, 70);
            _txtUsername.PlaceholderText = "Yönetici Kullanıcı Adı";
            _txtUsername.BorderRadius = 8;
            _txtUsername.FillColor = Color.FromArgb(241, 245, 249);
            _txtUsername.BorderColor = Color.FromArgb(203, 213, 225);
            _txtUsername.ForeColor = Color.FromArgb(15, 23, 42);
            _txtUsername.Font = new Font("Segoe UI", 9.5f);
            _txtUsername.PlaceholderForeColor = Color.FromArgb(148, 163, 184);
            _txtUsername.TextOffset = new Point(8, 0);

            // Password
            _txtPassword.Size = new Size(TW, 48);
            _txtPassword.Location = new Point(TX, 130);
            _txtPassword.PlaceholderText = "Şifre (En az 8 karakter)";
            _txtPassword.BorderRadius = 8;
            _txtPassword.FillColor = Color.FromArgb(241, 245, 249);
            _txtPassword.BorderColor = Color.FromArgb(203, 213, 225);
            _txtPassword.ForeColor = Color.FromArgb(15, 23, 42);
            _txtPassword.Font = new Font("Segoe UI", 9.5f);
            _txtPassword.UseSystemPasswordChar = true;
            _txtPassword.PlaceholderForeColor = Color.FromArgb(148, 163, 184);
            _txtPassword.TextOffset = new Point(8, 0);

            // Password Repeat
            _txtPasswordRepeat.Size = new Size(TW, 48);
            _txtPasswordRepeat.Location = new Point(TX, 190);
            _txtPasswordRepeat.PlaceholderText = "Şifre Tekrar";
            _txtPasswordRepeat.BorderRadius = 8;
            _txtPasswordRepeat.FillColor = Color.FromArgb(241, 245, 249);
            _txtPasswordRepeat.BorderColor = Color.FromArgb(203, 213, 225);
            _txtPasswordRepeat.ForeColor = Color.FromArgb(15, 23, 42);
            _txtPasswordRepeat.Font = new Font("Segoe UI", 9.5f);
            _txtPasswordRepeat.UseSystemPasswordChar = true;
            _txtPasswordRepeat.PlaceholderForeColor = Color.FromArgb(148, 163, 184);
            _txtPasswordRepeat.TextOffset = new Point(8, 0);

            // Error Label
            _lblError.Size = new Size(TW, 36);
            _lblError.Location = new Point(TX, 246);
            _lblError.ForeColor = UiTheme.Danger;
            _lblError.TextAlign = ContentAlignment.MiddleCenter;
            _lblError.Font = new Font("Segoe UI", 8.5f);
            _lblError.Text = "";
            _lblError.BackColor = Color.Transparent;

            // Submit Button
            _btnCreate.Text = "YÖNETİCİ HESABINI OLUŞTUR";
            _btnCreate.Size = new Size(TW, 50);
            _btnCreate.Location = new Point(TX, 290);
            _btnCreate.BorderRadius = 8;
            _btnCreate.FillColor = UiTheme.SidebarSelected;
            _btnCreate.HoverState.FillColor = Color.FromArgb(2, 110, 170);
            _btnCreate.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            _btnCreate.ForeColor = Color.White;
            _btnCreate.Cursor = Cursors.Hand;
            _btnCreate.Animated = true;
            _btnCreate.Click += BtnCreate_Click;

            // Notice text under button
            var lblHint = new Label
            {
                Text = "Not: Şifreniz en az 8 karakter olmalı ve güvenli tutulmalıdır.",
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 8f),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(TW, 40),
                Location = new Point(TX, 350),
                BackColor = Color.Transparent
            };

            card.Controls.Add(lblCardHeader);
            card.Controls.Add(divider);
            card.Controls.Add(_txtUsername);
            card.Controls.Add(_txtPassword);
            card.Controls.Add(_txtPasswordRepeat);
            card.Controls.Add(_lblError);
            card.Controls.Add(_btnCreate);
            card.Controls.Add(lblHint);

            Controls.Add(card);
            Controls.Add(lblSubtitle);
            Controls.Add(lblTitle);

            AcceptButton = _btnCreate;
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            var username = _txtUsername.Text.Trim();
            var password = _txtPassword.Text;
            var passwordRepeat = _txtPasswordRepeat.Text;

            _lblError.Text = "";

            if (string.IsNullOrWhiteSpace(username))
            {
                _lblError.Text = "Kullanıcı adı boş bırakılamaz.";
                _txtUsername.Focus();
                return;
            }

            if (username.Length < 3)
            {
                _lblError.Text = "Kullanıcı adı en az 3 karakter olmalıdır.";
                _txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                _lblError.Text = "Şifre boş bırakılamaz.";
                _txtPassword.Focus();
                return;
            }

            if (password.Length < 8)
            {
                _lblError.Text = "Şifre en az 8 karakter olmalıdır.";
                _txtPassword.Focus();
                return;
            }

            if (password != passwordRepeat)
            {
                _lblError.Text = "Girilen şifreler eşleşmiyor.";
                _txtPasswordRepeat.Clear();
                _txtPasswordRepeat.Focus();
                return;
            }

            try
            {
                Database.CreateAdminUser(username, password);

                CreatedUsername = username;
                MessageBox.Show(
                    "Yönetici hesabı başarıyla oluşturuldu!\nArtık belirlediğiniz kullanıcı adı ve şifre ile giriş yapabilirsiniz.",
                    "Kurulum Tamamlandı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                // Şifre bilgisi hiçbir log veya hata mesajına yazılmaz
                _lblError.Text = "Hata: " + ex.Message;
            }
        }
    }
}
