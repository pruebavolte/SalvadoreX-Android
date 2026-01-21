using SQLite;

namespace SalvadoreXAndroid.Models
{
    [Table("products")]
    public class Product
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int Stock { get; set; }
        public int MinStock { get; set; }
        public string? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public bool Active { get; set; } = true;
        public bool AvailablePos { get; set; } = true;
        public bool AvailableDigitalMenu { get; set; } = true;
        public bool NeedSync { get; set; } = true;
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    }

    [Table("categories")]
    public class Category
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool Active { get; set; } = true;
        public bool NeedSync { get; set; } = true;
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    }

    [Table("customers")]
    public class Customer
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Rfc { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentCredit { get; set; }
        public int LoyaltyPoints { get; set; }
        public string? Notes { get; set; }
        public bool Active { get; set; } = true;
        public bool NeedSync { get; set; } = true;
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    }

    [Table("sales")]
    public class Sale
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ReceiptNumber { get; set; } = string.Empty;
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = "cash";
        public decimal AmountPaid { get; set; }
        public decimal Change { get; set; }
        public string Status { get; set; } = "completed";
        public string? Notes { get; set; }
        public bool NeedSync { get; set; } = true;
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        [Ignore]
        public List<SaleItem> Items { get; set; } = new();
    }

    [Table("sale_items")]
    public class SaleItem
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SaleId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
    }

    [Table("settings")]
    public class Setting
    {
        [PrimaryKey]
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
    
    public class CartItem
    {
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; } = 1;
        public decimal Total => Product.Price * Quantity;
    }
}
