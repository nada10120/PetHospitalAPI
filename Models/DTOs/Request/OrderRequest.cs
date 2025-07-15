namespace Models.DTOs.Request
{
    public class OrderRequest
    {
        public string UserId { get; set; }
        public double TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string Status { get; set; }
    }
}