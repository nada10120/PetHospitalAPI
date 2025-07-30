using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Response
{
    public class PetResponse
    {
        [Key]
        public int PetId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; } // Cat, Dog, etc.
        public string Breed { get; set; }
        public int Age { get; set; }
        public string MedicalHistory { get; set; }
        public string ProfilePicture { get; set; }
    }
}