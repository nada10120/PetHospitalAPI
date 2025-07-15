using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Models.DTOs.Request;
using Models.DTOs.Response;
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
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderRepository.GetAsync();
            var orderDtos = orders.Adapt<List<OrderResponse>>();
            return Ok(orderDtos);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById([FromRoute] int id)
        {
            var order = await _orderRepository.GetOneAsync(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }
            var orderDto = order.Adapt<OrderResponse>();
            return Ok(orderDto);
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest orderRequest)
        {
            if (orderRequest == null)
            {
                return BadRequest("Invalid order data.");
            }
            var order = orderRequest.Adapt<Order>();
            await _orderRepository.CreateAsync(order);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order.Adapt<OrderResponse>());
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder([FromRoute] int id, [FromBody] OrderRequest orderRequest)
        {
            if (orderRequest == null)
            {
                return BadRequest("Invalid order data.");
            }
            var existingOrder = await _orderRepository.GetOneAsync(o => o.OrderId == id);
            if (existingOrder == null)
            {
                return NotFound();
            }
            existingOrder = orderRequest.Adapt(existingOrder);
            await _orderRepository.EditAsync(existingOrder);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder([FromRoute] int id)
        {
            var order = await _orderRepository.GetOneAsync(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }
            await _orderRepository.DeleteAsync(order);
            return NoContent();

        }
    }
}