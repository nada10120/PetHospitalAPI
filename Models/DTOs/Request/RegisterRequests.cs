using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Request
{
    public class RegisterRequests
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;

        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;

        public string? Address { get; set; }

        public string ProfilePicture { get; set; }

        public string? Role { get; set; } // Client, Vet, Admin


    }
}
