using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{


    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        [Required]
        public string Status { get; set; } // Pending, Shipped, Delivered, Cancelled

        // Navigation Properties
        [ForeignKey("UserId")]
        public User User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }


}
