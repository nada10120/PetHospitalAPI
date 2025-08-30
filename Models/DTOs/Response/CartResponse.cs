using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Response
{
    public class CartResponse
    {
        public int cartId { get; set; } 
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public double ProductPrice { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int Count { get; set; }
    }
}

