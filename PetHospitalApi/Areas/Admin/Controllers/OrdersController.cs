using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories;


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

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var Order = await _OrderRepository.GetAsync();


            return Ok(Order.Adapt<IEnumerable<OrderResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var order = await _OrderRepository.GetOneAsync(e => e.OrderId == id);

            if (order is not null)
            {

                return Ok(order.Adapt<OrderResponse>());
            }

            return NotFound();
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] OrderRequest orderRequest)
        {
            var order = await _OrderRepository.CreateAsync(orderRequest.Adapt<Order>());

            if (order is not null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Categories/{order.OrderId}", order.Adapt<OrderResponse>());
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] OrderRequest orderRequest)
        {
            var existingorder = await _OrderRepository.GetOneAsync(e => e.OrderId == id);
            if (existingorder is null)
            {
                return NotFound();
            }
            existingorder.Name = orderRequest.Name;
            var order = await _OrderRepository.EditAsync(existingorder);

            if (order is not null)
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var order = await _OrderRepository.GetOneAsync(e => e.OrderId == id);

            if (order is not null)
            {
                var result = await _OrderRepository.DeleteAsync(order);



                return NoContent();
            }

            return NotFound();
        }
    }
}
