using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class FrmUpdateDownload : Form
    {
        private readonly string _downloadUrl;
        private readonly string _sha256DownloadUrl;
        private readonly string _expectedSha256Hash;
        private readonly string _latestVersion;
        private readonly string _tempDir;
        private readonly string _tempFilePath;

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _isCompletedOrCancelled = false;

        private readonly Guna2ProgressBar _progressBar = new Guna2ProgressBar();
        private readonly Label _lblStatus = new Label();
        private readonly Label _lblProgressPercentage = new Label();
        private readonly Guna2Button _btnCancel = new Guna2Button();

        public FrmUpdateDownload(string downloadUrl, string sha256DownloadUrl, string latestVersion)
            : this(downloadUrl, sha256DownloadUrl, null, latestVersion)
        {
        }

        public FrmUpdateDownload(string downloadUrl, string sha256DownloadUrl, string expectedSha256Hash, string latestVersion)
        {
            _downloadUrl = downloadUrl;
            _sha256DownloadUrl = sha256DownloadUrl;
            _expectedSha256Hash = expectedSha256Hash;
            _latestVersion = latestVersion;

            // Kural: %TEMP% altında rastgele (Guid) adlı yeni bir alt klasöre indir
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            _tempFilePath = Path.Combine(_tempDir, "Poseidon_Setup_v" + latestVersion + ".exe");

            // ── Form Kurulumu ──────────────────────────────────────────────────
            this.Text = "Poseidon Güncelleme Sihirbazı";
            this.Size = new Size(420, 260);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UiTheme.MainBackground;
            this.AutoScaleMode = AutoScaleMode.None;
            this.Font = new Font("Segoe UI", 9.5f);

            // ── Gradient Başlık Arka Planı ──
            this.Paint += (s, pe) =>
            {
                using (var br = new LinearGradientBrush(
                    new Rectangle(0, 0, this.ClientSize.Width, 70),
                    Color.FromArgb(10, 25, 77),
                    Color.FromArgb(3, 82, 143),
                    0f))
                {
                    pe.Graphics.FillRectangle(br, new Rectangle(0, 0, this.ClientSize.Width, 70));
                }
            };

            // ── Başlık Etiketi ──
            var lblTitle = new Label
            {
                Text = "🚀  Sistem Güncelleniyor",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblTitle);

            // ── Durum Açıklaması ──
            _lblStatus.Text = "Güvenlik ve doğrulama denetimi yapılıyor...";
            _lblStatus.ForeColor = UiTheme.TextPrimary;
            _lblStatus.Font = new Font("Segoe UI", 9.5f);
            _lblStatus.Location = new Point(25, 90);
            _lblStatus.Size = new Size(370, 20);
            _lblStatus.BackColor = Color.Transparent;
            Controls.Add(_lblStatus);

            // ── İlerleme Çubuğu (Guna2ProgressBar) ──
            _progressBar.Size = new Size(370, 20);
            _progressBar.Location = new Point(25, 120);
            _progressBar.BorderRadius = 10;
            _progressBar.FillColor = Color.FromArgb(226, 232, 240);
            _progressBar.ProgressColor = UiTheme.Primary;
            _progressBar.ProgressColor2 = Color.FromArgb(2, 132, 199);
            _progressBar.Minimum = 0;
            _progressBar.Maximum = 100;
            _progressBar.Value = 0;
            Controls.Add(_progressBar);

            // ── Yüzde Göstergesi ──
            _lblProgressPercentage.Text = "%0 tamamlandı (0 MB / 0 MB)";
            _lblProgressPercentage.ForeColor = UiTheme.TextMuted;
            _lblProgressPercentage.Font = new Font("Segoe UI", 8.5f);
            _lblProgressPercentage.Location = new Point(25, 145);
            _lblProgressPercentage.Size = new Size(370, 20);
            _lblProgressPercentage.BackColor = Color.Transparent;
            Controls.Add(_lblProgressPercentage);

            // ── İptal Butonu ──
            _btnCancel.Text = "İptal Et";
            _btnCancel.Size = new Size(120, 36);
            _btnCancel.Location = new Point(275, 190);
            _btnCancel.BorderRadius = 8;
            _btnCancel.FillColor = UiTheme.Danger;
            _btnCancel.HoverState.FillColor = ControlPaint.Dark(UiTheme.Danger, 0.08f);
            _btnCancel.ForeColor = Color.White;
            _btnCancel.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnCancel.Cursor = Cursors.Hand;
            _btnCancel.Animated = true;
            _btnCancel.Click += BtnCancel_Click;
            Controls.Add(_btnCancel);
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await StartUpdateProcessAsync();
        }

        private async Task StartUpdateProcessAsync()
        {
            try
            {
                if (!Directory.Exists(_tempDir))
                {
                    Directory.CreateDirectory(_tempDir);
                }

                // 1. ADIM: SHA256 bütünlük doğrulama anahtarını temin et
                _lblStatus.Text = "Bütünlük doğrulama anahtarı alınıyor...";
                string expectedHash = _expectedSha256Hash;

                if (string.IsNullOrWhiteSpace(expectedHash) && !string.IsNullOrWhiteSpace(_sha256DownloadUrl))
                {
                    try
                    {
                        string sha256Content = await Task.Run(() => AutoUpdater.DownloadStringWithDomainCheck(_sha256DownloadUrl));
                        expectedHash = AutoUpdater.ExtractSha256Hash(sha256Content);
                    }
                    catch (Exception ex)
                    {
                        Program.AppendErrorLog("SHA256 kontrol dosyası indirilemedi: " + ex.Message);
                    }
                }

                if (string.IsNullOrWhiteSpace(expectedHash))
                {
                    CleanupTempFiles();
                    Program.AppendErrorLog("Doğrulama değeri bulunamadı (SHA256 hash anahtarı eksik veya geçersiz).");
                    MessageBox.Show("Güncelleme doğrulanamadı (Bütünlük doğrulama anahtarı eksik veya geçersiz).", "Güvenlik Uyarısı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }

                // 2. ADIM: Kurulum dosyasını indir
                _lblStatus.Text = "Güncelleme paketi indiriliyor...";
                await DownloadSetupFileAsync(_downloadUrl, _tempFilePath, _cts.Token);

                if (_cts.IsCancellationRequested)
                {
                    CleanupTempFiles();
                    return;
                }

                // 3. ADIM: SHA256 bütünlük hesaplama ve doğrulama
                _lblStatus.Text = "Dosya bütünlüğü (SHA256) doğrulanıyor...";
                string computedHash = await Task.Run(() => AutoUpdater.ComputeFileSha256(_tempFilePath));

                bool isHashValid = AutoUpdater.FixedTimeEquals(computedHash, expectedHash);

                if (!isHashValid)
                {
                    CleanupTempFiles();
                    Program.AppendErrorLog("Güncelleme paketi bütünlük doğrulama hatası: SHA256 hash uyuşmazlığı. Beklenen ve hesaplanan hash eşleşmedi.");
                    MessageBox.Show("İndirilen güncelleme dosyasının bütünlük doğrulaması (SHA256) başarısız oldu.\nKurulum güvenlik nedeniyle iptal edildi.",
                        "Bütünlük Doğrulama Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                // 4. ADIM: Doğrulama başarılı -> Hemen Process.Start ve Application.Exit
                _isCompletedOrCancelled = true;
                _lblStatus.Text = "Kurulum başlatılıyor...";
                _progressBar.Value = 100;
                _progressBar.ProgressColor = UiTheme.Success;
                _progressBar.ProgressColor2 = Color.FromArgb(4, 120, 87);

                Process.Start(_tempFilePath);
                Application.Exit();
            }
            catch (OperationCanceledException)
            {
                CleanupTempFiles();
                MessageBox.Show("Güncelleme indirme işlemi iptal edildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (SecurityException secEx)
            {
                CleanupTempFiles();
                Program.AppendErrorLog("Güvenlik kısıtlaması ihlali: " + secEx.Message);
                MessageBox.Show("Güncelleme güvenlik denetiminden geçemedi:\n" + secEx.Message, "Güvenlik Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
            catch (Exception ex)
            {
                CleanupTempFiles();
                Program.AppendErrorLog("Güncelleme indirme/kurulum hatası: " + ex.Message);
                MessageBox.Show("İndirme sırasında bir hata oluştu:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private async Task DownloadSetupFileAsync(string url, string destinationPath, CancellationToken cancellationToken)
        {
            if (!AutoUpdater.IsAllowedGitHubUrl(url))
            {
                throw new SecurityException("İzin verilmeyen indirme adresi: " + url);
            }

            string currentUrl = url;
            int redirectCount = 0;

            while (redirectCount < 5)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var request = (HttpWebRequest)WebRequest.Create(currentUrl);
                request.UserAgent = "Poseidon-Stock-Tracking-Updater";
                request.Timeout = 15000;
                request.ReadWriteTimeout = 30000;
                request.AllowAutoRedirect = false;

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.MovedPermanently ||
                        response.StatusCode == HttpStatusCode.Found ||
                        response.StatusCode == HttpStatusCode.SeeOther ||
                        response.StatusCode == HttpStatusCode.TemporaryRedirect ||
                        (int)response.StatusCode == 308)
                    {
                        string location = response.Headers["Location"];
                        if (string.IsNullOrWhiteSpace(location))
                        {
                            throw new WebException("Yönlendirme adresi bulunamadı.");
                        }

                        Uri nextUri = Uri.IsWellFormedUriString(location, UriKind.Absolute)
                            ? new Uri(location)
                            : new Uri(new Uri(currentUrl), location);

                        if (!AutoUpdater.IsAllowedGitHubUrl(nextUri.ToString()))
                        {
                            throw new SecurityException("Yönlendirilen adres izin verilmeyen bir alan adına ait: " + nextUri);
                        }

                        currentUrl = nextUri.ToString();
                        redirectCount++;
                        continue;
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException("Sunucu yanıtı başarısız: " + response.StatusCode);
                    }

                    long totalBytes = response.ContentLength;
                    using (var responseStream = response.GetResponseStream())
                    using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        byte[] buffer = new byte[8192];
                        long totalRead = 0;
                        int read;

                        while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read, cancellationToken);
                            totalRead += read;

                            if (totalBytes > 0)
                            {
                                int percentage = (int)((totalRead * 100) / totalBytes);
                                double mbytesIn = Math.Round((double)totalRead / 1024 / 1024, 2);
                                double mtotalBytes = Math.Round((double)totalBytes / 1024 / 1024, 2);

                                this.BeginInvoke(new Action(() =>
                                {
                                    if (!_cts.IsCancellationRequested && !this.IsDisposed)
                                    {
                                        _progressBar.Value = Math.Min(100, Math.Max(0, percentage));
                                        _lblProgressPercentage.Text = $"%{percentage} tamamlandı ({mbytesIn} MB / {mtotalBytes} MB)";
                                    }
                                }));
                            }
                        }
                    }

                    return; // İndirme tamamlandı
                }
            }

            throw new WebException("Çok fazla yönlendirme tespit edildi.");
        }

        private void CleanupTempFiles()
        {
            try
            {
                if (File.Exists(_tempFilePath))
                {
                    File.Delete(_tempFilePath);
                }

                if (Directory.Exists(_tempDir))
                {
                    Directory.Delete(_tempDir, true);
                }
            }
            catch { }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (!_isCompletedOrCancelled)
            {
                _isCompletedOrCancelled = true;
                _cts.Cancel();
                CleanupTempFiles();
                this.Close();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_isCompletedOrCancelled)
            {
                _isCompletedOrCancelled = true;
                _cts.Cancel();
                CleanupTempFiles();
            }
            base.OnClosing(e);
        }
    }
}
