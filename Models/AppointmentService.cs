using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class AppointmentService
    {
        [Key]
        public int AppointmentServiceId { get; set; }
        [Required]
        public int AppointmentId { get; set; }
        [Required]
        public int ServiceId { get; set; }

        // Navigation Properties
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }
        [ForeignKey("ServiceId")]
        public Service Service { get; set; }
    }
}
