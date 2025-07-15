using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{

    public class Pet
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
        public string ProfilePicture { get; set; } = "default-pet.png"; // Default profile picture

        // Navigation Properties
        [ForeignKey("UserId")]
        public User User { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<PetService> PetServices { get; set; }
     
    }

}
