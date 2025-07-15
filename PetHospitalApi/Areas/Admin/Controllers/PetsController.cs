using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Models;
using Repositories.IRepository;
using Mapster;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[Area]/[controller]")]
    [ApiController]
    public class PetsController : ControllerBase
    {
        private readonly IPetRepository _PetRepository;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public PetsController(IPetRepository PetRepository, UserManager<User> userManager, IWebHostEnvironment environment)
        {
            _PetRepository = PetRepository;
            _userManager = userManager;
            _environment = environment; 
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _PetRepository.GetAsync();


            return Ok(categories.Adapt<IEnumerable<PetResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var Pet = await _PetRepository.GetOneAsync(e => e.PetId == id);

            if (Pet is not null)
            {

                return Ok(Pet.Adapt<PetResponse>());
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PetRequest PetRequest)
        {
            if (PetRequest is null)
            {
                return BadRequest("Invalid Pet data.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if the user exists
            var user = await _userManager.FindByIdAsync(PetRequest.UserId);
            if (user is null)
            {
                return BadRequest("User does not exist.");
            }
            var Pet = PetRequest.Adapt<Pet>();
            // Handle profile picture upload if provided
            if (PetRequest.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "pets_profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(PetRequest.ProfilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PetRequest.ProfilePicture.CopyToAsync(stream);
                }
                Pet.ProfilePicture = fileName;
            }
            var PetCreated = await _PetRepository.CreateAsync(Pet);

            if (PetCreated is not null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Categories/{Pet.PetId}", Pet.Adapt<PetResponse>());
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] PetRequest PetRequest)
        {
            if (PetRequest is null)
            {
                return BadRequest("Invalid Pet data.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the user exists
            var user = await _userManager.FindByIdAsync(PetRequest.UserId);
            if (user is null)
            {
                return BadRequest("User does not exist.");
            }

            // Check if the pet exists
            var existingPet = await _PetRepository.GetOneAsync(e => e.PetId == id,tracked:false);
            if (existingPet is null)
            {
                return NotFound("Pet not found.");
            }

            // Map the request to the existing pet
            var Pet = PetRequest.Adapt<Pet>();
            Pet.PetId = existingPet.PetId;

            // Handle profile picture upload if provided
            if (PetRequest.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "pets_profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(PetRequest.ProfilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await PetRequest.ProfilePicture.CopyToAsync(stream);
                    }
                    Pet.ProfilePicture = fileName;
                    Console.WriteLine($"Profile picture saved: {fileName}, Path: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"File upload failed: {ex}");
                    return BadRequest("Failed to upload profile picture.");
                }
            }
            else
            {
                Pet.ProfilePicture = existingPet.ProfilePicture;
            }

            // Log the Pet object
            Console.WriteLine($"Pet to update: {JsonSerializer.Serialize(Pet)}");

            var PetEdited = await _PetRepository.EditAsync(Pet);

            if (PetEdited is not null)
            {
                return NoContent();
            }

            return BadRequest("Failed to update pet. Check server logs for details.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var Pet = await _PetRepository.GetOneAsync(e => e.PetId == id);

            if (Pet is not null)
            {
                // Check if the profile picture file exists and delete it
                var profilePicturePath = Path.Combine(_environment.WebRootPath, "images", "pets_profile", Pet.ProfilePicture);
                if (System.IO.File.Exists(profilePicturePath))
                {
                    System.IO.File.Delete(profilePicturePath);
                }
                var result = await _PetRepository.DeleteAsync(Pet);

                
                    return NoContent();
               
            }

            return NotFound();
        }
    }
}