using System;
using System.ComponentModel.DataAnnotations;

namespace aspnet.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        public virtual Order? Order { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        public decimal TotalPrice => Quantity * UnitPrice;
    }
} 