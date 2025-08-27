using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Request
{
    public class ProductRequest
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public double Price { get; set; }
        
        public int traffic { get; set; }
        public int? CategoryId { get; set; }

        [Required]
        public int StockQuantity { get; set; }
        public string ExistingImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}