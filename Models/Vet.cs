using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{

    public class Vet
    {
        [Key]
        [ForeignKey("User")]
        public string VetId { get; set; }
        public string Specialization { get; set; }
        public string AvailabilitySchedule { get; set; }

        // Navigation Properties
        public User User { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        
    }

}
