using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using aspnet.Models.Enums;

namespace aspnet.Models
{
    public class Order
    {
        public Order()
        {
            OrderNumber = "";
            FirstName = "";
            LastName = "";
            Email = "";
            Phone = "";
            Address = "";
            City = "";
            District = "";
            PostalCode = "";
            PaymentMethod = "";
            PaymentStatus = "";
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.Now;
            OrderItems = new List<OrderItem>();
            OrderDetails = new List<OrderDetail>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Sipariş numarası gereklidir.")]
        public string OrderNumber { get; set; }

        [Required(ErrorMessage = "Ad alanı gereklidir.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı gereklidir.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon numarası gereklidir.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Adres alanı gereklidir.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "İl alanı gereklidir.")]
        public string City { get; set; }

        [Required(ErrorMessage = "İlçe alanı gereklidir.")]
        public string District { get; set; }

        [Required(ErrorMessage = "Posta kodu gereklidir.")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Ödeme yöntemi gereklidir.")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Ödeme durumu gereklidir.")]
        public string PaymentStatus { get; set; }

        public decimal TotalAmount { get; set; }
        
        [Required]
        public OrderStatus Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        public string FullAddress => $"{Address}, {District}/{City} {PostalCode}";
    }
} 