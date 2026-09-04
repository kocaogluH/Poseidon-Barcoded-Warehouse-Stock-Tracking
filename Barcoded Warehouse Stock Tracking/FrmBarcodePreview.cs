using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class FrmBarcodePreview : Form
    {
        private readonly string _barcode;
        private readonly string _productName;
        private readonly string _priceText;
        private readonly Image _barcodeImage;
        private Guna2CheckBox chkFragile;

        public FrmBarcodePreview(string barcode, string productName, string priceText, Image barcodeImage)
        {
            _barcode = barcode;
            _productName = productName;
            _priceText = priceText;
            _barcodeImage = barcodeImage;

            // Form Ayarları
            this.Text = "Barkod Etiketi Önizleme";
            this.Size = new Size(420, 410); // Checkbox için boyutu büyüttük
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UiTheme.MainBackground;
            this.AutoScaleMode = AutoScaleMode.None;
            this.Font = new Font("Segoe UI", 9.5f);

            // Üst Başlık Paneli (Modern Mavi)
            var pnlHeader = new Guna2Panel
            {
                Size = new Size(420, 60),
                Location = new Point(0, 0),
                FillColor = UiTheme.Sidebar
            };

            var lblHeaderTitle = new Label
            {
                Text = "🏷️  Barkod Etiketi Önizleme",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Location = new Point(20, 16),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblHeaderTitle);
            this.Controls.Add(pnlHeader);

            // Bilgi Açıklaması
            var lblDesc = new Label
            {
                Text = "Ürün başarıyla eklendi. Otomatik oluşturulan barkod etiketini aşağıda görebilir ve yazdırabilirsiniz.",
                ForeColor = UiTheme.TextMuted,
                Font = new Font("Segoe UI", 9f),
                Location = new Point(20, 75),
                Size = new Size(380, 40),
                TextAlign = ContentAlignment.TopLeft
            };
            this.Controls.Add(lblDesc);

            // Barkod Kartı (Beyaz Yuvarlatılmış Panel)
            var card = new Guna2Panel
            {
                Size = new Size(380, 150),
                Location = new Point(20, 125),
                FillColor = Color.White,
                BorderRadius = 12,
                BorderColor = UiTheme.InputBorder,
                BorderThickness = 1
            };

            // Barkod Görseli
            var pbBarcode = new PictureBox
            {
                Image = _barcodeImage,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Size = new Size(340, 70),
                Location = new Point(20, 15),
                BackColor = Color.White
            };
            card.Controls.Add(pbBarcode);

            // Ürün Bilgisi ve Fiyatı
            var lblInfo = new Label
            {
                Text = $"{_productName} - {_priceText} TL",
                ForeColor = UiTheme.TextPrimary,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(340, 30),
                Location = new Point(20, 95),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblInfo);
            this.Controls.Add(card);

            // Kırılabilir Seçeneği (HATAY METAL Özelleştirmesi)
            chkFragile = new Guna2CheckBox
            {
                Text = "🍷 Kırılabilir Logosu Ekle (Hassas ürün uyarısı basar)",
                Location = new Point(25, 285),
                Size = new Size(370, 30),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = UiTheme.Danger,
                CheckedState = { BorderColor = UiTheme.Danger, FillColor = UiTheme.DangerSoft },
                Visible = FeatureManager.EnableFragileOption
            };
            this.Controls.Add(chkFragile);

            // Yazdır Butonu
            var btnPrint = new Guna2Button
            {
                Text = "🖨️  Etiketi Yazdır",
                Size = new Size(170, 40),
                Location = new Point(30, 325), // Konumu checkbox için aşağı kaydırdık
                BorderRadius = 8,
                FillColor = UiTheme.Success,
                HoverState = { FillColor = ControlPaint.Dark(UiTheme.Success, 0.08f) },
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Animated = true
            };
            btnPrint.Click += BtnPrint_Click;
            this.Controls.Add(btnPrint);

            // Kapat Butonu
            var btnClose = new Guna2Button
            {
                Text = "❌  Kapat",
                Size = new Size(170, 40),
                Location = new Point(220, 325), // Konumu checkbox için aşağı kaydırdık
                BorderRadius = 8,
                FillColor = UiTheme.SidebarHover,
                HoverState = { FillColor = ControlPaint.Dark(UiTheme.SidebarHover, 0.08f) },
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Animated = true
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                bool isFragile = chkFragile != null && chkFragile.Checked;
                var pd = new PrintDocument();
                pd.PrintPage += (s, ev) =>
                {
                    var g = ev.Graphics;
                    using (var fontTitle = new Font("Segoe UI", 12, FontStyle.Bold))
                    using (var fontSub = new Font("Segoe UI", 10, FontStyle.Regular))
                    using (var fontFragile = new Font("Segoe UI", 8, FontStyle.Bold))
                    {
                        string infoText = $"{_productName} - {_priceText} TL";
                        g.DrawString("Poseidon Stok", fontSub, Brushes.Black, new PointF(20, 10));
                        g.DrawImage(_barcodeImage, new Rectangle(20, 30, 260, 60));
                        g.DrawString(infoText, fontTitle, Brushes.Black, new PointF(20, 95));
                        
                        if (isFragile)
                        {
                            g.DrawString("⚠️ FRAGILE / KIRILABİLİR 🍷", fontFragile, Brushes.Red, new PointF(20, 120));
                        }
                    }
                };

                using (var printDlg = new PrintDialog())
                {
                    printDlg.Document = pd;
                    if (printDlg.ShowDialog() == DialogResult.OK)
                    {
                        pd.Print();
                        MessageBox.Show("Barkod başarıyla yazıcıya gönderildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yazdırma sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
