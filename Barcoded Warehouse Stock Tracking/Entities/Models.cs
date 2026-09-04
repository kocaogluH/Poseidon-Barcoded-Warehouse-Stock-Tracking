using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barcoded_Warehouse_Stock_Tracking.Entities
{
    public class User
    {
        [Key]
        public long Id { get; set; }
        [Required, StringLength(100)]
        public string Username { get; set; }
        [Required, StringLength(255)]
        public string PasswordHash { get; set; }
        [Required, StringLength(50)]
        public string Role { get; set; } // Admin, Personel
        public int IsActive { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    public class Category
    {
        [Key]
        public long Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Product
    {
        [Key]
        public long Id { get; set; }
        [Required, StringLength(100)]
        public string Barcode { get; set; }
        [Required, StringLength(200)]
        public string Name { get; set; }
        public double UnitPrice { get; set; }
        public double CostPrice { get; set; }
        public double VatRate { get; set; }
        public double StockQty { get; set; }
        public int IsActive { get; set; } = 1;
        
        // Metal & Sektörel Özelleştirme Alanları
        public string Category { get; set; } = "";
        public string Unit { get; set; } = "Adet"; // Kg, Metre, Boy (6m), Adet, Plaka, Ton, m2
        public string MaterialType { get; set; } = ""; // Kutu Profil, Boru, Sac, Lama, Köşebent, Mil, Paslanmaz, Alüminyum
        public string QualityStandard { get; set; } = ""; // St37, St52, 304 Paslanmaz, 6063 Alüminyum vb.
        public double Thickness { get; set; } = 0; // Kalınlık / Et Kalınlığı (mm)
        public double Width { get; set; } = 0; // Genişlik / Çap / En (mm)
        public double Length { get; set; } = 0; // Boy / Uzunluk (metre veya mm)
        public double TheoreticalWeight { get; set; } = 0; // Teorik Metre/m2 Ağırlığı (kg)
        public string ShelfLocation { get; set; } = ""; // Depo / Saha / Raf Konumu
        public double CriticalStock { get; set; } = 5;

        // Geriye dönük uyumluluk alanları
        public string Material { get; set; } = "";
        public int BoxQty { get; set; } = 1;
    }

    public class StockMovement
    {
        [Key]
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string BarcodeSnapshot { get; set; }
        public double Quantity { get; set; }
        public string Type { get; set; } // Giriş, Çıkış
        public string Reason { get; set; }
        public string RefType { get; set; }
        public long? RefId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long? CreatedByUserId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    public class Customer
    {
        [Key]
        public long Id { get; set; }
        [Required, StringLength(200)]
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public double Balance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Sale
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string SaleNo { get; set; }
        public long? CustomerId { get; set; }
        public double Subtotal { get; set; }
        public double DiscountTotal { get; set; }
        public double GrandTotal { get; set; }
        public string Status { get; set; } // Completed, Returned
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long? CreatedByUserId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class SaleItem
    {
        [Key]
        public long Id { get; set; }
        public long SaleId { get; set; }
        public long ProductId { get; set; }
        public string BarcodeSnapshot { get; set; }
        public string NameSnapshot { get; set; }
        public string UnitSnapshot { get; set; } = "Adet";
        public double UnitPrice { get; set; }
        public double Quantity { get; set; }
        public double LineTotal { get; set; }

        [ForeignKey("SaleId")]
        public virtual Sale Sale { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    public class Payment
    {
        [Key]
        public long Id { get; set; }
        public long SaleId { get; set; }
        public string Method { get; set; } // Cash, Card
        public double Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("SaleId")]
        public virtual Sale Sale { get; set; }
    }

    public class CustomerTransaction
    {
        [Key]
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string RefType { get; set; }
        public long RefId { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }

    public class CustomerCollection
    {
        [Key]
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string Method { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long? CreatedByUserId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }

    public class SaleReturn
    {
        [Key]
        public long Id { get; set; }
        public long SaleId { get; set; }
        public string ReturnNo { get; set; }
        public double Total { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long? CreatedByUserId { get; set; }

        [ForeignKey("SaleId")]
        public virtual Sale Sale { get; set; }
    }

    public class SaleReturnItem
    {
        [Key]
        public long Id { get; set; }
        public long SaleReturnId { get; set; }
        public long SaleItemId { get; set; }
        public long ProductId { get; set; }
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double LineTotal { get; set; }

        [ForeignKey("SaleReturnId")]
        public virtual SaleReturn SaleReturn { get; set; }
        [ForeignKey("SaleItemId")]
        public virtual SaleItem SaleItem { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    public class Log
    {
        [Key]
        public long Id { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public long? UserId { get; set; }
        public string Details { get; set; }
    }
}
