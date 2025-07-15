using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Request;
using Repositories.IRepository;

namespace PetHospitalApi.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IVetRepository _vetRepository;

        public ReservationController(IServiceRepository serviceRepository, IVetRepository vetRepository)
        {
            _serviceRepository = serviceRepository;
            _vetRepository = vetRepository;
        }
        [HttpGet("GetAllServices")]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _serviceRepository.GetAsync();
            if (services == null)
            {
                return NotFound("No services found.");
            }

            return Ok(services.Adapt<IEnumerable<ServiceRequest>>());
        }
        [HttpGet("GetAllVets")]
        public async Task<IActionResult> GetAllVets()
        {
            var vets = await _vetRepository.GetAsync();
            if (vets == null)
            {
                return NotFound("No vets found.");
            }

            return Ok(vets.Adapt<IEnumerable<VetRequest>>());
        }
        [HttpGet("GetVetById/{id}")]
        public async Task<IActionResult> GetVetById([FromRoute] string id)
        {
            var vet = await _vetRepository.GetOneAsync(e => e.VetId == id);
            if (vet == null)
            {
                return NotFound($"Vet with ID {id} not found.");
            }
            return Ok(vet.Adapt<VetRequest>());

        }
        [HttpGet("GetServiceById/{id}")]
        public async Task<IActionResult> GetServiceById([FromRoute] int id)
        {
            var service = await _serviceRepository.GetOneAsync(e => e.ServiceId == id);
            if (service == null)
            {
                return NotFound($"Service with ID {id} not found.");
            }
            return Ok(service.Adapt<ServiceRequest>());
        }

    }
}
