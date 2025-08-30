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
        public int ProductId { get; set; }
        public int Count { get; set; }
    }
}
