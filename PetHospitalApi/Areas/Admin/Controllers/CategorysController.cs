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

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepository.GetAsync();

       
            return Ok(categories.Adapt<IEnumerable<CategoryResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.CategoryId == id);

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

            if (category is not null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Categories/{category.CategoryId}", category.Adapt<CategoryResponse>());
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] CategoryRequest categoryRequest)
        {
            var existingCategory = await _categoryRepository.GetOneAsync(e => e.CategoryId == id);
            if (existingCategory is null)
            {
                return NotFound();
            }
            existingCategory.Name = categoryRequest.Name;
            var category =await _categoryRepository.EditAsync(existingCategory);

            if (category is not null)
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.CategoryId == id);

            if (category is not null)
            {
                var result = await  _categoryRepository.DeleteAsync(category);

                
                
                    return NoContent();
            }

            return NotFound();
        }
    }
}
