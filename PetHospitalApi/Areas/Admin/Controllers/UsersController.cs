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
using Utility;
using Microsoft.EntityFrameworkCore;

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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IVetRepository _vetRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<User> userManager,
            IWebHostEnvironment environment,
            IUserRepository userRepository,
            RoleManager<IdentityRole> roleManager,
            IVetRepository vetRepository,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _environment = environment;
            _userRepository = userRepository;
            _roleManager = roleManager;
            _vetRepository = vetRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("Fetching all users");
            var users = await _userRepository.GetAsync();
            var userResponses = new List<UserResponse>();

            foreach (var user in users)
            {
                
                    var roles = await _userManager.GetRolesAsync(user);

                userResponses.Add(user.Adapt<UserResponse>());
                
                userResponses.Last().Role = roles.FirstOrDefault() ?? "";
            }
         

            return Ok(userResponses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation("Fetching user with ID {Id}", id);
            var user = await _userRepository.GetOneAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var response = user.Adapt<UserResponse>();
            response.Role = roles.FirstOrDefault() ?? "";
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] UserRequest userRequest)
        {
            _logger.LogInformation("CreateUser request: {Request}", JsonSerializer.Serialize(userRequest));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogError("CreateUser failed: {Errors}", JsonSerializer.Serialize(errors));
                return BadRequest(new { Errors = errors });
            }

            if (string.IsNullOrEmpty(userRequest.Password))
            {
                return BadRequest(new { Errors = new[] { "Password is required for creating a user." } });
            }

            if (string.IsNullOrEmpty(userRequest.Address))
            {
                userRequest.Address = "Default Address";
            }

            var user = userRequest.Adapt<User>();

            if (userRequest.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userRequest.ProfilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await userRequest.ProfilePicture.CopyToAsync(stream);
                }
                user.ProfilePicture = fileName;
            }

            var result = await _userManager.CreateAsync(user, userRequest.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("CreateUser failed: {Errors}", JsonSerializer.Serialize(result.Errors));
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            if (!await _roleManager.RoleExistsAsync(userRequest.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(userRequest.Role));
            }

            if (userRequest.Role == SD.Vet)
            {
                var vet = new Vet
                {
                    VetId = user.Id,
                    Specialization = userRequest.Specialization ?? "General",
                    AvailabilitySchedule = userRequest.AvailabilitySchedule ?? "N/A"
                };
                var vetCreated = await _vetRepository.CreateAsync(vet);
                if (vetCreated == null)
                {
                    _logger.LogError("Failed to create vet profile for user {UserId}", user.Id);
                    return BadRequest("Failed to create vet profile.");
                }
                await _userManager.AddToRoleAsync(user, userRequest.Role);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, userRequest.Role);
            }

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user.Adapt<UserResponse>());
        }

[HttpPut("{id}")]
public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromForm] UserRequest userRequest)
        {
            _logger.LogInformation("UpdateUser request for ID {Id}: {Request}", id, JsonSerializer.Serialize(userRequest));
            _logger.LogInformation("ExistingProfilePicture received: {ExistingProfilePicture}", userRequest.ExistingProfilePicture);

            // Remove Password and ConfirmPassword from ModelState to make them optional
            if (ModelState.ContainsKey("Password"))
            {
                ModelState.Remove("Password");
            }
            if (ModelState.ContainsKey("ConfirmPassword"))
            {
                ModelState.Remove("ConfirmPassword");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogError("UpdateUser failed: ModelState invalid, Errors: {Errors}", JsonSerializer.Serialize(errors));
                return BadRequest(new { Errors = errors });
            }

            var user = await _userRepository.GetOneAsync(u => u.Id == id);
            if (user == null)
            {
                _logger.LogError("UpdateUser failed: User with ID {Id} not found", id);
                return NotFound();
            }

            if (string.IsNullOrEmpty(userRequest.Address))
            {
                userRequest.Address = user.Address ?? "Default Address";
            }

            // Map user properties manually
            user.Email = userRequest.Email;
            user.UserName = userRequest.UserName;
            user.Address = userRequest.Address;

            // Log ProfilePicture before update
            _logger.LogInformation("ProfilePicture before update: {ProfilePicture}", user.ProfilePicture);

            // Handle ProfilePicture
            if (userRequest.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userRequest.ProfilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await userRequest.ProfilePicture.CopyToAsync(stream);
                    }
                    if (!string.IsNullOrEmpty(user.ProfilePicture))
                    {
                        var oldFilePath = Path.Combine(uploadsFolder, user.ProfilePicture);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldFilePath);
                                _logger.LogInformation("Deleted old profile picture: {OldFilePath}", oldFilePath);
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                _logger.LogWarning("Failed to delete old profile picture {OldFilePath}: {Message}", oldFilePath, ex.Message);
                                // Continue with update despite deletion failure
                            }
                            catch (IOException ex)
                            {
                                _logger.LogWarning("Failed to delete old profile picture {OldFilePath}: {Message}", oldFilePath, ex.Message);
                                // Continue with update despite deletion failure
                            }
                        }
                    }
                    user.ProfilePicture = fileName;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to save new profile picture {FileName}: {Message}", fileName, ex.Message);
                    return StatusCode(500, new { Errors = new[] { "Failed to process profile picture." } });
                }
            }
            else
            {
                // Use ExistingProfilePicture if provided, otherwise preserve existing or set default
                user.ProfilePicture = !string.IsNullOrEmpty(userRequest.ExistingProfilePicture)
                    ? userRequest.ExistingProfilePicture
                    : (user.ProfilePicture ?? "default-profile.png");
            }

            _logger.LogInformation("ProfilePicture after update: {ProfilePicture}", user.ProfilePicture);

            // Only update password if explicitly provided
            if (!string.IsNullOrEmpty(userRequest.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, userRequest.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError("UpdateUser password change failed: {Errors}", JsonSerializer.Serialize(result.Errors));
                    return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("UpdateUser failed: {Errors}", JsonSerializer.Serialize(updateResult.Errors));
                return BadRequest(new { Errors = updateResult.Errors.Select(e => e.Description) });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.FirstOrDefault() != userRequest.Role)
            {
                if (!await _roleManager.RoleExistsAsync(userRequest.Role))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(userRequest.Role));
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError("Failed to create role {Role}: {Errors}", userRequest.Role, JsonSerializer.Serialize(roleResult.Errors));
                        return BadRequest(new { Errors = roleResult.Errors.Select(e => e.Description) });
                    }
                }
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                var roleAssignResult = await _userManager.AddToRoleAsync(user, userRequest.Role);
                if (!roleAssignResult.Succeeded)
                {
                    _logger.LogError("Failed to assign role {Role} to user {UserId}: {Errors}", userRequest.Role, user.Id, JsonSerializer.Serialize(roleAssignResult.Errors));
                    return BadRequest(new { Errors = roleAssignResult.Errors.Select(e => e.Description) });
                }
            }

            if (userRequest.Role == SD.Vet)
            {
                var vet = await _vetRepository.GetOneAsync(v => v.VetId == user.Id);
                if (vet == null)
                {
                    vet = new Vet
                    {
                        VetId = user.Id,
                        Specialization = userRequest.Specialization ?? "General",
                        AvailabilitySchedule = userRequest.AvailabilitySchedule ?? "N/A"
                    };
                    var vetCreated = await _vetRepository.CreateAsync(vet);
                    if (vetCreated == null)
                    {
                        _logger.LogError("Failed to create vet profile for user {UserId}", user.Id);
                        return BadRequest(new { Errors = new[] { "فشل إنشاء ملف الطبيب البيطري." } });
                    }
                }
                else
                {
                    vet.Specialization = userRequest.Specialization ?? vet.Specialization;
                    vet.AvailabilitySchedule = userRequest.AvailabilitySchedule ?? vet.AvailabilitySchedule;
                    await _vetRepository.EditAsync(vet);
                }
            }
            else
            {
                var vet = await _vetRepository.GetOneAsync(v => v.VetId == user.Id);
                if (vet != null)
                {
                    await _vetRepository.DeleteAsync(vet);
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var response = user.Adapt<UserResponse>();
            response.Role = roles.FirstOrDefault() ?? "";
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userRepository.GetOneAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            if(user.Role == SD.Vet)
            {
                var vet = await _vetRepository.GetOneAsync(v => v.VetId == user.Id);
                if (vet != null)
                {
                    await _vetRepository.DeleteAsync(vet);
                }
            }
            var result = await _userManager.DeleteAsync(user);
            
            if (!result.Succeeded)
            {
                _logger.LogError("DeleteUser failed: {Errors}", JsonSerializer.Serialize(result.Errors));
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            return Ok();
        }
    }
}