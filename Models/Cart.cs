using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aspnet.Models
{
    public class Cart
    {
        public Cart()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            CartItems = new List<CartItem>();
        }

        public int Id { get; set; }

        [Display(Name = "Kullanıcı")]
        public int? UserId { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Display(Name = "Ara Toplam")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Display(Name = "İndirim Tutarı")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Toplam Tutar")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string? AppliedCouponCode { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public ICollection<CartItem> CartItems { get; set; }

        // Helper methods
        public void UpdateTotalAmount()
        {
            SubTotal = CartItems?.Sum(item => item.Quantity * item.UnitPrice) ?? 0;
            TotalAmount = SubTotal - DiscountAmount;
        }

        public int GetTotalItems()
        {
            return CartItems?.Sum(item => item.Quantity) ?? 0;
        }
    }
} 