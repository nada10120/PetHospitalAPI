using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Models.DTOs.Response
{
    public class UserResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string ProfilePicture { get; set; }
        public string Role { get; set; }
        public bool EmailConfirmed { get; set; }
        public int PetCount { get; set; }
        public int AppointmentCount { get; set; }
        public int OrderCount { get; set; }
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
        public string? Specialization { get; set; } // For vets
        public string? AvailabilitySchedule { get; set; } // For vets
    }
}

