using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{

    public class Like
    {
        [Key]
        public int PostLikeId { get; set; }
        [Required]
        public int PostId { get; set; }
        [Required]
        public string UserId { get; set; }

        // Navigation Properties
        [ForeignKey("PostId")]
        public Post Post { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }


}
