using System.ComponentModel.DataAnnotations;

namespace SqlInjectionWorkshop.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public string Category { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
    }
}
