using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class FrmChangePassword : Form
    {
        private long _userId;
        private Guna2TextBox txtNewPassword;
        private Guna2TextBox txtConfirmPassword;
        private Guna2Button btnSave;

        public FrmChangePassword(long userId)
        {
            _userId = userId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Zorunlu Şifre Değiştirme";
            this.ClientSize = new Size(400, 280);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = UiTheme.MainBackground;

            var lblInfo = new Label
            {
                Text = "Güvenliğiniz için lütfen en az 8 karakterli yeni bir şifre belirleyin.",
                Location = new Point(25, 20),
                Size = new Size(350, 42),
                ForeColor = UiTheme.Danger,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            this.Controls.Add(lblInfo);

            txtNewPassword = new Guna2TextBox
            {
                PlaceholderText = "Yeni Şifre (En az 8 karakter)",
                UseSystemPasswordChar = true,
                Location = new Point(25, 75),
                Size = new Size(350, 42),
                BorderRadius = 8,
                FillColor = Color.FromArgb(241, 245, 249),
                BorderColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 9.5f),
                TextOffset = new Point(6, 0)
            };
            this.Controls.Add(txtNewPassword);

            txtConfirmPassword = new Guna2TextBox
            {
                PlaceholderText = "Yeni Şifre (Tekrar)",
                UseSystemPasswordChar = true,
                Location = new Point(25, 130),
                Size = new Size(350, 42),
                BorderRadius = 8,
                FillColor = Color.FromArgb(241, 245, 249),
                BorderColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 9.5f),
                TextOffset = new Point(6, 0)
            };
            this.Controls.Add(txtConfirmPassword);

            btnSave = new Guna2Button
            {
                Text = "Şifreyi Güncelle ve Devam Et",
                Location = new Point(25, 195),
                Size = new Size(350, 46),
                BorderRadius = 8,
                FillColor = UiTheme.SidebarSelected,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnSave.HoverState.FillColor = Color.FromArgb(2, 110, 170);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            this.AcceptButton = btnSave;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var p1 = txtNewPassword.Text;
            var p2 = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(p1) || p1.Length < 8)
            {
                MessageBox.Show("Şifre en az 8 karakter olmalıdır.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (p1 != p2)
            {
                MessageBox.Show("Şifreler birbiriyle eşleşmiyor.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Database.UpdatePassword(_userId, p1);
                MessageBox.Show("Şifreniz başarıyla değiştirildi. Yeni şifrenizle devam edebilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
