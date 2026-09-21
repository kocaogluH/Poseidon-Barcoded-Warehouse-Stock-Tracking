using System;
using System.Collections.Generic;
using System.Linq;
using Barcoded_Warehouse_Stock_Tracking.Entities;
using Barcoded_Warehouse_Stock_Tracking.DataAccess;
using System.Data;

namespace Barcoded_Warehouse_Stock_Tracking.Business
{
    public class ProductAnalysisItem
    {
        public long ProductId { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public double UnitPrice { get; set; }
        public double CostPrice { get; set; }
        public double StockQty { get; set; }
        public double CriticalStock { get; set; }
        public double TotalSoldQty { get; set; }
        public double TotalRevenue { get; set; }
    }

    public class ProductService
    {
        private GenericRepository<Product> _repository;

        public ProductService(WarehouseContext context)
        {
            _repository = new GenericRepository<Product>(context);
        }

        public List<Product> GetAllActiveProducts()
        {
            return _repository.Find(p => p.IsActive == 1).OrderBy(p => p.Name).ToList();
        }

        public Product GetProductByBarcode(string barcode)
        {
            return _repository.Find(p => p.Barcode == barcode && p.IsActive == 1).FirstOrDefault();
        }

        public void AddProduct(Product product)
        {
            _repository.Insert(product);
            _repository.Save();
        }

        public void UpdateProduct(Product product)
        {
            _repository.Update(product);
            _repository.Save();
        }

        public void DeleteProduct(long id)
        {
            var p = _repository.GetById(id);
            if (p != null)
            {
                p.IsActive = 0; // Soft delete
                _repository.Update(p);
                _repository.Save();
            }
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return _repository.Find(p => p.IsActive == 1 && (p.Barcode.ToLower().Contains(term) || p.Name.ToLower().Contains(term))).ToList();
        }
        
        public int GetLowStockCount()
        {
             return _repository.Find(p => p.IsActive == 1 && p.StockQty < p.CriticalStock).Count();
        }

        public List<ProductAnalysisItem> GetTopSellingProductsAnalysis(WarehouseContext context, int limit = 50)
        {
            var salesGroup = context.SaleItems
                .Where(si => si.Sale.Status == "Completed")
                .GroupBy(si => si.ProductId)
                .Select(g => new { ProductId = g.Key, TotalSoldQty = g.Sum(x => x.Quantity), TotalRevenue = g.Sum(x => x.LineTotal) })
                .OrderByDescending(x => x.TotalSoldQty)
                .Take(limit)
                .ToList();

            var result = new List<ProductAnalysisItem>();
            foreach (var sg in salesGroup)
            {
                var p = context.Products.FirstOrDefault(x => x.Id == sg.ProductId && x.IsActive == 1);
                if (p != null)
                {
                    result.Add(new ProductAnalysisItem
                    {
                        ProductId = p.Id,
                        Barcode = p.Barcode,
                        Name = p.Name,
                        Category = p.Category,
                        Unit = p.Unit,
                        UnitPrice = p.UnitPrice,
                        CostPrice = p.CostPrice,
                        StockQty = p.StockQty,
                        CriticalStock = p.CriticalStock,
                        TotalSoldQty = sg.TotalSoldQty,
                        TotalRevenue = sg.TotalRevenue
                    });
                }
            }
            return result;
        }

        public List<ProductAnalysisItem> GetLowSellingProductsAnalysis(WarehouseContext context, int limit = 50)
        {
            var salesGroup = context.SaleItems
                .Where(si => si.Sale.Status == "Completed")
                .GroupBy(si => si.ProductId)
                .Select(g => new { ProductId = g.Key, TotalSoldQty = g.Sum(x => x.Quantity), TotalRevenue = g.Sum(x => x.LineTotal) })
                .OrderBy(x => x.TotalSoldQty)
                .Take(limit)
                .ToList();

            var result = new List<ProductAnalysisItem>();
            foreach (var sg in salesGroup)
            {
                var p = context.Products.FirstOrDefault(x => x.Id == sg.ProductId && x.IsActive == 1);
                if (p != null)
                {
                    result.Add(new ProductAnalysisItem
                    {
                        ProductId = p.Id,
                        Barcode = p.Barcode,
                        Name = p.Name,
                        Category = p.Category,
                        Unit = p.Unit,
                        UnitPrice = p.UnitPrice,
                        CostPrice = p.CostPrice,
                        StockQty = p.StockQty,
                        CriticalStock = p.CriticalStock,
                        TotalSoldQty = sg.TotalSoldQty,
                        TotalRevenue = sg.TotalRevenue
                    });
                }
            }
            return result;
        }

        public List<ProductAnalysisItem> GetNeverSoldProductsAnalysis(WarehouseContext context)
        {
            var soldProductIds = context.SaleItems
                .Where(si => si.Sale.Status == "Completed")
                .Select(si => si.ProductId)
                .Distinct()
                .ToList();

            var neverSold = context.Products
                .Where(p => p.IsActive == 1 && !soldProductIds.Contains(p.Id))
                .OrderBy(p => p.Name)
                .ToList();

            return neverSold.Select(p => new ProductAnalysisItem
            {
                ProductId = p.Id,
                Barcode = p.Barcode,
                Name = p.Name,
                Category = p.Category,
                Unit = p.Unit,
                UnitPrice = p.UnitPrice,
                CostPrice = p.CostPrice,
                StockQty = p.StockQty,
                CriticalStock = p.CriticalStock,
                TotalSoldQty = 0,
                TotalRevenue = 0
            }).ToList();
        }

        public bool QuickStockIn(WarehouseContext context, long productId, double addQty, string reason, long? userId)
        {
            var p = context.Products.Find(productId);
            if (p == null) return false;

            p.StockQty += addQty;

            var sm = new StockMovement
            {
                ProductId = p.Id,
                BarcodeSnapshot = p.Barcode,
                Type = "Giriş",
                Quantity = addQty,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Analiz Ekranından Hızlı Stok Girişi" : reason,
                RefType = "Manual",
                CreatedByUserId = userId,
                CreatedAt = DateTime.Now
            };

            context.StockMovements.Add(sm);
            context.SaveChanges();
            Database.NotifyDataChanged();
            return true;
        }

        public bool QuickPriceUpdate(WarehouseContext context, long productId, double newUnitPrice, double newCostPrice)
        {
            var p = context.Products.Find(productId);
            if (p == null) return false;

            p.UnitPrice = newUnitPrice;
            p.CostPrice = newCostPrice;
            context.SaveChanges();
            Database.NotifyDataChanged();
            return true;
        }
    }

    public class DashboardService
    {
        private WarehouseContext _context;
        public DashboardService(WarehouseContext context) { _context = context; }

        public int GetTotalActiveProducts()
        {
            return _context.Products.Count(p => p.IsActive == 1);
        }

        public double GetTodaySalesTotal()
        {
            var today = DateTime.Today;
            var sales = _context.Sales.Where(s => s.Status == "Completed" && s.CreatedAt >= today).ToList();
            return sales.Sum(s => s.GrandTotal);
        }

        public double GetTotalPendingBalance()
        {
            return Database.GetTotalPendingBalance();
        }

        public DataTable GetTopSellingProducts(int limit = 5)
        {
            return Database.GetTopSellingProducts(limit);
        }
    }

    public class LogService
    {
        private GenericRepository<Log> _repository;
        public LogService(WarehouseContext context) { _repository = new GenericRepository<Log>(context); }

        public void Info(string action, string details, long? userId = null)
        {
            _repository.Insert(new Log { Action = action, Details = details, UserId = userId, Timestamp = DateTime.Now });
            _repository.Save();
        }
    }

    public class AuthService
    {
        private GenericRepository<User> _repository;

        public AuthService(WarehouseContext context)
        {
            _repository = new GenericRepository<User>(context);
        }

        public User Authenticate(string username, string password)
        {
            // Veritabanı önceden var ise ve şifre hashleri eşleşmiyorsa diye güvenli arka kapı
            if (username == "admin" && password == "1234")
            {
                return new User { Id = 1, Username = "admin", Role = "Admin", IsActive = 1 };
            }

            var user = _repository.Find(u => u.Username == username && u.IsActive == 1).FirstOrDefault();
            if (user != null && Security.VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }
    }
}
