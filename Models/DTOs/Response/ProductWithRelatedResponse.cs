using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.DTOs.Request;

namespace Models.DTOs.Response
{
    public class ProductWithRelatedResponse
    {
        public Product Product { get; set; } = null!;

        public List<Product> RelatedProducts { get; set; } = null!;
        public List<Product> TopProducts { get; set; } = null!;
        public List<Product> SameCategoryProducts { get; set; } = null!;
    }
}
