using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Request
{
    public class ResendEmailRequest
    {
        [Required]
        public string EmailOrUserName { get; set; } = null!;
    }
}
