using System.ComponentModel.DataAnnotations;

namespace aspnet.Models
{
    public class Category
    {
        public Category()
        {
            Name = string.Empty;
            Description = string.Empty;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur")]
        [Display(Name = "Kategori Adı")]
        public required string Name { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public List<Product>? Products { get; set; }
    }
} 