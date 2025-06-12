using System.ComponentModel.DataAnnotations;

namespace aspnet.Models
{
    public class Product
    {
        public Product()
        {
            Name = string.Empty;
            Description = string.Empty;
            ImageUrl = string.Empty;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [Display(Name = "Ürün Adı")]
        public required string Name { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miktarı zorunludur")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0'dan küçük olamaz")]
        [Display(Name = "Stok Miktarı")]
        public int StockQuantity { get; set; }

        [Display(Name = "Resim URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Required(ErrorMessage = "Kategori zorunludur")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
    }
} 