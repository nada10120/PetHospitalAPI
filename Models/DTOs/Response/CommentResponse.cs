using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Response
{
    public class CommentResponse
    {
        public int CommentId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public int PostId { get; set; }
    }
}
