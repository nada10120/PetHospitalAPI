using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Models
{


    public class Post
    {
        [Key]
        public int PostId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Content { get; set; }
        public string MediaUrl { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<PostComment> PostComments { get; set; }
        public ICollection<Like> Likes { get; set; }
    }
}
