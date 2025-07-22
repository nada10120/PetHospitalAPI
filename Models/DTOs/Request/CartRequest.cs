using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Request
{
    public class CartRequest
    {
        public string ApplicationUserId { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [MinLength(1)]
        public int Count { get; set; }
    }

}
