using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public static class FeatureManager
    {
        public static string TenantId { get; set; } = "default";
        public static string CompanyName { get; set; } = "DEPO & STOK YÖNETİMİ";
        public static string CompanySubTitle { get; set; } = "Barkodlu Stok Takip Sistemi";

        // Feature Flags
        public static bool EnableMetalCalculator { get; set; } = true;
        public static bool EnableGlassSection { get; set; } = true;
        public static bool EnableFragileOption { get; set; } = true;
        public static bool EnableCustomerBalance { get; set; } = true;
        public static bool EnablePosModule { get; set; } = true;

        private static string ConfigPath => Path.Combine(AppPaths.DataDirectory, "tenant_config.json");

        static FeatureManager()
        {
            LoadConfig();
        }

        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    string tId = GetJsonValue(json, "TenantId");
                    if (!string.IsNullOrEmpty(tId)) TenantId = tId;

                    string cName = GetJsonValue(json, "CompanyName");
                    if (!string.IsNullOrEmpty(cName)) CompanyName = cName;

                    string cSub = GetJsonValue(json, "CompanySubTitle");
                    if (!string.IsNullOrEmpty(cSub)) CompanySubTitle = cSub;

                    if (bool.TryParse(GetJsonValue(json, "EnableMetalCalculator"), out bool mc)) EnableMetalCalculator = mc;
                    if (bool.TryParse(GetJsonValue(json, "EnableGlassSection"), out bool gs)) EnableGlassSection = gs;
                    if (bool.TryParse(GetJsonValue(json, "EnableFragileOption"), out bool fo)) EnableFragileOption = fo;
                    if (bool.TryParse(GetJsonValue(json, "EnableCustomerBalance"), out bool cb)) EnableCustomerBalance = cb;
                    if (bool.TryParse(GetJsonValue(json, "EnablePosModule"), out bool pos)) EnablePosModule = pos;
                }
                else
                {
                    SaveConfig();
                }
            }
            catch { }
        }

        public static void SaveConfig()
        {
            try
            {
                string json = $"{{\n" +
                              $"  \"TenantId\": \"{EscapeJson(TenantId)}\",\n" +
                              $"  \"CompanyName\": \"{EscapeJson(CompanyName)}\",\n" +
                              $"  \"CompanySubTitle\": \"{EscapeJson(CompanySubTitle)}\",\n" +
                              $"  \"EnableMetalCalculator\": {EnableMetalCalculator.ToString().ToLower()},\n" +
                              $"  \"EnableGlassSection\": {EnableGlassSection.ToString().ToLower()},\n" +
                              $"  \"EnableFragileOption\": {EnableFragileOption.ToString().ToLower()},\n" +
                              $"  \"EnableCustomerBalance\": {EnableCustomerBalance.ToString().ToLower()},\n" +
                              $"  \"EnablePosModule\": {EnablePosModule.ToString().ToLower()}\n" +
                              $"}}";

                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }

        private static string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string GetJsonValue(string json, string key)
        {
            var match = Regex.Match(json, $"\"{key}\"\\s*:\\s*\"?([^\",\n\\}}]+)\"?");
            return match.Success ? match.Groups[1].Value.Trim().Trim('"') : null;
        }
    }
}
