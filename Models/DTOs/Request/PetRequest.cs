using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Request
{
    public class PetRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; } // Cat, Dog, etc.
        public string Breed { get; set; }
        public int Age { get; set; }
        public string MedicalHistory { get; set; }
        public IFormFile? ProfilePicture { get; set; } // For uploading profile picture
        public string? ExistingProfilePicture { get; set; } // For existing profile picture filename
    }
}