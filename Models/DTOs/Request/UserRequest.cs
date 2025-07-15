using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Request
{
    public class UserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } // For create/update

        [Required]
        public string UserName { get; set; }

        public string Address { get; set; }

        [Required]
        public string Role { get; set; } // Client, Vet, Admin

        public IFormFile? ProfilePicture { get; set; } // For create/update
    }
}


