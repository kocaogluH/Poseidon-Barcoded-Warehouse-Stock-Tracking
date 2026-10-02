using System;
using System.Security.Cryptography;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public static class Security
    {
        // Tek kaynak: mevcut PBKDF2-HMAC-SHA256 iterasyon sayısı.
        // Eski hash'ler saklandığı iterasyon değeriyle doğrulanmaya devam eder.
        public const int CurrentIterations = 600_000;

        // Format: {iterations}.{saltBase64}.{hashBase64}
        public static string HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, CurrentIterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                return $"{CurrentIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
            }
        }

        // Doğrulama: hash içindeki iterasyon sayısını okur → geriye dönük uyumluluk.
        public static bool VerifyPassword(string password, string stored)
        {
            if (password == null) return false;
            if (string.IsNullOrWhiteSpace(stored)) return false;

            var parts = stored.Split('.');
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out int iterations)) return false;

            byte[] salt, expectedHash;
            try
            {
                salt         = Convert.FromBase64String(parts[1]);
                expectedHash = Convert.FromBase64String(parts[2]);
            }
            catch
            {
                return false;
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] actual = pbkdf2.GetBytes(expectedHash.Length);
                return FixedTimeEquals(actual, expectedHash);
            }
        }

        // Saklı hash'in iterasyonu güncel değerden düşükse rehash gerekir.
        public static bool NeedsRehash(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return false;
            var parts = stored.Split('.');
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out int storedIter)) return false;
            return storedIter < CurrentIterations;
        }

        // Zamanlama saldırılarını (timing attack) önlemek için:
        // Kullanıcı veritabanında bulunamadığında sahte hash doğrulama çalıştırılır.
        private static readonly string DummyHash =
            $"{CurrentIterations}.{Convert.ToBase64String(new byte[16])}.{Convert.ToBase64String(new byte[32])}";

        public static void DummyVerify(string password)
        {
            if (password == null) password = "";
            VerifyPassword(password, DummyHash);
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
