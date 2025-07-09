using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }

}
