using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;

namespace Barcoded_Warehouse_Stock_Tracking
{
    public static class Database
    {
        public static event Action DataChanged; // Global event for data updates
        private static void NotifyDataChanged() => DataChanged?.Invoke();

        private static readonly string ConnectionString = GetConnectionString();

        private static string GetConnectionString()
        {
            var cs = ConfigurationManager.ConnectionStrings["WarehouseDb"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException(
                    "Connection string bulunamadı: 'WarehouseDb'. App.config içindeki <connectionStrings> bölümünü kontrol edin.");
            }

            return cs;
        }

        public static SQLiteConnection GetConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            return conn;
        }

        public static void EnsureDatabase()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var pragma = conn.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA foreign_keys = ON;";
                    pragma.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Users (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Username TEXT NOT NULL UNIQUE,
  PasswordHash TEXT NOT NULL,
  Role TEXT NOT NULL,
  IsActive INTEGER NOT NULL DEFAULT 1,
  CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Products (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Barcode TEXT NOT NULL UNIQUE,
  Name TEXT NOT NULL,
  UnitPrice REAL NOT NULL,
  CostPrice REAL NOT NULL DEFAULT 0,
  VatRate REAL NOT NULL DEFAULT 0,
  StockQty REAL NOT NULL DEFAULT 0,
  IsActive INTEGER NOT NULL DEFAULT 1,
  Category TEXT NOT NULL DEFAULT '',
  Unit TEXT NOT NULL DEFAULT 'Adet',
  MaterialType TEXT NOT NULL DEFAULT '',
  QualityStandard TEXT NOT NULL DEFAULT '',
  Thickness REAL NOT NULL DEFAULT 0,
  Width REAL NOT NULL DEFAULT 0,
  Length REAL NOT NULL DEFAULT 0,
  TheoreticalWeight REAL NOT NULL DEFAULT 0,
  ShelfLocation TEXT NOT NULL DEFAULT '',
  CriticalStock REAL NOT NULL DEFAULT 5,
  Material TEXT NOT NULL DEFAULT '',
  BoxQty INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS StockMovements (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  ProductId INTEGER NOT NULL,
  BarcodeSnapshot TEXT NOT NULL,
  Quantity REAL NOT NULL,
  Type TEXT NOT NULL,
  Reason TEXT NULL,
  RefType TEXT NULL,
  RefId INTEGER NULL,
  CreatedAt TEXT NOT NULL,
  CreatedByUserId INTEGER NULL,
  FOREIGN KEY(ProductId) REFERENCES Products(Id)
);

CREATE TABLE IF NOT EXISTS Customers (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL,
  Phone TEXT NULL,
  Email TEXT NULL,
  Balance REAL NOT NULL DEFAULT 0,
  IsActive INTEGER NOT NULL DEFAULT 1,
  CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Sales (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  SaleNo TEXT NOT NULL UNIQUE,
  CustomerId INTEGER NULL,
  Subtotal REAL NOT NULL,
  DiscountTotal REAL NOT NULL,
  GrandTotal REAL NOT NULL,
  Status TEXT NOT NULL,
  CreatedAt TEXT NOT NULL,
  CreatedByUserId INTEGER NULL,
  FOREIGN KEY(CustomerId) REFERENCES Customers(Id)
);

CREATE TABLE IF NOT EXISTS SaleItems (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  SaleId INTEGER NOT NULL,
  ProductId INTEGER NOT NULL,
  BarcodeSnapshot TEXT NOT NULL,
  NameSnapshot TEXT NOT NULL,
  UnitSnapshot TEXT NOT NULL DEFAULT 'Adet',
  UnitPrice REAL NOT NULL,
  Quantity REAL NOT NULL,
  LineTotal REAL NOT NULL,
  FOREIGN KEY(SaleId) REFERENCES Sales(Id),
  FOREIGN KEY(ProductId) REFERENCES Products(Id)
);

CREATE TABLE IF NOT EXISTS Payments (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  SaleId INTEGER NOT NULL,
  Method TEXT NOT NULL,
  Amount REAL NOT NULL,
  CreatedAt TEXT NOT NULL,
  FOREIGN KEY(SaleId) REFERENCES Sales(Id)
);

CREATE TABLE IF NOT EXISTS CustomerTransactions (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  CustomerId INTEGER NOT NULL,
  RefType TEXT NOT NULL,
  RefId INTEGER NOT NULL,
  Amount REAL NOT NULL,
  CreatedAt TEXT NOT NULL,
  FOREIGN KEY(CustomerId) REFERENCES Customers(Id)
);

CREATE TABLE IF NOT EXISTS CustomerCollections (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  CustomerId INTEGER NOT NULL,
  Method TEXT NOT NULL,
  Amount REAL NOT NULL,
  CreatedAt TEXT NOT NULL,
  CreatedByUserId INTEGER NULL,
  FOREIGN KEY(CustomerId) REFERENCES Customers(Id)
);

CREATE TABLE IF NOT EXISTS SaleReturns (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  SaleId INTEGER NOT NULL,
  ReturnNo TEXT NOT NULL UNIQUE,
  Total REAL NOT NULL,
  CreatedAt TEXT NOT NULL,
  CreatedByUserId INTEGER NULL,
  FOREIGN KEY(SaleId) REFERENCES Sales(Id)
);

CREATE TABLE IF NOT EXISTS SaleReturnItems (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  SaleReturnId INTEGER NOT NULL,
  SaleItemId INTEGER NOT NULL,
  ProductId INTEGER NOT NULL,
  Quantity REAL NOT NULL,
  UnitPrice REAL NOT NULL,
  LineTotal REAL NOT NULL,
  FOREIGN KEY(SaleReturnId) REFERENCES SaleReturns(Id),
  FOREIGN KEY(SaleItemId) REFERENCES SaleItems(Id),
  FOREIGN KEY(ProductId) REFERENCES Products(Id)
);
";
                    cmd.ExecuteNonQuery();
                }

                // migration: add IsActive to Customers if missing
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "ALTER TABLE Customers ADD COLUMN IsActive INTEGER NOT NULL DEFAULT 1;";
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { /* already exists or error */ }

                // migrations for Metal & personalization
                foreach (var migration in new[] {
                    "ALTER TABLE Products ADD COLUMN Category TEXT NOT NULL DEFAULT '';",
                    "ALTER TABLE Products ADD COLUMN Material TEXT NOT NULL DEFAULT '';",
                    "ALTER TABLE Products ADD COLUMN ShelfLocation TEXT NOT NULL DEFAULT '';",
                    "ALTER TABLE Products ADD COLUMN BoxQty INTEGER NOT NULL DEFAULT 1;",
                    "ALTER TABLE Products ADD COLUMN CriticalStock REAL NOT NULL DEFAULT 5;",
                    "ALTER TABLE Products ADD COLUMN Unit TEXT NOT NULL DEFAULT 'Adet';",
                    "ALTER TABLE Products ADD COLUMN MaterialType TEXT NOT NULL DEFAULT '';",
                    "ALTER TABLE Products ADD COLUMN QualityStandard TEXT NOT NULL DEFAULT '';",
                    "ALTER TABLE Products ADD COLUMN Thickness REAL NOT NULL DEFAULT 0;",
                    "ALTER TABLE Products ADD COLUMN Width REAL NOT NULL DEFAULT 0;",
                    "ALTER TABLE Products ADD COLUMN Length REAL NOT NULL DEFAULT 0;",
                    "ALTER TABLE Products ADD COLUMN TheoreticalWeight REAL NOT NULL DEFAULT 0;",
                    "ALTER TABLE SaleItems ADD COLUMN UnitSnapshot TEXT NOT NULL DEFAULT 'Adet';"
                })
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = migration;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch { /* already exists or error */ }
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(1) FROM Users";
                    long userCount = Convert.ToInt64(cmd.ExecuteScalar() ?? 0L);
                    if (userCount == 0)
                    {
                        var hash = Security.HashPassword("1234");
                        using (var ins = conn.CreateCommand())
                        {
                            ins.CommandText = "INSERT INTO Users(Username, PasswordHash, Role, CreatedAt) VALUES('admin', @h, 'Admin', @dt)";
                            ins.Parameters.AddWithValue("@h", hash);
                            ins.Parameters.AddWithValue("@dt", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
                            ins.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static (long Id, string Role)? AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return null;
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, PasswordHash, Role FROM Users WHERE LOWER(Username) = LOWER(@u) AND IsActive = 1";
                    cmd.Parameters.AddWithValue("@u", username);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;
                        long id = r.GetInt64(0);
                        string hash = r.GetString(1);
                        string role = r.GetString(2);
                        if (Security.VerifyPassword(password, hash))
                        {
                            return (id, role);
                        }
                        return null;
                    }
                }
            }
        }

        public static void UpdatePassword(long userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword)) throw new ArgumentException("Şifre boş olamaz.");
            var hash = Security.HashPassword(newPassword);
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Users SET PasswordHash = @h WHERE Id = @id";
                cmd.Parameters.AddWithValue("@h", hash);
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateUsername(long userId, string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername)) throw new ArgumentException("Kullanıcı adı boş olamaz.");
            using (var conn = GetConnection())
            {
                conn.Open();
                // Aynı kullanıcı adının başka birisi tarafından kullanılıp kullanılmadığını kontrol et
                using (var chk = conn.CreateCommand())
                {
                    chk.CommandText = "SELECT COUNT(1) FROM Users WHERE LOWER(Username) = LOWER(@u) AND Id != @id";
                    chk.Parameters.AddWithValue("@u", newUsername);
                    chk.Parameters.AddWithValue("@id", userId);
                    long cnt = Convert.ToInt64(chk.ExecuteScalar() ?? 0L);
                    if (cnt > 0) throw new InvalidOperationException("Bu kullanıcı adı zaten başka biri tarafından kullanılıyor.");
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET Username = @u WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@u", newUsername);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void BackupDatabase()
        {
            try
            {
                string dataDir = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (string.IsNullOrWhiteSpace(dataDir)) return;
                
                string dbPath = System.IO.Path.Combine(dataDir, "warehouse.db");
                if (!System.IO.File.Exists(dbPath)) return;

                string docsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string backupFolder = System.IO.Path.Combine(docsFolder, "BarcodedWarehouse_Backups");
                
                if (!System.IO.Directory.Exists(backupFolder))
                {
                    System.IO.Directory.CreateDirectory(backupFolder);
                }

                string backupFileName = $"warehouse_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                string backupFilePath = System.IO.Path.Combine(backupFolder, backupFileName);

                System.IO.File.Copy(dbPath, backupFilePath, true);
            }
            catch (Exception)
            {
                // Sessizce yoksay veya logla
            }
        }


        public static DataTable GetProducts()
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Barcode, Name, UnitPrice, StockQty FROM Products WHERE IsActive = 1 ORDER BY Name";
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static void InsertProduct(string barcode, string name, decimal unitPrice)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Products(Barcode, Name, UnitPrice) VALUES(@b,@n,@p)";
                cmd.Parameters.AddWithValue("@b", barcode);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@p", Convert.ToDouble(unitPrice, CultureInfo.InvariantCulture));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            NotifyDataChanged();
        }

        public static bool ProductExists(string barcode)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM Products WHERE Barcode = @b AND IsActive = 1";
                cmd.Parameters.AddWithValue("@b", barcode);
                conn.Open();
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private static (long Id, string Barcode, string Name, double UnitPrice) GetProductByBarcode(SQLiteConnection conn, string barcode)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Barcode, Name, UnitPrice FROM Products WHERE Barcode = @b AND IsActive = 1";
                cmd.Parameters.AddWithValue("@b", barcode);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        throw new InvalidOperationException("Ürün bulunamadı: " + barcode);
                    }

                    return (r.GetInt64(0), r.GetString(1), r.GetString(2), r.GetDouble(3));
                }
            }
        }

        public static DataTable GetMovements()
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT CreatedAt AS Date, BarcodeSnapshot AS Barcode, Type, Quantity
FROM StockMovements
ORDER BY datetime(CreatedAt) DESC;";
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static void InsertMovement(string barcode, double quantity, string type, DateTime date)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    var product = GetProductByBarcode(conn, barcode);

                    double signedQty = string.Equals(type, "Çıkış", StringComparison.OrdinalIgnoreCase) ? -quantity : quantity;

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
INSERT INTO StockMovements(ProductId, BarcodeSnapshot, Quantity, Type, Reason, RefType, RefId, CreatedAt, CreatedByUserId)
VALUES(@pid, @b, @q, @t, NULL, 'Manual', NULL, @dt, NULL);";
                        cmd.Parameters.AddWithValue("@pid", product.Id);
                        cmd.Parameters.AddWithValue("@b", product.Barcode);
                        cmd.Parameters.AddWithValue("@q", signedQty);
                        cmd.Parameters.AddWithValue("@t", type);
                        cmd.Parameters.AddWithValue("@dt", date.ToString("s", CultureInfo.InvariantCulture));
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd2 = conn.CreateCommand())
                    {
                        cmd2.Transaction = tx;
                        cmd2.CommandText = "UPDATE Products SET StockQty = StockQty + @dq WHERE Id = @pid";
                        cmd2.Parameters.AddWithValue("@dq", signedQty);
                        cmd2.Parameters.AddWithValue("@pid", product.Id);
                        cmd2.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        public sealed class SaleItemInput
        {
            public long ProductId { get; set; }
            public string BarcodeSnapshot { get; set; }
            public string NameSnapshot { get; set; }
            public string UnitSnapshot { get; set; } = "Adet";
            public double UnitPrice { get; set; }
            public double Quantity { get; set; }
        }

        public static bool TryGetProductForSale(string barcode, out long productId, out string name, out double unitPrice, out double stockQty, out string unit)
        {
            productId = 0;
            name = null;
            unitPrice = 0;
            stockQty = 0;
            unit = "Adet";

            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT Id, Name, UnitPrice, StockQty, IFNULL(Unit, 'Adet') FROM Products WHERE Barcode = @b AND IsActive = 1";
                cmd.Parameters.AddWithValue("@b", barcode);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return false;
                    productId = r.GetInt64(0);
                    name = r.GetString(1);
                    unitPrice = r.GetDouble(2);
                    stockQty = Convert.ToDouble(r.GetValue(3));
                    unit = r.IsDBNull(4) ? "Adet" : r.GetString(4);
                    return true;
                }
            }
        }

        public static long CreateSale(
            string saleNo,
            long? customerId,
            IReadOnlyList<SaleItemInput> items,
            double discountTotal,
            IReadOnlyList<(string Method, double Amount)> payments,
            long createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(saleNo)) throw new ArgumentException("SaleNo zorunludur.", nameof(saleNo));
            if (items == null || items.Count == 0) throw new ArgumentException("Satış kalemi zorunludur.", nameof(items));
            if (payments == null || payments.Count == 0) throw new ArgumentException("Ödeme zorunludur.", nameof(payments));

            double subtotal = 0;
            foreach (var it in items)
            {
                if (it.Quantity <= 0) throw new InvalidOperationException("Miktar 0 olamaz.");
                if (it.UnitPrice < 0) throw new InvalidOperationException("Fiyat 0'dan küçük olamaz.");
                subtotal += it.UnitPrice * it.Quantity;
            }

            if (discountTotal < 0) discountTotal = 0;
            if (discountTotal > subtotal) discountTotal = subtotal;
            double grandTotal = subtotal - discountTotal;

            double paid = 0;
            foreach (var p in payments)
            {
                if (p.Amount <= 0) continue;
                paid += p.Amount;
            }

            if (!customerId.HasValue)
            {
                if (Math.Abs(paid - grandTotal) > 0.01)
                {
                    throw new InvalidOperationException("Ödeme toplamı genel toplam ile aynı olmalıdır.");
                }
            }
            else
            {
                if (paid - grandTotal > 0.01)
                {
                    throw new InvalidOperationException("Ödeme toplamı genel toplamı aşamaz (veresiye).");
                }
            }

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    long saleId;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
INSERT INTO Sales(SaleNo, CustomerId, Subtotal, DiscountTotal, GrandTotal, Status, CreatedAt, CreatedByUserId)
VALUES(@no, @cid, @sub, @disc, @grand, 'Completed', @dt, @uid);
SELECT last_insert_rowid();";
                        cmd.Parameters.AddWithValue("@no", saleNo);
                        cmd.Parameters.AddWithValue("@cid", customerId.HasValue ? (object)customerId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@sub", subtotal);
                        cmd.Parameters.AddWithValue("@disc", discountTotal);
                        cmd.Parameters.AddWithValue("@grand", grandTotal);
                        cmd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("@uid", createdByUserId);
                        saleId = (long)(cmd.ExecuteScalar() ?? 0L);
                    }

                    foreach (var it in items)
                    {
                        // stock check
                        double currentStock;
                        using (var cmdStock = conn.CreateCommand())
                        {
                            cmdStock.Transaction = tx;
                            cmdStock.CommandText = "SELECT StockQty FROM Products WHERE Id = @pid";
                            cmdStock.Parameters.AddWithValue("@pid", it.ProductId);
                            currentStock = Convert.ToDouble(cmdStock.ExecuteScalar() ?? 0);
                        }

                        if (currentStock < it.Quantity)
                        {
                            throw new InvalidOperationException($"Yetersiz stok: {it.BarcodeSnapshot} (Mevcut: {currentStock:N2})");
                        }

                        using (var cmdItem = conn.CreateCommand())
                        {
                            cmdItem.Transaction = tx;
                            cmdItem.CommandText = @"
INSERT INTO SaleItems(SaleId, ProductId, BarcodeSnapshot, NameSnapshot, UnitSnapshot, UnitPrice, Quantity, LineTotal)
VALUES(@sid, @pid, @b, @n, @u, @p, @q, @lt);";
                            cmdItem.Parameters.AddWithValue("@sid", saleId);
                            cmdItem.Parameters.AddWithValue("@pid", it.ProductId);
                            cmdItem.Parameters.AddWithValue("@b", it.BarcodeSnapshot);
                            cmdItem.Parameters.AddWithValue("@n", it.NameSnapshot);
                            cmdItem.Parameters.AddWithValue("@u", string.IsNullOrEmpty(it.UnitSnapshot) ? "Adet" : it.UnitSnapshot);
                            cmdItem.Parameters.AddWithValue("@p", it.UnitPrice);
                            cmdItem.Parameters.AddWithValue("@q", it.Quantity);
                            cmdItem.Parameters.AddWithValue("@lt", it.UnitPrice * it.Quantity);
                            cmdItem.ExecuteNonQuery();
                        }

                        using (var cmdUpd = conn.CreateCommand())
                        {
                            cmdUpd.Transaction = tx;
                            cmdUpd.CommandText = "UPDATE Products SET StockQty = StockQty - @q WHERE Id = @pid";
                            cmdUpd.Parameters.AddWithValue("@q", it.Quantity);
                            cmdUpd.Parameters.AddWithValue("@pid", it.ProductId);
                            cmdUpd.ExecuteNonQuery();
                        }

                        using (var cmdMv = conn.CreateCommand())
                        {
                            cmdMv.Transaction = tx;
                            cmdMv.CommandText = @"
INSERT INTO StockMovements(ProductId, BarcodeSnapshot, Quantity, Type, Reason, RefType, RefId, CreatedAt, CreatedByUserId)
VALUES(@pid, @b, @q, 'Çıkış', 'Sale', 'Sale', @rid, @dt, @uid);";
                            cmdMv.Parameters.AddWithValue("@pid", it.ProductId);
                            cmdMv.Parameters.AddWithValue("@b", it.BarcodeSnapshot);
                            cmdMv.Parameters.AddWithValue("@q", -it.Quantity);
                            cmdMv.Parameters.AddWithValue("@rid", saleId);
                            cmdMv.Parameters.AddWithValue("@dt", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
                            cmdMv.Parameters.AddWithValue("@uid", createdByUserId);
                            cmdMv.ExecuteNonQuery();
                        }
                    }

                    foreach (var p in payments)
                    {
                        if (p.Amount <= 0) continue;
                        using (var cmdPay = conn.CreateCommand())
                        {
                            cmdPay.Transaction = tx;
                            cmdPay.CommandText = "INSERT INTO Payments(SaleId, Method, Amount, CreatedAt) VALUES(@sid,@m,@a,@dt)";
                            cmdPay.Parameters.AddWithValue("@sid", saleId);
                            cmdPay.Parameters.AddWithValue("@m", p.Method);
                            cmdPay.Parameters.AddWithValue("@a", p.Amount);
                            cmdPay.Parameters.AddWithValue("@dt", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
                            cmdPay.ExecuteNonQuery();
                        }
                    }

                    // Customer balance (veresiye)
                    if (customerId.HasValue)
                    {
                        var due = grandTotal - paid;
                        if (due > 0.01)
                        {
                            InsertCustomerTransaction(conn, tx, customerId.Value, "Sale", saleId, due);
                        }
                    }
                    tx.Commit();
                    NotifyDataChanged();
                    return saleId;
                }
            }
        }

        private static void InsertCustomerTransaction(SQLiteConnection conn, SQLiteTransaction tx, long customerId, string refType, long refId, double amount)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO CustomerTransactions(CustomerId, RefType, RefId, Amount, CreatedAt) VALUES(@c,@t,@r,@a,@dt)";
                cmd.Parameters.AddWithValue("@c", customerId);
                cmd.Parameters.AddWithValue("@t", refType);
                cmd.Parameters.AddWithValue("@r", refId);
                cmd.Parameters.AddWithValue("@a", amount);
                cmd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "UPDATE Customers SET Balance = Balance + @a WHERE Id = @c";
                cmd.Parameters.AddWithValue("@a", amount);
                cmd.Parameters.AddWithValue("@c", customerId);
                cmd.ExecuteNonQuery();
            }
        }

        public static DataTable GetCustomers()
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT Id, Name, Phone, Email, Balance FROM Customers ORDER BY Name";
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static void DeleteCustomer(long id)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Customers SET IsActive = 0 WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            NotifyDataChanged();
        }

        public static void DeleteProduct(string barcode)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Products SET IsActive = 0 WHERE Barcode = @b";
                cmd.Parameters.AddWithValue("@b", barcode);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            NotifyDataChanged();
        }

        public static long InsertCustomer(string name, string phone, string email)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
INSERT INTO Customers(Name, Phone, Email, Balance, CreatedAt)
VALUES(@n,@p,@e,0,@dt);
SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@p", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);
                cmd.Parameters.AddWithValue("@e", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                cmd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
                long id = (long)(cmd.ExecuteScalar() ?? 0L);
                NotifyDataChanged();
                return id;
            }
        }

        public static void AddCollection(long customerId, string method, double amount, long createdByUserId)
        {
            if (amount <= 0) throw new InvalidOperationException("Tutar 0 olamaz.");

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    long collectionId;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
INSERT INTO CustomerCollections(CustomerId, Method, Amount, CreatedAt, CreatedByUserId)
VALUES(@c,@m,@a,@dt,@u);
SELECT last_insert_rowid();";
                        cmd.Parameters.AddWithValue("@c", customerId);
                        cmd.Parameters.AddWithValue("@m", method);
                        cmd.Parameters.AddWithValue("@a", amount);
                        cmd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("@u", createdByUserId);
                        collectionId = (long)(cmd.ExecuteScalar() ?? 0L);
                    }

                    // Collection reduces balance (negative transaction)
                    InsertCustomerTransaction(conn, tx, customerId, "Collection", collectionId, -amount);

                    tx.Commit();
                }
            }
        }

        public sealed class ReturnItemInput
        {
            public long SaleItemId { get; set; }
            public long ProductId { get; set; }
            public string BarcodeSnapshot { get; set; }
            public int Quantity { get; set; }
            public double UnitPrice { get; set; }
        }

        public static DataTable GetSaleItemsForReturn(string saleNo)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
SELECT 
  s.Id AS SaleId,
  si.Id AS SaleItemId,
  si.ProductId,
  si.BarcodeSnapshot AS Barcode,
  si.NameSnapshot AS Name,
  si.UnitPrice,
  si.Quantity AS SoldQty,
  IFNULL((SELECT SUM(ri.Quantity) FROM SaleReturnItems ri 
          JOIN SaleReturns r ON r.Id = ri.SaleReturnId
          WHERE r.SaleId = s.Id AND ri.SaleItemId = si.Id), 0) AS ReturnedQty
FROM Sales s
JOIN SaleItems si ON si.SaleId = s.Id
WHERE s.SaleNo = @no AND s.Status = 'Completed'
ORDER BY si.Id;";
                cmd.Parameters.AddWithValue("@no", saleNo);
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static long CreateReturn(string saleNo, string returnNo, IReadOnlyList<ReturnItemInput> items, long createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(saleNo)) throw new ArgumentException("SaleNo zorunludur.", nameof(saleNo));
            if (string.IsNullOrWhiteSpace(returnNo)) throw new ArgumentException("ReturnNo zorunludur.", nameof(returnNo));
            if (items == null || items.Count == 0) throw new ArgumentException("İade kalemi zorunludur.", nameof(items));

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    long saleId;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = "SELECT Id FROM Sales WHERE SaleNo = @no AND Status = 'Completed'";
                        cmd.Parameters.AddWithValue("@no", saleNo);
                        var o = cmd.ExecuteScalar();
                        if (o == null) throw new InvalidOperationException("Satış bulunamadı veya iade edilemez durumda.");
                        saleId = Convert.ToInt64(o);
                    }

                    // Validate max return qty per sale item
                    foreach (var it in items)
                    {
                        if (it.Quantity <= 0) throw new InvalidOperationException("İade adedi 0 olamaz.");

                        int soldQty;
                        using (var cmdSold = conn.CreateCommand())
                        {
                            cmdSold.Transaction = tx;
                            cmdSold.CommandText = "SELECT Quantity FROM SaleItems WHERE Id = @id AND SaleId = @sid";
                            cmdSold.Parameters.AddWithValue("@id", it.SaleItemId);
                            cmdSold.Parameters.AddWithValue("@sid", saleId);
                            soldQty = Convert.ToInt32(cmdSold.ExecuteScalar());
                        }

                        int alreadyReturned;
                        using (var cmdRet = conn.CreateCommand())
                        {
                            cmdRet.Transaction = tx;
                            cmdRet.CommandText = @"
SELECT IFNULL(SUM(ri.Quantity),0)
FROM SaleReturnItems ri
JOIN SaleReturns r ON r.Id = ri.SaleReturnId
WHERE r.SaleId = @sid AND ri.SaleItemId = @si;";
                            cmdRet.Parameters.AddWithValue("@sid", saleId);
                            cmdRet.Parameters.AddWithValue("@si", it.SaleItemId);
                            alreadyReturned = Convert.ToInt32(cmdRet.ExecuteScalar());
                        }

                        if (alreadyReturned + it.Quantity > soldQty)
                        {
                            throw new InvalidOperationException("İade adedi satılan adedi aşamaz.");
                        }
                    }

                    double total = 0;
                    foreach (var it in items) total += it.UnitPrice * it.Quantity;

                    long returnId;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
INSERT INTO SaleReturns(SaleId, ReturnNo, Total, CreatedAt, CreatedByUserId)
VALUES(@sid, @no, @tot, @dt, @uid);
SELECT last_insert_rowid();";
                        cmd.Parameters.AddWithValue("@sid", saleId);
                        cmd.Parameters.AddWithValue("@no", returnNo);
                        cmd.Parameters.AddWithValue("@tot", total);
                        cmd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("@uid", createdByUserId);
                        returnId = (long)(cmd.ExecuteScalar() ?? 0L);
                    }

                    foreach (var it in items)
                    {
                        using (var cmdRi = conn.CreateCommand())
                        {
                            cmdRi.Transaction = tx;
                            cmdRi.CommandText = @"
INSERT INTO SaleReturnItems(SaleReturnId, SaleItemId, ProductId, Quantity, UnitPrice, LineTotal)
VALUES(@rid, @si, @pid, @q, @p, @lt);";
                            cmdRi.Parameters.AddWithValue("@rid", returnId);
                            cmdRi.Parameters.AddWithValue("@si", it.SaleItemId);
                            cmdRi.Parameters.AddWithValue("@pid", it.ProductId);
                            cmdRi.Parameters.AddWithValue("@q", it.Quantity);
                            cmdRi.Parameters.AddWithValue("@p", it.UnitPrice);
                            cmdRi.Parameters.AddWithValue("@lt", it.UnitPrice * it.Quantity);
                            cmdRi.ExecuteNonQuery();
                        }

                        using (var cmdUpd = conn.CreateCommand())
                        {
                            cmdUpd.Transaction = tx;
                            cmdUpd.CommandText = "UPDATE Products SET StockQty = StockQty + @q WHERE Id = @pid";
                            cmdUpd.Parameters.AddWithValue("@q", it.Quantity);
                            cmdUpd.Parameters.AddWithValue("@pid", it.ProductId);
                            cmdUpd.ExecuteNonQuery();
                        }

                        using (var cmdMv = conn.CreateCommand())
                        {
                            cmdMv.Transaction = tx;
                            cmdMv.CommandText = @"
INSERT INTO StockMovements(ProductId, BarcodeSnapshot, Quantity, Type, Reason, RefType, RefId, CreatedAt, CreatedByUserId)
VALUES(@pid, @b, @q, 'Giriş', 'Return', 'Return', @ref, @dt, @uid);";
                            cmdMv.Parameters.AddWithValue("@pid", it.ProductId);
                            cmdMv.Parameters.AddWithValue("@b", it.BarcodeSnapshot);
                            cmdMv.Parameters.AddWithValue("@q", it.Quantity);
                            cmdMv.Parameters.AddWithValue("@ref", returnId);
                            cmdMv.Parameters.AddWithValue("@dt", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
                            cmdMv.Parameters.AddWithValue("@uid", createdByUserId);
                            cmdMv.ExecuteNonQuery();
                        }
                    }

                    // If fully returned, mark sale as Returned
                    using (var cmdStatus = conn.CreateCommand())
                    {
                        cmdStatus.Transaction = tx;
                        cmdStatus.CommandText = @"
UPDATE Sales
SET Status = CASE 
  WHEN NOT EXISTS (
    SELECT 1 FROM SaleItems si
    WHERE si.SaleId = @sid
      AND (si.Quantity > IFNULL((SELECT SUM(ri.Quantity) FROM SaleReturnItems ri
                                 JOIN SaleReturns r ON r.Id = ri.SaleReturnId
                                 WHERE r.SaleId = @sid AND ri.SaleItemId = si.Id),0))
  )
  THEN 'Returned'
  ELSE 'Completed'
END
WHERE Id = @sid;";
                        cmdStatus.Parameters.AddWithValue("@sid", saleId);
                        cmdStatus.ExecuteNonQuery();
                    }

                    tx.Commit();
                    NotifyDataChanged();
                    return returnId;
                }
            }
        }

        public static DataTable ReportDailySales(DateTime from, DateTime to)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
SELECT
  substr(CreatedAt, 1, 10) AS Day,
  COUNT(1) AS SaleCount,
  SUM(GrandTotal) AS Total
FROM Sales
WHERE Status IN ('Completed','Returned')
  AND datetime(CreatedAt) >= datetime(@from)
  AND datetime(CreatedAt) < datetime(@to)
GROUP BY substr(CreatedAt, 1, 10)
ORDER BY Day DESC;";
                cmd.Parameters.AddWithValue("@from", from.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("@to", to.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static DataTable ReportProductSales(DateTime from, DateTime to)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
SELECT
  si.BarcodeSnapshot AS Barcode,
  si.NameSnapshot AS Name,
  SUM(si.Quantity) AS Qty,
  SUM(si.LineTotal) AS Total
FROM SaleItems si
JOIN Sales s ON s.Id = si.SaleId
WHERE s.Status = 'Completed'
  AND datetime(s.CreatedAt) >= datetime(@from)
  AND datetime(s.CreatedAt) < datetime(@to)
GROUP BY si.BarcodeSnapshot, si.NameSnapshot
ORDER BY Total DESC;";
                cmd.Parameters.AddWithValue("@from", from.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("@to", to.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static DataTable ReportLowStock(int threshold)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
SELECT Barcode, Name, StockQty
FROM Products
WHERE IsActive = 1 AND StockQty <= @t
ORDER BY StockQty ASC, Name;";
                cmd.Parameters.AddWithValue("@t", threshold);
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static DataTable ReportCustomerBalances()
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT Name, Phone, Email, Balance FROM Customers WHERE IsActive = 1 ORDER BY Balance DESC, Name";
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static double GetTotalPendingBalance()
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT SUM(Balance) FROM Customers WHERE IsActive = 1";
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToDouble(result);
            }
        }

        public static DataTable GetTopSellingProducts(int limit)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT p.Name, SUM(si.Quantity) as TotalQty 
FROM SaleItems si
JOIN Products p ON si.ProductId = p.Id
GROUP BY p.Id, p.Name
ORDER BY TotalQty DESC
LIMIT @limit";
                cmd.Parameters.AddWithValue("@limit", limit);
                conn.Open();
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }
}
