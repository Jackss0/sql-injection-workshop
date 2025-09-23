using System.ComponentModel.DataAnnotations;

namespace SqlInjectionWorkshop.Models.DTOs
{
    public class UpdateUserRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public bool IsAdmin { get; set; }
    }
}
