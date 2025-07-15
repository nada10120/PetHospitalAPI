using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Response
{
    public class OrderResponse
    {
<<<<<<< HEAD
     
        [Key]
=======
>>>>>>> 3d6975ec877b2f96f82fbced73ebf5dff70967e7
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

       
    }
}
