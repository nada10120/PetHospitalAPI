using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Response
{
    public class CategoryResponse
    {
        public int CategoryId { get; set; } // Primary Key
        public string Name { get; set; }
    }
}
