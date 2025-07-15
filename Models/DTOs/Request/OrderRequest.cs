namespace Models.DTOs.Request
{
    public class OrderRequest
    {
<<<<<<< HEAD
    
        [Key]
        public int OrderId { get; set; }
        [Required]
=======
>>>>>>> 3d6975ec877b2f96f82fbced73ebf5dff70967e7
        public string UserId { get; set; }
        public double TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string Status { get; set; }
    }
}