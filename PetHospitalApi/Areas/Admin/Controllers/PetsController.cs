using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories.IRepository;
using Mapster;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class PetsController : ControllerBase
    {
        private readonly IPetRepository _PetRepository;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PetsController> _logger;

        public PetsController(
            IPetRepository PetRepository,
            UserManager<User> userManager,
            IWebHostEnvironment environment,
            ILogger<PetsController> logger)
        {
            _PetRepository = PetRepository;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var pets = await _PetRepository.GetAsync();
            return Ok(pets.Adapt<IEnumerable<PetResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var pet = await _PetRepository.GetOneAsync(e => e.PetId == id);
            if (pet is not null)
            {
                return Ok(pet.Adapt<PetResponse>());
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PetRequest petRequest)
        {
            _logger.LogInformation("CreatePet request: {Request}", JsonSerializer.Serialize(petRequest));

            if (petRequest is null)
            {
                _logger.LogError("CreatePet failed: Invalid Pet data.");
                return BadRequest("Invalid Pet data.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogError("CreatePet failed: ModelState invalid, Errors: {Errors}", JsonSerializer.Serialize(errors));
                return BadRequest(new { Errors = errors });
            }

            var user = await _userManager.FindByIdAsync(petRequest.UserId);
            if (user is null)
            {
                _logger.LogError("CreatePet failed: User with ID {UserId} not found", petRequest.UserId);
                return BadRequest("User does not exist.");
            }

            var pet = petRequest.Adapt<Pet>();

            // Handle profile picture upload if provided
            if (petRequest.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "pets_profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(petRequest.ProfilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await petRequest.ProfilePicture.CopyToAsync(stream);
                    }
                    pet.ProfilePicture = fileName;
                    _logger.LogInformation("Profile picture saved: {FileName}, Path: {FilePath}", fileName, filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to save profile picture {FileName}: {Message}", fileName, ex.Message);
                    return StatusCode(500, new { Errors = new[] { "Failed to process profile picture." } });
                }
            }
            else
            {
                pet.ProfilePicture = "default-pet.png"; // Set default if no picture is provided
            }

            var petCreated = await _PetRepository.CreateAsync(pet);
            if (petCreated is not null)
            {
                _logger.LogInformation("Pet created with ID: {PetId}", pet.PetId);
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Pets/{pet.PetId}", pet.Adapt<PetResponse>());
            }

            _logger.LogError("CreatePet failed: Unable to create pet.");
            return BadRequest("Failed to create pet.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] PetRequest petRequest)
        {
            _logger.LogInformation("EditPet request for ID {Id}: {Request}", id, JsonSerializer.Serialize(petRequest));
            _logger.LogInformation("ExistingProfilePicture received: {ExistingProfilePicture}", petRequest.ExistingProfilePicture);

            if (petRequest is null)
            {
                _logger.LogError("EditPet failed: Invalid Pet data.");
                return BadRequest("Invalid Pet data.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogError("EditPet failed: ModelState invalid, Errors: {Errors}", JsonSerializer.Serialize(errors));
                return BadRequest(new { Errors = errors });
            }

            var user = await _userManager.FindByIdAsync(petRequest.UserId);
            if (user is null)
            {
                _logger.LogError("EditPet failed: User with ID {UserId} not found", petRequest.UserId);
                return BadRequest("User does not exist.");
            }

            var existingPet = await _PetRepository.GetOneAsync(e => e.PetId == id, tracked: false);
            if (existingPet is null)
            {
                _logger.LogError("EditPet failed: Pet with ID {Id} not found", id);
                return NotFound("Pet not found.");
            }

            // Map properties manually to avoid null issues
            var pet = new Pet
            {
                PetId = existingPet.PetId,
                UserId = petRequest.UserId,
                Name = petRequest.Name,
                Type = petRequest.Type,
                Breed = petRequest.Breed,
                Age = petRequest.Age,
                MedicalHistory = petRequest.MedicalHistory
            };

            // Handle profile picture
            _logger.LogInformation("ProfilePicture before update: {ProfilePicture}", existingPet.ProfilePicture);
            if (petRequest.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "pets_profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(petRequest.ProfilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await petRequest.ProfilePicture.CopyToAsync(stream);
                    }
                    if (!string.IsNullOrEmpty(existingPet.ProfilePicture) && existingPet.ProfilePicture != "default-pet.png")
                    {
                        var oldFilePath = Path.Combine(uploadsFolder, existingPet.ProfilePicture);
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
                            }
                            catch (IOException ex)
                            {
                                _logger.LogWarning("Failed to delete old profile picture {OldFilePath}: {Message}", oldFilePath, ex.Message);
                            }
                        }
                    }
                    pet.ProfilePicture = fileName;
                    _logger.LogInformation("Profile picture saved: {FileName}, Path: {FilePath}", fileName, filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to save profile picture {FileName}: {Message}", fileName, ex.Message);
                    return StatusCode(500, new { Errors = new[] { "Failed to process profile picture." } });
                }
            }
            else
            {
                pet.ProfilePicture = !string.IsNullOrEmpty(petRequest.ExistingProfilePicture)
                    ? petRequest.ExistingProfilePicture
                    : (existingPet.ProfilePicture ?? "default-pet.png");
            }

            _logger.LogInformation("ProfilePicture after update: {ProfilePicture}", pet.ProfilePicture);

            var petEdited = await _PetRepository.EditAsync(pet);
            if (petEdited is not null)
            {
                _logger.LogInformation("Pet updated with ID: {PetId}", pet.PetId);
                return NoContent();
            }

            _logger.LogError("EditPet failed: Unable to update pet.");
            return BadRequest("Failed to update pet. Check server logs for details.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var pet = await _PetRepository.GetOneAsync(e => e.PetId == id);
            if (pet is not null)
            {
                if (!string.IsNullOrEmpty(pet.ProfilePicture) && pet.ProfilePicture != "default-pet.png")
                {
                    var profilePicturePath = Path.Combine(_environment.WebRootPath, "images", "pets_profile", pet.ProfilePicture);
                    if (System.IO.File.Exists(profilePicturePath))
                    {
                        try
                        {
                            System.IO.File.Delete(profilePicturePath);
                            _logger.LogInformation("Deleted profile picture: {ProfilePicturePath}", profilePicturePath);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            _logger.LogWarning("Failed to delete profile picture {ProfilePicturePath}: {Message}", profilePicturePath, ex.Message);
                        }
                        catch (IOException ex)
                        {
                            _logger.LogWarning("Failed to delete profile picture {ProfilePicturePath}: {Message}", profilePicturePath, ex.Message);
                        }
                    }
                }
                await _PetRepository.DeleteAsync(pet);
                
                    _logger.LogInformation("Pet deleted with ID: {PetId}", id);
                    return NoContent();
                
                
            }

            _logger.LogError("DeletePet failed: Pet with ID {Id} not found", id);
            return NotFound();
        }
    }
}