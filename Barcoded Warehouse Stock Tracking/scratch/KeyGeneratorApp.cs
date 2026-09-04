using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace LicenseKeyGenerator
{
    public class KeyGeneratorForm : Form
    {
        private TextBox txtHwid;
        private TextBox txtKey;
        private Button btnGenerate;
        private Button btnCopy;
        private const string SecretSalt = "Poseidon-StockTracking-SecretSalt-2026#Halil";

        public KeyGeneratorForm()
        {
            this.Text = "Poseidon Lisans Anahtarı Üreteci — Halil Kocaoğlu";
            this.Size = new Size(520, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10f);
            this.BackColor = Color.FromArgb(245, 247, 250);

            Label lblHwid = new Label { Text = "Müşterinin Cihaz Kodu (HWID):", Location = new Point(25, 25), AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            txtHwid = new TextBox { Location = new Point(25, 55), Size = new Size(450, 30), Text = "HWID-" };

            btnGenerate = new Button { Text = "🔑  Lisans Anahtarı Üret", Location = new Point(25, 95), Size = new Size(450, 42), BackColor = Color.FromArgb(13, 148, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold) };
            btnGenerate.Click += BtnGenerate_Click;

            Label lblKey = new Label { Text = "Müşteriye Gönderilecek Aktivasyon Anahtarı:", Location = new Point(25, 150), AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            txtKey = new TextBox { Location = new Point(25, 180), Size = new Size(320, 30), ReadOnly = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42) };

            btnCopy = new Button { Text = "📋 Kopyala", Location = new Point(355, 178), Size = new Size(120, 34), BackColor = Color.FromArgb(30, 58, 138), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            btnCopy.Click += (s, e) => {
                if (!string.IsNullOrWhiteSpace(txtKey.Text))
                {
                    Clipboard.SetText(txtKey.Text);
                    MessageBox.Show("Aktivasyon Anahtarı kopyalandı! Müşterinize WhatsApp'tan gönderebilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            Controls.Add(lblHwid);
            Controls.Add(txtHwid);
            Controls.Add(btnGenerate);
            Controls.Add(lblKey);
            Controls.Add(txtKey);
            Controls.Add(btnCopy);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            string hwid = txtHwid.Text.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(hwid) || !hwid.StartsWith("HWID-"))
            {
                MessageBox.Show("Lütfen geçerli bir Cihaz Kodu (HWID-...) giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(hwid + SecretSalt));
                string rawHash = BitConverter.ToString(bytes).Replace("-", "").Substring(0, 16);
                txtKey.Text = $"KEY-{rawHash.Substring(0, 4)}-{rawHash.Substring(4, 4)}-{rawHash.Substring(8, 4)}-{rawHash.Substring(12, 4)}";
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new KeyGeneratorForm());
        }
    }
}
