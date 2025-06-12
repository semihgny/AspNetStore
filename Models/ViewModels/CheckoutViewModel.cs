using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using aspnet.Models;

namespace aspnet.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public IEnumerable<CartItem> CartItems { get; set; } = new List<CartItem>();
        public Cart? Cart { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon alanı zorunludur")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres satırı zorunludur")]
        [Display(Name = "Adres")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "İl alanı zorunludur")]
        [Display(Name = "İl")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "İlçe alanı zorunludur")]
        [Display(Name = "İlçe")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Posta kodu zorunludur")]
        [Display(Name = "Posta Kodu")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Posta kodu 5 haneli olmalıdır")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ödeme yöntemi seçiniz")]
        [Display(Name = "Ödeme Yöntemi")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";
    }
} 