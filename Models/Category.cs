using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models
{

    public class Category
    {
        public int? CategoryId { get; set; } // Primary Key
        public string Name { get; set; }

        // Navigation Property
        [JsonIgnore]
        public ICollection<Product> Products { get; set; }
    }



}
