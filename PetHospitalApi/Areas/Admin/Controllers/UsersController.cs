using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Mapster;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserRepository _userRepository;

        public UsersController(UserManager<User> userManager, IWebHostEnvironment environment, IUserRepository userRepository)
        {
            _userManager = userManager;
            _environment = environment;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAsync();
            var userDtos = users.Adapt<List<UserResponse>>();
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] string id)
        {
            var user = await _userRepository.GetOneAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var userDto = user.Adapt<UserResponse>();
            return Ok(userDto);
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] UserRequest userRequest)
        {
            try
            {
                // Validate request
                if (userRequest == null)
                {
                    return BadRequest("User data is required.");
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(userRequest.Email) ||
                    string.IsNullOrWhiteSpace(userRequest.Password) ||
                    string.IsNullOrWhiteSpace(userRequest.UserName) ||
                    string.IsNullOrWhiteSpace(userRequest.Role))
                {
                    return BadRequest("Email, Password, UserName, and Role are required.");
                }

                // Validate email format
                if (!new EmailAddressAttribute().IsValid(userRequest.Email))
                {
                    return BadRequest("Invalid email format.");
                }

                // Validate password strength (example - adjust as needed)
                if (userRequest.Password.Length < 8)
                {
                    return BadRequest("Password must be at least 8 characters long.");
                }

                // Map to User entity
                var user = userRequest.Adapt<User>();

                // Handle profile picture if provided
                if (userRequest.ProfilePicture != null && userRequest.ProfilePicture.Length > 0)
                {
                    // Validate file size (e.g., 5MB limit)
                    if (userRequest.ProfilePicture.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest("Profile picture must be less than 5MB.");
                    }

                    // Validate file extension
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(userRequest.ProfilePicture.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest("Only JPG, JPEG, PNG, and GIF images are allowed.");
                    }

                    // Create directory if it doesn't exist
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await userRequest.ProfilePicture.CopyToAsync(stream);
                    }

                    user.ProfilePicture = fileName;
                }

                // Create user
                var result = await _userManager.CreateAsync(user, userRequest.Password);
                if (!result.Succeeded)
                {
                    // Clean up uploaded file if user creation failed
                    if (!string.IsNullOrEmpty(user.ProfilePicture))
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile", user.ProfilePicture);
                       
                    }

                    return BadRequest(result.Errors.Select(e => e.Description));
                }

                
               
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user.Adapt<UserResponse>());
            }
            catch (Exception ex)
            {
                // Log the exception (you should have logging configured)
                // _logger.LogError(ex, "Error creating user");

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user.");
            }
        }
    }
}