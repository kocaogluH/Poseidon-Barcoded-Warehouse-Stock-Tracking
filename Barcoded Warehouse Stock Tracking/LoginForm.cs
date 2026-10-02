using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class LoginForm : Form
    {
        private readonly Guna2TextBox _txtUser  = new Guna2TextBox();
        private readonly Guna2TextBox _txtPass  = new Guna2TextBox();
        private readonly Guna2Button  _btnLogin = new Guna2Button();
        private readonly Label        _lblError = new Label();
        private bool    _dbReady   = false;
        private Timer   _lockTimer = null; // geri sayım için

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (int Attempts, DateTime? LockoutUntil)> _nonExistentUserLocks
            = new System.Collections.Concurrent.ConcurrentDictionary<string, (int Attempts, DateTime? LockoutUntil)>(StringComparer.OrdinalIgnoreCase);

        public LoginForm()
        {
            // ── Form ──────────────────────────────────────────────────────────
            Text            = "Poseidon — Giriş";
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            AutoScaleMode   = AutoScaleMode.None;   // DPI ölçeklemeyi kapat
            Font            = new Font("Segoe UI", 9f);

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

            // Form: 460 × 660 (sabit, DPI bağımsız)
            ClientSize = new Size(460, 660);

            // Gradient arka plan
            this.Paint += (s, pe) =>
            {
                using (var br = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(10, 25,  77),   // lacivert
                    Color.FromArgb( 3, 82, 143),   // safir mavisi
                    150f))
                    pe.Graphics.FillRectangle(br, this.ClientRectangle);
            };

            // ── Logo ──────────────────────────────────────────────────────────
            const int LS = 110;  // Logo Size
            var pbLogo = new PictureBox
            {
                Size      = new Size(LS, LS),
                Location  = new Point((460 - LS) / 2, 24),  // x=175, y=24
                SizeMode  = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };

            var cp = new GraphicsPath();
            cp.AddEllipse(0, 0, LS, LS);
            pbLogo.Region = new Region(cp);

            pbLogo.Paint += (s, pe) =>
            {
                if (pbLogo.Image == null) return;
                pe.Graphics.SmoothingMode     = SmoothingMode.AntiAlias;
                pe.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, pbLogo.Width - 1, pbLogo.Height - 1);
                    pe.Graphics.SetClip(path);
                    var img = pbLogo.Image;
                    float sc = Math.Max((float)pbLogo.Width / img.Width, (float)pbLogo.Height / img.Height);
                    float dw = img.Width * sc, dh = img.Height * sc;
                    pe.Graphics.DrawImage(img, (pbLogo.Width - dw) / 2f, (pbLogo.Height - dh) / 2f, dw, dh);
                }
            };

            try
            {
                string exe  = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                string proj = System.IO.Path.GetFullPath(System.IO.Path.Combine(exe, "..", ".."));
                foreach (var p in new[]
                {
                    System.IO.Path.Combine(proj, "Resources", "poseidon_logo.png"),
                    System.IO.Path.Combine(exe,  "Resources", "poseidon_logo.png"),
                    System.IO.Path.Combine(exe,  "poseidon_logo.png"),
                    @"C:\Users\Halil Kocaoğlu\OneDrive\Masaüstü\iş\Poseidon Otomasyon&Yazılım\Logo-2--removebg-preview.png"
                })
                { if (System.IO.File.Exists(p)) { pbLogo.Image = Image.FromFile(p); break; } }
            }
            catch { }

            // ── Marka Başlığı  (logo alt = 24+110 = 134) ─────────────────────
            // lblBrand: y=140, h=40  →  alt=180
            var lblBrand = new Label
            {
                Text      = "Poseidon Yazılım",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 20, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Size      = new Size(460, 40),
                Location  = new Point(0, 140),
                BackColor = Color.Transparent
            };

            // lblSub: y=182, h=22  →  alt=204
            var lblSub = new Label
            {
                Text      = "Depo & Stok Yönetim Sistemi",
                ForeColor = Color.FromArgb(180, 210, 255),
                Font      = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                Size      = new Size(460, 22),
                Location  = new Point(0, 182),
                BackColor = Color.Transparent
            };

            // ── Kart  (lblSub alt=204 → +14 boşluk = 218) ────────────────────
            // Kart: x=40, y=218, w=380, h=376  →  sağ kenar=420, alt=594
            const int CW = 380, CH = 376;
            const int CX = (460 - CW) / 2;  // 40
            const int CY = 218;

            var card = new Guna2Panel
            {
                Size         = new Size(CW, CH),
                Location     = new Point(CX, CY),
                FillColor    = Color.White,
                BorderRadius = 22,
                ShadowDecoration =
                {
                    Enabled      = true,
                    Color        = Color.FromArgb(90, 0, 20, 80),
                    Depth        = 22,
                    BorderRadius = 22,
                    Shadow       = new Padding(10)
                }
            };

            // Kart içi ─ tüm Y değerleri karta görelidir
            // CTRL_W=300, iç yatay başlangıç: (380-300)/2 = 40
            const int TW = 300;
            const int TX = (CW - TW) / 2;   // 40

            // lblTitle: y=28, h=36  →  alt=64
            var lblTitle = new Label
            {
                Text      = "Hesabınıza Giriş Yapın",
                ForeColor = Color.FromArgb(15, 23, 42),
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Size      = new Size(TW, 36),
                Location  = new Point(TX, 28),
                BackColor = Color.Transparent
            };

            // ince ayırıcı çizgi: y=70, h=1  →  alt=71
            var divider = new Label
            {
                BackColor = Color.FromArgb(226, 232, 240),
                Size      = new Size(TW, 1),
                Location  = new Point(TX, 70)
            };

            // txtUser: y=90, h=55  →  alt=145
            _txtUser.Size               = new Size(TW, 55);
            _txtUser.Location           = new Point(TX, 90);
            _txtUser.PlaceholderText    = "Kullanıcı Adı";
            _txtUser.BorderRadius       = 10;
            _txtUser.FillColor          = Color.FromArgb(241, 245, 249);
            _txtUser.BorderColor        = Color.FromArgb(203, 213, 225);
            _txtUser.ForeColor          = Color.FromArgb(15, 23, 42);
            _txtUser.Font               = new Font("Segoe UI", 10);
            _txtUser.PlaceholderForeColor = Color.FromArgb(148, 163, 184);
            _txtUser.TextOffset         = new Point(10, 0);

            // txtPass: y=160, h=55  →  alt=215
            _txtPass.Size               = new Size(TW, 55);
            _txtPass.Location           = new Point(TX, 160);
            _txtPass.PlaceholderText    = "Şifre";
            _txtPass.BorderRadius       = 10;
            _txtPass.FillColor          = Color.FromArgb(241, 245, 249);
            _txtPass.BorderColor        = Color.FromArgb(203, 213, 225);
            _txtPass.ForeColor          = Color.FromArgb(15, 23, 42);
            _txtPass.Font               = new Font("Segoe UI", 10);
            _txtPass.UseSystemPasswordChar  = true;
            _txtPass.PlaceholderForeColor   = Color.FromArgb(148, 163, 184);
            _txtPass.TextOffset         = new Point(10, 0);

            // btnLogin: y=275, h=55  →  alt=330 (Eski konumu)
            _btnLogin.Text              = "Sistem hazırlanıyor...";
            _btnLogin.Size              = new Size(TW, 55);
            _btnLogin.Location          = new Point(TX, 275);
            _btnLogin.BorderRadius      = 10;
            _btnLogin.FillColor         = Color.FromArgb(14, 165, 233);
            _btnLogin.HoverState.FillColor = Color.FromArgb(2, 132, 199);
            _btnLogin.Font              = new Font("Segoe UI", 11, FontStyle.Bold);
            _btnLogin.ForeColor         = Color.White;
            _btnLogin.Cursor            = Cursors.Hand;
            _btnLogin.Animated          = true;
            _btnLogin.Enabled           = false;   // DB hazır olana kadar kilitli
            _btnLogin.Click            += async (_, __) => await DoLoginAsync();

            // lblError: y=334, h=32  →  alt=366 (Tam butonun altına gelir)
            _lblError.Size      = new Size(TW, 32);
            _lblError.Location  = new Point(TX, 334);
            _lblError.ForeColor = UiTheme.Danger;
            _lblError.TextAlign = ContentAlignment.MiddleCenter;
            _lblError.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _lblError.Text      = "";
            _lblError.BackColor = Color.Transparent;

            _txtUser.Leave += async (s, e) => await CheckUserLockoutAsync(_txtUser.Text.Trim());
            _txtUser.TextChanged += (s, e) =>
            {
                if (_lockTimer != null && _lockTimer.Enabled)
                {
                    _lockTimer.Stop();
                    _lockTimer.Dispose();
                    _lockTimer = null;
                    _btnLogin.Enabled = true;
                    _btnLogin.Text = "SİSTEME GİRİŞ YAP";
                    _lblError.Text = "";
                }
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(divider);
            card.Controls.Add(_txtUser);
            card.Controls.Add(_txtPass);
            card.Controls.Add(_btnLogin);
            card.Controls.Add(_lblError);
            _lblError.BringToFront();

            // ── Telif  (kart alt=218+370=588 → +14 = 602) ────────────────────
            // lblCopy: y=604, h=22  →  alt=626 < 660 ✓
            var lblCopy = new Label
            {
                Text      = "© 2026 Poseidon Yazılım — Software & Automation",
                ForeColor = Color.FromArgb(150, 185, 225),
                Font      = new Font("Segoe UI", 8),
                TextAlign = ContentAlignment.MiddleCenter,
                Size      = new Size(460, 22),
                Location  = new Point(0, 604),
                BackColor = Color.Transparent
            };

            // Controls.Add sırası: sonra eklenen ÜSTE çıkar.
            Controls.Add(card);
            Controls.Add(lblCopy);
            Controls.Add(lblSub);
            Controls.Add(lblBrand);
            Controls.Add(pbLogo);   // en üstte

            AcceptButton = _btnLogin;
        }

        // ── Başlangıç: DB arka planda hazırlanır ─────────────────────────────
        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                await Task.Run(() => Database.EnsureDatabase());
                _dbReady = true;

                bool hasUsers = await Task.Run(() => Database.HasAnyUser());
                if (!hasUsers)
                {
                    PromptInitialAdminSetup();
                }
                else
                {
                    _btnLogin.Text    = "SİSTEME GİRİŞ YAP";
                    _btnLogin.Enabled = true;
                    _txtUser.Focus();

                    if (!string.IsNullOrWhiteSpace(_txtUser.Text))
                    {
                        await CheckUserLockoutAsync(_txtUser.Text.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                _lblError.ForeColor = UiTheme.Danger;
                _lblError.Text      = "Veritabanı hatası!";
                _btnLogin.Text      = "Hata — Yeniden Dene";
                _btnLogin.Enabled   = true;
                _btnLogin.Click    -= null; // varolan handler'ı koru, retry ekle
                MessageBox.Show(
                    "Veritabanı başlatılamadı:\n\n" + ex.Message,
                    "Başlatma Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task CheckUserLockoutAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username) || !_dbReady) return;

            // 1. Veritabanından gerçek kilit kontrolü
            var status = await Task.Run(() => Database.GetLockoutStatus(username));
            if (status.Locked && status.LockoutUntil.HasValue)
            {
                StartLockoutCountdown(status.LockoutUntil.Value);
                return;
            }

            // 2. Bellekteki kilit kontrolü (var olmayan kullanıcı adları için)
            if (_nonExistentUserLocks.TryGetValue(username, out var memLock) && memLock.LockoutUntil.HasValue)
            {
                if (memLock.LockoutUntil.Value > DateTime.UtcNow)
                {
                    StartLockoutCountdown(memLock.LockoutUntil.Value);
                    return;
                }
                else
                {
                    _nonExistentUserLocks.TryRemove(username, out _);
                }
            }

            if (_lockTimer != null && _lockTimer.Enabled)
            {
                _lockTimer.Stop();
                _lockTimer.Dispose();
                _lockTimer = null;
            }
            _btnLogin.Enabled = true;
            _btnLogin.Text = "SİSTEME GİRİŞ YAP";
        }

        private void PromptInitialAdminSetup()
        {
            using (var setup = new FrmInitialAdminSetup())
            {
                if (setup.ShowDialog(this) == DialogResult.OK)
                {
                    _txtUser.Text = setup.CreatedUsername ?? "";
                    _txtPass.Clear();
                    _txtPass.Focus();
                    _lblError.ForeColor = UiTheme.Success;
                    _lblError.Text = "Yönetici oluşturuldu. Lütfen giriş yapın.";
                    _btnLogin.Text = "SİSTEME GİRİŞ YAP";
                    _btnLogin.Enabled = true;
                }
                else
                {
                    _lblError.ForeColor = UiTheme.Danger;
                    _lblError.Text = "Yönetici hesabı oluşturulmalıdır.";
                    _btnLogin.Text = "YÖNETİCİ HESABI OLUŞTUR";
                    _btnLogin.Enabled = true;
                }
            }
        }

        private async Task DoLoginAsync()
        {
            if (!_dbReady)
            {
                _lblError.Text = "Sistem henüz hazır değil, lütfen bekleyin.";
                return;
            }

            if (!Database.HasAnyUser())
            {
                PromptInitialAdminSetup();
                return;
            }

            var user = _txtUser.Text.Trim();
            var pass = _txtPass.Text;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                _lblError.Text = "Lütfen tüm alanları doldurun.";
                return;
            }

            // ── Kilit kontrolü (Doğrudan DB'den, şifre doğrulamadan KESİNLİKLE ÖNCE) ──
            var lockStatus = await Task.Run(() => Database.GetLockoutStatus(user));
            if (lockStatus.Locked && lockStatus.LockoutUntil.HasValue)
            {
                StartLockoutCountdown(lockStatus.LockoutUntil.Value);
                _txtPass.Clear();
                _txtPass.Focus();
                return;
            }

            // Bellekteki kilit kontrolü (var olmayan kullanıcı adları için)
            if (_nonExistentUserLocks.TryGetValue(user, out var memLock) && memLock.LockoutUntil.HasValue)
            {
                if (memLock.LockoutUntil.Value > DateTime.UtcNow)
                {
                    StartLockoutCountdown(memLock.LockoutUntil.Value);
                    _txtPass.Clear();
                    _txtPass.Focus();
                    return;
                }
                else
                {
                    _nonExistentUserLocks.TryRemove(user, out _);
                }
            }

            // UI'yi kilitle, geri bildirim ver
            _btnLogin.Enabled = false;
            _btnLogin.Text    = "Giriş yapılıyor...";
            _lblError.Text    = "";

            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = await Task.Run(() => Database.AuthenticateUser(user, pass));
                sw.Stop();

                if (result.HasValue)
                {
                    // Başarılı giriş → hata sayacını sıfırla
                    long uid = result.Value.Id;
                    _ = Task.Run(() => Database.ResetFailedAttempts(uid));
                    _nonExistentUserLocks.TryRemove(user, out _);

                    // 1) Önce: zayıf / eski şifre kontrolü — kullanıcı yeni şifre belirlemeden geçemez
                    if (pass == "1234" || pass.Length < 8)
                    {
                        using (var changeForm = new FrmChangePassword(result.Value.Id))
                        {
                            if (changeForm.ShowDialog(this) != DialogResult.OK)
                            {
                                _lblError.Text = "Devam edebilmek için lütfen şifrenizi güncelleyin.";
                                _txtPass.Clear();
                                _txtPass.Focus();
                                return;
                            }
                        }
                        // Yeni şifre belirledikten sonra oturum açılır; rehash zaten UpdatePassword ile yapıldı.
                    }
                    else
                    {
                        // 2) Sonra: sessiz rehash — iterasyon düşükse arka planda yeniden hash'le
                        if (Security.NeedsRehash(result.Value.StoredHash))
                        {
                            string capturedPass = pass;
                            long   capturedId   = result.Value.Id;
                            _ = Task.Run(() => Database.RehashPassword(capturedId, capturedPass));
                        }
                    }

                    Session.UserId    = result.Value.Id;
                    Session.Username  = user;
                    Session.Role      = result.Value.Role;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    // Hatalı giriş → sayacı artır
                    int newAttempts = await Task.Run(() => Database.RecordFailedAttempt(user));
                    const int maxAttempts = 5;

                    _txtPass.Clear();
                    _txtPass.Focus();

                    if (newAttempts == 0)
                    {
                        // Kullanıcı veritabanında YOK -> Bellekte say
                        var cur = _nonExistentUserLocks.AddOrUpdate(
                            user,
                            (1, (DateTime?)null),
                            (k, old) =>
                            {
                                int nextAtt = old.Attempts + 1;
                                DateTime? nextLock = nextAtt >= maxAttempts ? (DateTime?)DateTime.UtcNow.AddMinutes(5) : (DateTime?)null;
                                return (nextAtt, nextLock);
                            });

                        if (cur.LockoutUntil.HasValue && cur.LockoutUntil.Value > DateTime.UtcNow)
                        {
                            StartLockoutCountdown(cur.LockoutUntil.Value);
                        }
                        else
                        {
                            int remaining = maxAttempts - cur.Attempts;
                            _lblError.ForeColor = UiTheme.Danger;
                            _lblError.Text = remaining <= 1
                                ? "Şifre hatalı! Son 1 deneme hakkınız kaldı."
                                : $"Şifre hatalı! {remaining} deneme hakkınız kaldı.";
                        }
                    }
                    else
                    {
                        // Kullanıcı veritabanında VAR
                        if (newAttempts >= maxAttempts)
                        {
                            var newStatus = await Task.Run(() => Database.GetLockoutStatus(user));
                            if (newStatus.LockoutUntil.HasValue)
                                StartLockoutCountdown(newStatus.LockoutUntil.Value);
                        }
                        else
                        {
                            int remaining = maxAttempts - newAttempts;
                            _lblError.ForeColor = UiTheme.Danger;
                            _lblError.Text = remaining <= 1
                                ? "Şifre hatalı! Son 1 deneme hakkınız kaldı."
                                : $"Şifre hatalı! {remaining} deneme hakkınız kaldı.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _lblError.Text = "Giriş sırasında hata oluştu.";
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Her durumda butonu geri aç (kilit yoksa)
                if (_lockTimer == null || !_lockTimer.Enabled)
                {
                    _btnLogin.Enabled = true;
                    _btnLogin.Text    = "SİSTEME GİRİŞ YAP";
                }
            }
        }

        // Kilit süresince geri sayımı ekranda gösterir; süre dolunca giriş ekranı normale döner.
        private void StartLockoutCountdown(DateTime lockUntilUtc)
        {
            // Eski timer varsa temizle
            _lockTimer?.Stop();
            _lockTimer?.Dispose();

            _btnLogin.Enabled = false;
            _btnLogin.Text    = "KİLİTLİ";

            void Tick()
            {
                var remaining = lockUntilUtc.ToLocalTime() - DateTime.Now;
                if (remaining <= TimeSpan.Zero)
                {
                    _lockTimer?.Stop();
                    _lockTimer?.Dispose();
                    _lockTimer = null;

                    _lblError.ForeColor = UiTheme.Danger;
                    _lblError.Text      = "Hesap kilidi kalktı. Lütfen giriş yapın.";
                    _btnLogin.Enabled   = true;
                    _btnLogin.Text      = "SİSTEME GİRİŞ YAP";
                    _txtPass.Focus();
                }
                else
                {
                    string countdown = remaining.TotalMinutes >= 1
                        ? $"{(int)remaining.TotalMinutes}:{remaining.Seconds:D2}"
                        : $"{remaining.Seconds} sn";
                    _lblError.ForeColor = UiTheme.Danger;
                    _lblError.Text      = $"Hesap kilitlendi! {countdown} sonra tekrar deneyin.";
                }
            }

            Tick(); // hemen bir kez çalıştır

            _lockTimer = new Timer { Interval = 1000 };
            _lockTimer.Tick += (s, e) => Tick();
            _lockTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _lockTimer?.Stop();
                _lockTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}