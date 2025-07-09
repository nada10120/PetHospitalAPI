using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Response
{
    public class VetResponse
    {
        public string VetId { get; set; }
        public string Specialization { get; set; }
        public string AvailabilitySchedule { get; set; }
    }
}
