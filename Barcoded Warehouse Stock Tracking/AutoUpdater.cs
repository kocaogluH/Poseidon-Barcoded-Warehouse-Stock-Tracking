using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class UpdateInfo
    {
        public bool IsUpdateAvailable { get; set; }
        public string LatestVersion { get; set; }
        public string CurrentVersion { get; set; }
        public string DownloadUrl { get; set; }
        public string Sha256DownloadUrl { get; set; }
        public string ExpectedSha256Hash { get; set; }
        public string ReleaseNotes { get; set; }
    }

    public static class AutoUpdater
    {
        private const string RepoOwner = "kocaogluH";
        private const string RepoName = "Poseidon-Barcoded-Warehouse-Stock-Tracking";
        public const string ApiUrl = "https://api.github.com/repos/" + RepoOwner + "/" + RepoName + "/releases/latest";
        public const int RequestTimeoutMs = 10000; // 10 saniye zaman aşımı

        static AutoUpdater()
        {
            // GitHub API requires TLS 1.2
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        private class GitHubAssetDto
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }
        }

        private class GitHubReleaseDto
        {
            [JsonProperty("tag_name")]
            public string TagName { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("assets")]
            public List<GitHubAssetDto> Assets { get; set; }
        }

        /// <summary>
        /// Verilen URL'nin HTTPS ve izin verilen GitHub alan adlarına ait olup olmadığını doğrular.
        /// </summary>
        public static bool IsAllowedGitHubUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri)) return false;
            if (uri.Scheme != Uri.UriSchemeHttps) return false;

            string host = uri.Host.ToLowerInvariant();
            if (host == "api.github.com" || host == "github.com") return true;
            if (host == "githubusercontent.com" || host.EndsWith(".githubusercontent.com")) return true;

            return false;
        }

        /// <summary>
        /// .sha256 dosya içeriğinden veya release metninden 64 karakterlik hex hash değerini ayıklar.
        /// </summary>
        public static string ExtractSha256Hash(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            // Seçenek 1: Metin içinde "SHA256: <hash>" veya "SHA-256: <hash>" formatı
            var bodyMatch = Regex.Match(content, @"(?:SHA256|SHA-256)\s*[:=]\s*([a-fA-F0-9]{64})(?:\s|$)", RegexOptions.IgnoreCase);
            if (bodyMatch.Success)
            {
                return bodyMatch.Groups[1].Value.ToLowerInvariant();
            }

            // Seçenek 2: Standart .sha256 dosya formatı: "<64 karakterlik hex>  <dosya adı>" veya sadece "<64 karakterlik hex>"
            var fileMatch = Regex.Match(content.Trim(), @"^([a-fA-F0-9]{64})(?:\s|$)");
            if (fileMatch.Success)
            {
                return fileMatch.Groups[1].Value.ToLowerInvariant();
            }

            return null;
        }

        /// <summary>
        /// Dosyanın SHA256 özetini hesaplar.
        /// </summary>
        public static string ComputeFileSha256(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                var sb = new System.Text.StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString().ToLowerInvariant();
            }
        }

        /// <summary>
        /// Sabit zamanlı (constant-time) karakter/hash karşılaştırması yapar (Timing attack önlemi).
        /// </summary>
        public static bool FixedTimeEquals(string hash1, string hash2)
        {
            if (hash1 == null || hash2 == null) return false;
            if (hash1.Length != hash2.Length) return false;

            int diff = 0;
            for (int i = 0; i < hash1.Length; i++)
            {
                diff |= char.ToLowerInvariant(hash1[i]) ^ char.ToLowerInvariant(hash2[i]);
            }

            return diff == 0;
        }

        /// <summary>
        /// Güvenli HTTPS GET isteği ile yönlendirmeleri denetleyerek metin indirir.
        /// </summary>
        public static string DownloadStringWithDomainCheck(string url)
        {
            if (!IsAllowedGitHubUrl(url))
            {
                throw new System.Security.SecurityException("İzin verilmeyen veya güvensiz indirme adresi: " + url);
            }

            string currentUrl = url;
            int redirectCount = 0;

            while (redirectCount < 5)
            {
                var request = (HttpWebRequest)WebRequest.Create(currentUrl);
                request.UserAgent = "Poseidon-Stock-Tracking-Updater";
                request.Accept = "text/plain, application/octet-stream, application/vnd.github.v3+json, */*";
                request.Timeout = RequestTimeoutMs;
                request.ReadWriteTimeout = RequestTimeoutMs;
                request.AllowAutoRedirect = false;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (var response = (HttpWebResponse)request.GetResponse())
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

                        if (!IsAllowedGitHubUrl(nextUri.ToString()))
                        {
                            throw new System.Security.SecurityException("Yönlendirilen adres izin verilmeyen bir alan adına ait: " + nextUri);
                        }

                        currentUrl = nextUri.ToString();
                        redirectCount++;
                        continue;
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException("Sunucu yanıtı başarısız: " + response.StatusCode);
                    }

                    using (var stream = response.GetResponseStream())
                    {
                        if (stream == null) return string.Empty;
                        using (var reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            throw new WebException("Çok fazla yönlendirme tespit edildi.");
        }

        /// <summary>
        /// Arka planda asenkron olarak en son güncellemeyi denetler.
        /// </summary>
        public static async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            return await Task.Run(() =>
            {
                var info = new UpdateInfo
                {
                    IsUpdateAvailable = false,
                    CurrentVersion = Application.ProductVersion
                };

                try
                {
                    if (!IsAllowedGitHubUrl(ApiUrl))
                    {
                        Program.AppendErrorLog("GitHub API adresi izin verilen alan adları listesinde değil.");
                        return info;
                    }

                    string jsonResponse = DownloadStringWithDomainCheck(ApiUrl);
                    if (string.IsNullOrWhiteSpace(jsonResponse))
                    {
                        return info;
                    }

                    var release = JsonConvert.DeserializeObject<GitHubReleaseDto>(jsonResponse);
                    if (release == null || string.IsNullOrWhiteSpace(release.TagName) || release.Assets == null || release.Assets.Count == 0)
                    {
                        return info;
                    }

                    string latestVersionStr = release.TagName.Trim();
                    string releaseNotes = release.Body ?? string.Empty;

                    // Kural 1: Kurulum dosyası mutlaka adı "Poseidon_Setup" ile başlayıp ".exe" ile bitmelidir.
                    var setupAsset = release.Assets.FirstOrDefault(a =>
                        !string.IsNullOrEmpty(a.Name) &&
                        a.Name.StartsWith("Poseidon_Setup", StringComparison.OrdinalIgnoreCase) &&
                        a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

                    // Bulunamazsa Assets[0] gibi bir yedek SEÇME; güncelleme yapılmasın.
                    if (setupAsset == null || string.IsNullOrWhiteSpace(setupAsset.BrowserDownloadUrl))
                    {
                        return info;
                    }

                    // Kural 2: Doğrulama değeri (SHA256) tespiti:
                    // Seçenek A: .sha256 asset dosyası
                    var sha256Asset = release.Assets.FirstOrDefault(a =>
                        !string.IsNullOrEmpty(a.Name) &&
                        (a.Name.Equals(setupAsset.Name + ".sha256", StringComparison.OrdinalIgnoreCase) ||
                         (a.Name.StartsWith("Poseidon_Setup", StringComparison.OrdinalIgnoreCase) && a.Name.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase))));

                    // Seçenek B: Release notları (body) içerisindeki "SHA256: <hash>" satırı
                    string hashFromBody = ExtractSha256Hash(releaseNotes);

                    // Doğrulama değeri her iki seçenekte de bulunamazsa güncellemeyi yapma
                    if (sha256Asset == null && string.IsNullOrWhiteSpace(hashFromBody))
                    {
                        Program.AppendErrorLog("Güncelleme için SHA256 doğrulama değeri (.sha256 asset veya release notu) bulunamadı.");
                        return info;
                    }

                    // Alan adı ve HTTPS kontrolü
                    if (!IsAllowedGitHubUrl(setupAsset.BrowserDownloadUrl))
                    {
                        Program.AppendErrorLog("Kurulum dosyası indirme adresi güvenlik kısıtlamasından geçemedi.");
                        return info;
                    }

                    if (sha256Asset != null && !IsAllowedGitHubUrl(sha256Asset.BrowserDownloadUrl))
                    {
                        Program.AppendErrorLog("SHA256 asset indirme adresi güvenlik kısıtlamasından geçemedi.");
                        return info;
                    }

                    info.LatestVersion = latestVersionStr;
                    info.DownloadUrl = setupAsset.BrowserDownloadUrl;
                    info.Sha256DownloadUrl = sha256Asset != null ? sha256Asset.BrowserDownloadUrl : null;
                    info.ExpectedSha256Hash = hashFromBody;
                    info.ReleaseNotes = releaseNotes;

                    // Sürüm karşılaştırması (4 bileşenli normalleştirme ile)
                    Version latestVer = NormalizeVersion(latestVersionStr);
                    Version currentVer = NormalizeVersion(info.CurrentVersion);

                    if (latestVer > currentVer)
                    {
                        info.IsUpdateAvailable = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Güncelleme kontrolü sırasında hata (sessizce atlandı): " + ex.Message);
                }

                return info;
            });
        }

        public static Version NormalizeVersion(string versionStr)
        {
            if (string.IsNullOrWhiteSpace(versionStr))
                return new Version(0, 0, 0, 0);

            string clean = Regex.Replace(versionStr, @"^[^\d]+", ""); // "v1.0.1" -> "1.0.1"
            var parts = clean.Split('.');
            int major = parts.Length > 0 && int.TryParse(parts[0], out int mj) ? mj : 0;
            int minor = parts.Length > 1 && int.TryParse(parts[1], out int mn) ? mn : 0;
            int build = parts.Length > 2 && int.TryParse(parts[2], out int bd) ? bd : 0;
            int revision = parts.Length > 3 && int.TryParse(parts[3], out int rv) ? rv : 0;

            return new Version(major, minor, build, revision);
        }
    }
}
