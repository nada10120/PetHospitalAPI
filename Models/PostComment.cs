using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class PostComment
    {
        [Required]
        public int PostId { get; set; }
        [Required]
        public int CommentId { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }
        [ForeignKey("CommentId")]
        public Comment Comment { get; set; }
    }
}