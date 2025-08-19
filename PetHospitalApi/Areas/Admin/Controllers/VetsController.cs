using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Models;
using Repositories.IRepository;
using Mapster;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class VetsController : ControllerBase
    {
        private readonly IVetRepository _vetRepository;

        public VetsController(IVetRepository vetRepository)
        {
            _vetRepository = vetRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vets = await _vetRepository.GetAsync();
            return Ok(vets.Adapt<IEnumerable<VetResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] string id)
        {
            var vet = await _vetRepository.GetOneAsync(e => e.VetId == id);
            if (vet != null)
            {
                return Ok(vet.Adapt<VetResponse>());
            }
            return NotFound($"Vet with ID {id} not found.");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VetRequest vetRequest)
        {
            if (vetRequest == null)
                return BadRequest("Vet data is null.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vet = vetRequest.Adapt<Vet>();
            var createdVet = await _vetRepository.CreateAsync(vet);
            await _vetRepository.CommitAsync();

            if (createdVet != null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Vets/{createdVet.VetId}", createdVet.Adapt<VetResponse>());
            }

            return StatusCode(500, "Failed to create vet. Please check the data or server logs.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] string id, [FromBody] VetRequest vetRequest)
        {
            if (vetRequest == null)
                return BadRequest("Vet data is null.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingVet = await _vetRepository.GetOneAsync(e => e.VetId == id);
            if (existingVet == null)
                return NotFound($"Vet with ID {id} not found.");

            vetRequest.Adapt(existingVet);
            var updatedVet = _vetRepository.Update(existingVet);
            await _vetRepository.CommitAsync();

            if (updatedVet != null)
            {
                return NoContent();
            }

            return StatusCode(500, "Failed to update vet. Please check the data or server logs.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            var vet = await _vetRepository.GetOneAsync(e => e.VetId == id);
            if (vet != null)
            {
                var result = _vetRepository.Delete(vet);
                await _vetRepository.CommitAsync();

                if (result)
                {
                    return NoContent();
                }

                return StatusCode(500, "Failed to delete vet. Please check the data or server logs.");
            }

            return NotFound($"Vet with ID {id} not found.");
        }
    }
}