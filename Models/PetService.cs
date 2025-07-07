using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class PetService
    {
        [Key]
        public int PetServiceId { get; set; }
        [Required]
        public int PetId { get; set; }
        [Required]
        public int ServiceId { get; set; }
        public DateTime DatePerformed { get; set; }

        // Navigation Properties
        [ForeignKey("PetId")]
        public Pet Pet { get; set; }
        [ForeignKey("ServiceId")]
        public Service Service { get; set; }
    }
}
