using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }   // مفتاح أساسي

        [Required]
        public string ApplicationUserId { get; set; } = null!; // المستخدم صاحب السلة

        [Required]
        public int ProductId { get; set; } // المنتج
        public Product Product { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Count { get; set; }
    }
}
