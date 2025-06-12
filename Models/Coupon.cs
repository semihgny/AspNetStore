using System.ComponentModel.DataAnnotations;

namespace aspnet.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kupon kodu gereklidir.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Kupon kodu 3-20 karakter arasında olmalıdır.")]
        public required string Code { get; set; }

        [Required(ErrorMessage = "İndirim miktarı gereklidir.")]
        [Range(0.01, 1000000, ErrorMessage = "İndirim miktarı 0.01'den büyük olmalıdır.")]
        public decimal DiscountAmount { get; set; }

        [Required(ErrorMessage = "İndirim tipi gereklidir.")]
        public required string DiscountType { get; set; } // Percentage veya Fixed

        [Required(ErrorMessage = "Minimum sepet tutarı gereklidir.")]
        [Range(0, 1000000, ErrorMessage = "Minimum sepet tutarı 0'dan büyük olmalıdır.")]
        public decimal MinimumCartAmount { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 