using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Response
{
    public class AppointmentResponse
    {
        public int AppointmentId { get; set; }
        public string UserId { get; set; }
        public int PetId { get; set; }
        public string VetId { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
    }
}
