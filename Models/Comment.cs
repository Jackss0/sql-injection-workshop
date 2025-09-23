using System.ComponentModel.DataAnnotations;

namespace SqlInjectionWorkshop.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public string Author { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsApproved { get; set; } = false;
    }
}
