using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public Category Category { get; set; }
        public int? CategoryId { get; set; }
        [Required]
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }

        // Navigation Properties
        public ICollection<OrderItem> OrderItems { get; set; }
        public int Quantity { get; set; }
       
        public int Traffic { get; set; }
    }



}
