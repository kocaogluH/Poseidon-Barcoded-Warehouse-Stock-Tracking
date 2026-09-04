using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public class UpdateInfo
    {
        public bool IsUpdateAvailable { get; set; }
        public string LatestVersion { get; set; }
        public string CurrentVersion { get; set; }
        public string DownloadUrl { get; set; }
        public string ReleaseNotes { get; set; }
    }

    public static class AutoUpdater
    {
        private const string RepoOwner = "kocaogluH";
        private const string RepoName = "Poseidon-Barcoded-Warehouse-Stock-Tracking";
        private const string ApiUrl = "https://api.github.com/repos/" + RepoOwner + "/" + RepoName + "/releases/latest";

        static AutoUpdater()
        {
            // GitHub API requires TLS 1.2
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
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
                    using (var webClient = new WebClient())
                    {
                        // GitHub API requires a User-Agent header
                        webClient.Headers.Add("User-Agent", "Poseidon-Stock-Tracking-Updater");
                        webClient.Headers.Add("Accept", "application/vnd.github.v3+json");

                        string json = webClient.DownloadString(ApiUrl);

                        // Regex ile JSON ayrıştırma (Harici bağımlılıkları önlemek için)
                        var tagMatch = Regex.Match(json, @"\""tag_name\""\s*:\s*\""([^\""]+)\""");
                        var urlMatch = Regex.Match(json, @"\""browser_download_url\""\s*:\s*\""([^\""]+\.exe)\""");
                        var bodyMatch = Regex.Match(json, @"\""body\""\s*:\s*\""([^\""]+)\""");

                        if (tagMatch.Success && urlMatch.Success)
                        {
                            string latestVersionStr = tagMatch.Groups[1].Value;
                            string downloadUrl = urlMatch.Groups[1].Value;
                            string releaseNotes = bodyMatch.Success ? bodyMatch.Groups[1].Value : "";

                            // Unicode karakter kaçışlarını çöz (Örn: \r\n, \u003c vb.)
                            releaseNotes = Regex.Unescape(releaseNotes);

                            info.LatestVersion = latestVersionStr;
                            info.DownloadUrl = downloadUrl;
                            info.ReleaseNotes = releaseNotes;

                            // Sürüm karşılaştırması (4 bileşenli normalleştirme ile)
                            Version latestVer = NormalizeVersion(latestVersionStr);
                            Version currentVer = NormalizeVersion(info.CurrentVersion);

                            if (latestVer > currentVer)
                            {
                                info.IsUpdateAvailable = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Güncelleme kontrolü sırasında hata: " + ex.Message);
                }

                return info;
            });
        }

        private static Version NormalizeVersion(string versionStr)
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
