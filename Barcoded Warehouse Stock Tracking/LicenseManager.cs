using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public static class LicenseManager
    {
        private const string SecretSalt = "Poseidon-StockTracking-SecretSalt-2026#Halil";
        private static string LicenseFilePath => Path.Combine(AppPaths.DataDirectory, "license.lic");

        public static string GetHardwareId()
        {
            try
            {
                string machineGuid = "";
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                    .OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key != null)
                    {
                        machineGuid = key.GetValue("MachineGuid")?.ToString() ?? "";
                    }
                }

                if (string.IsNullOrEmpty(machineGuid))
                {
                    machineGuid = Environment.MachineName + "_" + Environment.UserName;
                }

                using (var sha = SHA256.Create())
                {
                    byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(machineGuid + "HWID"));
                    string rawHash = BitConverter.ToString(bytes).Replace("-", "").Substring(0, 16);
                    return $"HWID-{rawHash.Substring(0, 4)}-{rawHash.Substring(4, 4)}-{rawHash.Substring(8, 4)}-{rawHash.Substring(12, 4)}";
                }
            }
            catch
            {
                return "HWID-DEFAULT-0000-0000";
            }
        }

        public static string GenerateKeyForHwid(string hwid)
        {
            if (string.IsNullOrWhiteSpace(hwid)) return "";
            string cleanedHwid = hwid.Trim().ToUpper();
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(cleanedHwid + SecretSalt));
                string rawHash = BitConverter.ToString(bytes).Replace("-", "").Substring(0, 16);
                return $"KEY-{rawHash.Substring(0, 4)}-{rawHash.Substring(4, 4)}-{rawHash.Substring(8, 4)}-{rawHash.Substring(12, 4)}";
            }
        }

        public static bool ValidateKey(string inputKey)
        {
            if (string.IsNullOrWhiteSpace(inputKey)) return false;
            string currentHwid = GetHardwareId();
            string expectedKey = GenerateKeyForHwid(currentHwid);
            return string.Equals(inputKey.Trim(), expectedKey, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsLicensed()
        {
            try
            {
                if (!File.Exists(LicenseFilePath)) return false;
                string savedKey = File.ReadAllText(LicenseFilePath).Trim();
                return ValidateKey(savedKey);
            }
            catch
            {
                return false;
            }
        }

        public static void SaveLicense(string validKey)
        {
            try
            {
                File.WriteAllText(LicenseFilePath, validKey.Trim());
            }
            catch { }
        }
    }
}
