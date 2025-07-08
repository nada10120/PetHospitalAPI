using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories.IRepository;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[Area]/[controller]")]
    [ApiController]
    public class CategorysController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategorysController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepository.GetAsync();

       
            return Ok(categories.Adapt<IEnumerable<CategoryResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id);

            if (category is not null)
            {

                return Ok(category.Adapt<CategoryResponse>());
            }

            return NotFound();
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CategoryRequest categoryRequest)
        {
            var category = await _categoryRepository.CreateAsync(categoryRequest.Adapt<Category>());
            await _categoryRepository.CommitAsync();

            if (category is not null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Categories/{category.Id}", category.Adapt<CategoryResponse>());
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] CategoryRequest categoryRequest)
        {
            var category = _categoryRepository.Update(categoryRequest.Adapt<Category>());
            await _categoryRepository.CommitAsync();

            if (category is not null)
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id);

            if (category is not null)
            {
                var result = _categoryRepository.Delete(category);
                await _categoryRepository.CommitAsync();

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