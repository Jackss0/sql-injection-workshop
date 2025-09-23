using System.ComponentModel.DataAnnotations;

namespace SqlInjectionWorkshop.Models.DTOs
{
    public class CommentRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public string Author { get; set; } = string.Empty;
    }
}
