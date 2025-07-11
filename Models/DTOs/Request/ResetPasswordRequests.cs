using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Request
{
    public class ResetPasswordRequests
    {
        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;
    }
}
