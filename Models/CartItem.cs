using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aspnet.Models
{
    public class CartItem
    {
        public CartItem()
        {
            DateCreated = DateTime.UtcNow;
            Quantity = 1;
        }

        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den büyük olmalıdır")]
        [Display(Name = "Miktar")]
        public int Quantity { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime DateCreated { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Birim Fiyat")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Toplam Tutar")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice => Quantity * UnitPrice;

        // Navigation properties
        public Cart? Cart { get; set; }
        public Product? Product { get; set; }
    }
} 