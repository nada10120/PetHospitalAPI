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
        private readonly UserManager<User> _userManager;

        public OrdersController(IOrderRepository orderRepository , UserManager<User> userManager)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderRepository.GetAsync();
            if (orders == null )
            {
                return NotFound("No orders found.");
            }
            return Ok(orders.Adapt<IEnumerable<OrderResponse>>());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById([FromRoute] int id)
        {
            var order = await _orderRepository.GetOneAsync(e => e.OrderId == id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }
            return Ok(order.Adapt<OrderResponse>());
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest orderRequest)
        {
            if (orderRequest == null)
                return BadRequest("Order data is null.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await _userManager.FindByIdAsync(orderRequest.UserId);
            if (user == null)
            {
                return BadRequest("Invalid user");

            }
            var order = orderRequest.Adapt<Order>();
            await _orderRepository.CreateAsync(order);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order.Adapt<OrderResponse>());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder([FromRoute] int id, [FromBody] OrderRequest orderRequest)
        {
            if (orderRequest == null)
                return BadRequest("Order data is null.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var order = await _orderRepository.GetOneAsync(e => e.OrderId == id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }
            order = orderRequest.Adapt(order);
            await _orderRepository.EditAsync(order);
            return Ok(order.Adapt<OrderResponse>());
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder([FromRoute] int id)
        {
            var order = await _orderRepository.GetOneAsync(e => e.OrderId == id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }
            await _orderRepository.DeleteAsync(order);
            return NoContent();
        }
    }

}

