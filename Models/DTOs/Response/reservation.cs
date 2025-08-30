using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Response
{
    public class reservation
    {
        public string VetId { get; set; }
        public string VetName { get; set; }
        public string Specialization { get; set; }
        public string AvailabilitySchedule { get; set; }
    }
}
