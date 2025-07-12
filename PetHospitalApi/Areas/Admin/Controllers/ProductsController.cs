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
    public class ProductsController : ControllerBase
    {
        
            private readonly IProductRepository _ProductRepository;

            public ProductsController(IProductRepository ProductRepository)
            {
                _ProductRepository = ProductRepository;
            }

         
            [HttpGet("")]
            public async Task<IActionResult> GetAll()
            {
                var categories = await _ProductRepository.GetAsync();


                return Ok(categories.Adapt<IEnumerable<ProductResponse>>());
            }

            [HttpGet("{id}")]
            public async Task<IActionResult> GetOne([FromRoute] int id)
            {
                var Product = await _ProductRepository.GetOneAsync(e => e.Id == id);

                if (Product is not null)
                {

                    return Ok(Product.Adapt<ProductResponse>());
                }

                return NotFound();
            }

            [HttpPost("")]
            public async Task<IActionResult> Create([FromBody] ProductRequest ProductRequest)
            {
                var Product = await _ProductRepository.CreateAsync(ProductRequest.Adapt<Product>());
                await _ProductRepository.CommitAsync();

                if (Product is not null)
                {
                    return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Categories/{Product.Id}", Product.Adapt<ProductResponse>());
                }

                return BadRequest();
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] ProductRequest ProductRequest)
            {
                var Product = _ProductRepository.Update(ProductRequest.Adapt<Product>());
                await _ProductRepository.CommitAsync();

                if (Product is not null)
                {
                    return NoContent();
                }

                return BadRequest();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete([FromRoute] int id)
            {
                var Product = await _ProductRepository.GetOneAsync(e => e.Id == id);

                if (Product is not null)
                {
                    var result = _ProductRepository.Delete(Product);
                    await _ProductRepository.CommitAsync();

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
