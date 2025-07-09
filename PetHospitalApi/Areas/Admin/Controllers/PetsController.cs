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
    public class PetsController : ControllerBase
    {
        private readonly IPetRepository _PetRepository;

        public PetsController(IPetRepository PetRepository)
        {
            _PetRepository = PetRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _PetRepository.GetAsync();


            return Ok(categories.Adapt<IEnumerable<PetResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var Pet = await _PetRepository.GetOneAsync(e => e.Id == id);

            if (Pet is not null)
            {

                return Ok(Pet.Adapt<PetResponse>());
            }

            return NotFound();
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] PetRequest PetRequest)
        {
            var Pet = await _PetRepository.CreateAsync(PetRequest.Adapt<Pet>());
            await _PetRepository.CommitAsync();

            if (Pet is not null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Categories/{Pet.Id}", Pet.Adapt<PetResponse>());
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] PetRequest PetRequest)
        {
            var Pet = _PetRepository.Update(PetRequest.Adapt<Pet>());
            await _PetRepository.CommitAsync();

            if (Pet is not null)
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var Pet = await _PetRepository.GetOneAsync(e => e.Id == id);

            if (Pet is not null)
            {
                var result = _PetRepository.Delete(Pet);
                await _PetRepository.CommitAsync();

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