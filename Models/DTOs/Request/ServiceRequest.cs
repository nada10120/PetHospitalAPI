using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Request
{
    public class ServiceRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }

        public IFormFile? ImageUrl { get; set; }  // Default image URL



    }
}
