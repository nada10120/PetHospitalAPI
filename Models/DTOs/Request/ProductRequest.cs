using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Request
{
    public class ProductRequest
    {
      
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }

    }

}
