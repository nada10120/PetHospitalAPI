using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Request
{
    public class LoginRequests
    {
        [Required]
        public string EmailOrUserName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}
