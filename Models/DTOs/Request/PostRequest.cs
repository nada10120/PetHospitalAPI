using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Request
{
    public class PostRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Content { get; set; }
        public string MediaUrl { get; set; }
    }
}
