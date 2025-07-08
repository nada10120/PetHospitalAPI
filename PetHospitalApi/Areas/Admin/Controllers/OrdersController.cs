using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Models.DTOs.Request;
using Models.DTOs.Response;


namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[Area]/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        
       
            private readonly IOrderRepository _OrderRepository;

            public OrdersController(IOrderRepository OrderRepository)
            {
                _OrderRepository = OrderRepository;
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            [HttpGet("")]
            public async Task<IActionResult> GetAll()
            {
                var categories = await _OrderRepository.GetAsync();


                return Ok(categories.Adapt<IEnumerable<OrderResponse>>());
            }

            [HttpGet("{id}")]
            public async Task<IActionResult> GetOne([FromRoute] int id)
            {
                var Order = await _OrderRepository.GetOneAsync(e => e.Id == id);

                if (Order is not null)
                {

                    return Ok(Order.Adapt<OrderResponse>());
                }

                return NotFound();
            }

            [HttpPost("")]
            public async Task<IActionResult> Create([FromBody] OrderRequest OrderRequest)
            {
                var Order = await _OrderRepository.CreateAsync(OrderRequest.Adapt<Order>());
                await _OrderRepository.CommitAsync();

                if (Order is not null)
                {
                    return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Categories/{Order.Id}", Order.Adapt<OrderResponse>());
                }

                return BadRequest();
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] OrderRequest OrderRequest)
            {
                var Order = _OrderRepository.Update(OrderRequest.Adapt<Order>());
                await _OrderRepository.CommitAsync();

                if (Order is not null)
                {
                    return NoContent();
                }

                return BadRequest();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete([FromRoute] int id)
            {
                var Order = await _OrderRepository.GetOneAsync(e => e.Id == id);

                if (Order is not null)
                {
                    var result = _OrderRepository.Delete(Order);
                    await _OrderRepository.CommitAsync();

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
