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
    [Route("api/[Area]/[controller]")]
    [ApiController]
    public class VetsController : ControllerBase
    {
        private readonly IVetRepository _VetRepository;

        public VetsController(IVetRepository VetRepository)
        {
            _VetRepository = VetRepository;
        }

      
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _VetRepository.GetAsync();


            return Ok(categories.Adapt<IEnumerable<VetResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] string id)
        {
            var Vet = await _VetRepository.GetOneAsync(e => e.VetId == id);

            if (Vet is not null)
            {

                return Ok(Vet.Adapt<VetResponse>());
            }

            return NotFound();
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] VetRequest VetRequest)
        {
            var Vet = await _VetRepository.CreateAsync(VetRequest.Adapt<Vet>());
            await _VetRepository.CommitAsync();

            if (Vet is not null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Categories/{Vet.VetId}", Vet.Adapt<VetResponse>());
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] VetRequest VetRequest)
        {
            var Vet = _VetRepository.Update(VetRequest.Adapt<Vet>());
            await _VetRepository.CommitAsync();

            if (Vet is not null)
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            var Vet = await _VetRepository.GetOneAsync(e => e.VetId == id);

            if (Vet is not null)
            {
                var result = _VetRepository.Delete(Vet);
                await _VetRepository.CommitAsync();

                if (result)
                {
                    return NoContent();
                }

                return BadRequest();
            }

            return NotFound();
        }
    }
}