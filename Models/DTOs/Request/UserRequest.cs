using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class UserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string? Password { get; set; } // Optional for update



    [Required]
    public string UserName { get; set; }

    public string Address { get; set; }

    [Required]
    public string Role { get; set; } // Client, Vet, Admin

    public string? Specialization { get; set; } // For vets
    public string? AvailabilitySchedule { get; set; } // For vets

    public IFormFile? ProfilePicture { get; set; }
    public string? ExistingProfilePicture { get; set; }
}