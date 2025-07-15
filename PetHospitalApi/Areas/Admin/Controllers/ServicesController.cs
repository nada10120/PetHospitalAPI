using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories.IRepository;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IWebHostEnvironment _environment;

        public ServicesController(IServiceRepository serviceRepository, IWebHostEnvironment environment)
        {
            _serviceRepository = serviceRepository;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _serviceRepository.GetAsync();
            if (services == null )
            {
                return NotFound("No services found.");
            }
            return Ok(services.Adapt<IEnumerable<ServiceResponse>>());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById([FromRoute] int id)
        {
            var service = await _serviceRepository.GetOneAsync(e => e.ServiceId == id);
            if (service == null)
            {
                return NotFound($"Service with ID {id} not found.");
            }
            return Ok(service.Adapt<ServiceResponse>());
        }
        [HttpPost]
        public async Task<IActionResult> CreateService([FromForm] ServiceRequest serviceRequest)
        {
            if (serviceRequest == null)
                return BadRequest("Service data is null.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var service = serviceRequest.Adapt<Service>();
            if (serviceRequest.ImageUrl != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "Services");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(serviceRequest.ImageUrl.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await serviceRequest.ImageUrl.CopyToAsync(stream);
                    }
                    service.ImageUrl = fileName;
                    Console.WriteLine($"Profile picture saved: {fileName}, Path: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"File upload failed: {ex}");
                    return BadRequest("Failed to upload service picture.");
                }
            }
            var createdService = await _serviceRepository.CreateAsync(service);
            if (createdService == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating service.");
            }
            return CreatedAtAction(nameof(GetServiceById), new { id = createdService.ServiceId }, createdService.Adapt<ServiceResponse>());
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService([FromRoute] int id, [FromForm] ServiceRequest serviceRequest)
        {
            if (serviceRequest == null)
                return BadRequest("Service data is null.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var existingService = await _serviceRepository.GetOneAsync(e => e.ServiceId == id,tracked:false);
            if (existingService == null)
            {
                return NotFound($"Service with ID {id} not found.");
            }
            // If an image is provided, handle the upload
            if (serviceRequest.ImageUrl != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "Services");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(serviceRequest.ImageUrl.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await serviceRequest.ImageUrl.CopyToAsync(stream);
                    }
                    existingService.ImageUrl = fileName;
                    Console.WriteLine($"Service picture updated: {fileName}, Path: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"File upload failed: {ex}");
                    return BadRequest("Failed to upload service picture.");
                }
            }
            else
            {
                // If no new image is provided, keep the existing image URL
                existingService.ImageUrl = existingService.ImageUrl ?? "default-service.png"; // Ensure a default image is set
            }

            var serviceToUpdate = new Service
            {
                ServiceId = id, // Ensure the ID is set for the update
                Name = serviceRequest.Name,
                Description = serviceRequest.Description,
                Price = serviceRequest.Price,
                ImageUrl = existingService.ImageUrl // Use the updated image URL


            };
            var updatedService = await _serviceRepository.EditAsync(serviceToUpdate);
            if (updatedService == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating service.");
            }
            return Ok(updatedService.Adapt<ServiceResponse>());
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService([FromRoute] int id)
        {
            var service = await _serviceRepository.GetOneAsync(e => e.ServiceId == id,tracked:false);
            if (service == null)
            {
                return NotFound($"Service with ID {id} not found.");
            }
            var deletedService = await _serviceRepository.DeleteAsync(service);
            if (deletedService == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting service.");
            }
            return Ok(deletedService.Adapt<ServiceResponse>());
        }
    }
}
