using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Models
{

    public class User : IdentityUser
    {
      
     
        public string Address { get; set; }
        public string ProfilePicture { get; set; }
        public string Role { get; set; } // Client, Vet, Admin

        // Navigation Properties
        public ICollection<Pet> Pets { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Like> Likes { get; set; }
        public Vet Vet { get; set; } // For users with Role = Vet
    }
}
