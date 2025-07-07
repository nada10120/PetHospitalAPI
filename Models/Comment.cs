using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        public int PostId { get; set; } // The post this comment belongs to


        [ForeignKey("UserId")]
        public User User { get; set; }

        public ICollection<PostComment> PostComments { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; } // Posts that this comment belongs to
    }

}
