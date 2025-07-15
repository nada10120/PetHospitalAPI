using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories.IRepository;
using Repositories;

using Microsoft.AspNetCore.Identity;


namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
     

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var order = await _orderRepository.GetAsync();


            return Ok(order.Adapt<IEnumerable<OrderResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var Order = await _orderRepository.GetOneAsync(e => e.OrderId == id);

            if (Order is not null)
            {

                return Ok(Order.Adapt<OrderResponse>());
            }

            return NotFound();
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] OrderRequest OrderRequest)
        {
            var Order = await _orderRepository.CreateAsync(OrderRequest.Adapt<Order>());

            if (Order is not null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Categories/{Order.OrderId}", Order.Adapt<OrderResponse>());
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] OrderRequest OrderRequest)
        {
            var existingOrder = await _orderRepository.GetOneAsync(e => e.OrderId == id);
            if (existingOrder is null)
            {
                return NotFound();
            }
            existingOrder.Name = OrderRequest.Name;
            var Order = await _orderRepository.EditAsync(existingOrder);

            if (Order is not null)
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var Order = await _orderRepository.GetOneAsync(e => e.OrderId == id);

            if (Order is not null)
            {
                var result = await _orderRepository.DeleteAsync(Order);



                return NoContent();
            }

            return NotFound();
        }
    }
}

