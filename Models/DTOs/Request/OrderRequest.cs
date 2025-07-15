using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Request
{
    public class OrderRequest
    {

        public string Name { get; set; }
        [Key]
        public int OrderId { get; set; }
        [Required]
        public string UserId { get; set; }
        public double TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string Status { get; set; }
    }
}