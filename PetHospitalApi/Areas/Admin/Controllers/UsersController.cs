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
using Utility;
using Repositories;

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

        public UsersController(UserManager<User> userManager, IWebHostEnvironment environment, IUserRepository userRepository , RoleManager<IdentityRole> roleManager,IVetRepository vetRepository)
        {
            _userManager = userManager;
            _environment = environment;
            _userRepository = userRepository;
            _roleManager = roleManager;
            _vetRepository = vetRepository;
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
        public async Task <IActionResult> CreateUser([FromForm] UserRequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = userRequest.Adapt<User>();

            // Check if the role is valid
            if (!await _roleManager.RoleExistsAsync(userRequest.Role))
            {
                return BadRequest($"Role '{userRequest.Role}' does not exist.");
            }

            // Handle profile picture upload if provided
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
                return BadRequest(result.Errors.Select(e => e.Description));
            }
            if (user.Role == SD.Vet)
            {
               var vet = new Vet
               {
                   VetId = user.Id,
                  
               };
               var vetcreated= await _vetRepository.CreateAsync(vet);
                if (vetcreated == null)
                {
                    return BadRequest("Failed to create vet profile.");
                }
               
                    // Assign the role to the user
                    await _userManager.AddToRoleAsync(user, user.Role);
                



            }
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user.Adapt<UserResponse>());
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromForm] UserRequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userRepository.GetOneAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            // Check if the role is valid
            if (!await _roleManager.RoleExistsAsync(userRequest.Role))
            {
                return BadRequest($"Role '{userRequest.Role}' does not exist.");
            }
            user = userRequest.Adapt(user);

            // Handle profile picture upload if provided
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
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(user.Adapt<UserResponse>());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            var user = await _userRepository.GetOneAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            // delete the profile picture file if it exists
            var profilePicturePath = Path.Combine(_environment.WebRootPath, "images", "profile", user.ProfilePicture);
            if (System.IO.File.Exists(profilePicturePath))
            {
                System.IO.File.Delete(profilePicturePath);
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }
            return NoContent();
        }
    }
}