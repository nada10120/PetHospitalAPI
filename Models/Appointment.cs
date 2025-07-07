using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{


    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public int PetId { get; set; }
        [Required]
        public string VetId { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        [Required]
        public string Status { get; set; } // Pending, Confirmed, Cancelled

        // Navigation Properties
        [ForeignKey("UserId")]
        public User User { get; set; }
        [ForeignKey("PetId")]
        public Pet Pet { get; set; }
        [ForeignKey("VetId")]
        public Vet Vet { get; set; }
        public ICollection<AppointmentService> AppointmentServices { get; set; }
    }

}
