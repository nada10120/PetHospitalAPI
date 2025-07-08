using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Response
{
    public class PostResponse
    {
        
        public int PostId { get; set; }
        
        public string UserId { get; set; }
        
        public string Content { get; set; }
        public string MediaUrl { get; set; }
       
        public DateTime CreatedAt { get; set; }
    }
}
