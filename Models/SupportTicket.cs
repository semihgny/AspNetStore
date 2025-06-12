using System.ComponentModel.DataAnnotations;

namespace aspnet.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }

        public string TicketNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konu alanı zorunludur.")]
        [MinLength(3, ErrorMessage = "Konu en az 3 karakter olmalıdır.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mesaj alanı zorunludur.")]
        [MinLength(10, ErrorMessage = "Mesaj en az 10 karakter olmalıdır.")]
        public string Message { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Status { get; set; } = "Open"; 
        public string Priority { get; set; } = "Normal"; 
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        // Computed property for customer name
        public string CustomerName => $"{Name} ({Email})";

        public ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
    }

    public class SupportMessage
    {
        public int Id { get; set; }
        public int SupportTicketId { get; set; }
        public required SupportTicket SupportTicket { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsStaffReply { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 