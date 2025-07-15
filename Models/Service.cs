using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public double Price { get; set; }

        public string ImageUrl { get; set; } = "default-service.png"; // Default image URL

        // Navigation Properties
        public ICollection<PetService> PetServices { get; set; }
        public ICollection<AppointmentService> AppointmentServices { get; set; }
    }

}
