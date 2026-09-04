using System;
using System.Data.Entity;
using System.Data.SQLite;
using SQLite.CodeFirst;
using Barcoded_Warehouse_Stock_Tracking.Entities;
using System.Linq;

namespace Barcoded_Warehouse_Stock_Tracking.DataAccess
{
    public class WarehouseContext : DbContext
    {
        public WarehouseContext() : base(GetConnection(), true)
        {
            try
            {
                Database.ExecuteSqlCommand(@"
                    CREATE TABLE IF NOT EXISTS Logs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Level TEXT,
                        Action TEXT,
                        Details TEXT,
                        UserId INTEGER,
                        Timestamp TEXT
                    );
                    CREATE TABLE IF NOT EXISTS StockMovements (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductId INTEGER NOT NULL,
                        BarcodeSnapshot TEXT,
                        Type TEXT,
                        Quantity INTEGER NOT NULL,
                        Reason TEXT,
                        RefType TEXT,
                        RefId INTEGER,
                        CreatedByUserId INTEGER,
                        CreatedAt TEXT
                    );
                ");

                // Metal sektörü kolon ekleme migrasyonları
                foreach (var col in new[] {
                    "ALTER TABLE Products ADD COLUMN Unit TEXT NOT NULL DEFAULT 'Adet';",
                    "ALTER TABLE Products ADD COLUMN MaterialType TEXT NOT NULL DEFAULT '';",
                    "ALTER TABLE Products ADD COLUMN QualityStandard TEXT NOT NULL DEFAULT '';",
                    "ALTER TABLE Products ADD COLUMN Thickness REAL NOT NULL DEFAULT 0;",
                    "ALTER TABLE Products ADD COLUMN Width REAL NOT NULL DEFAULT 0;",
                    "ALTER TABLE Products ADD COLUMN Length REAL NOT NULL DEFAULT 0;",
                    "ALTER TABLE Products ADD COLUMN TheoreticalWeight REAL NOT NULL DEFAULT 0;",
                    "ALTER TABLE Products ADD COLUMN ShelfLocation TEXT NOT NULL DEFAULT '';",
                    "ALTER TABLE Products ADD COLUMN CriticalStock REAL NOT NULL DEFAULT 5;",
                    "ALTER TABLE SaleItems ADD COLUMN UnitSnapshot TEXT NOT NULL DEFAULT 'Adet';"
                })
                {
                    try { Database.ExecuteSqlCommand(col); } catch { }
                }
                
                try 
                {
                    Database.ExecuteSqlCommand(@"
                        CREATE TABLE IF NOT EXISTS Categories (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            CreatedAt TEXT NOT NULL
                        );
                    ");
                    // Seed data
                    var count = Database.SqlQuery<int>("SELECT COUNT(*) FROM Categories").FirstOrDefault();
                    if (count == 0)
                    {
                        Database.ExecuteSqlCommand(@"
                            INSERT INTO Categories (Name, CreatedAt) VALUES ('Kutu Profil', datetime('now'));
                            INSERT INTO Categories (Name, CreatedAt) VALUES ('Sanayi & Boru', datetime('now'));
                            INSERT INTO Categories (Name, CreatedAt) VALUES ('Sac Grubu (DKP/Siyah/Galvaniz)', datetime('now'));
                            INSERT INTO Categories (Name, CreatedAt) VALUES ('Dolu Demir & Lama', datetime('now'));
                            INSERT INTO Categories (Name, CreatedAt) VALUES ('Köşebent & NPU/NPI', datetime('now'));
                            INSERT INTO Categories (Name, CreatedAt) VALUES ('Paslanmaz Çelik', datetime('now'));
                            INSERT INTO Categories (Name, CreatedAt) VALUES ('Alüminyum', datetime('now'));
                            INSERT INTO Categories (Name, CreatedAt) VALUES ('Hırdavat & Bağlantı', datetime('now'));
                        ");
                    }
                } 
                catch { }
            }
            catch { }
        }

        private static SQLiteConnection GetConnection()
        {
            // We read the connection string from App.config or fallback
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["WarehouseDb"]?.ConnectionString;
            if (string.IsNullOrEmpty(connStr))
                connStr = "Data Source=warehouse.db;Version=3;";
            return new SQLiteConnection(connStr);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Initialize SQLite DB using CodeFirst
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<WarehouseContext>(modelBuilder);
            System.Data.Entity.Database.SetInitializer(sqliteConnectionInitializer);
            
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
        public DbSet<CustomerCollection> CustomerCollections { get; set; }
        public DbSet<SaleReturn> SaleReturns { get; set; }
        public DbSet<SaleReturnItem> SaleReturnItems { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}
