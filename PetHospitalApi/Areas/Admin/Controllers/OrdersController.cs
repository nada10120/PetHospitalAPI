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
        private readonly UserManager<User> _userManager;

        public OrdersController(IOrderRepository orderRepository, UserManager<User> userManager)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderRepository.GetAsync();
            return Ok(orders.Adapt<IEnumerable<OrderResponse>>());
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

            var user = await _userManager.FindByIdAsync(OrderRequest.UserId);
            if (user is null)
            {
                return BadRequest("Invalid UserId: User does not exist.");
            }

            var order = new Order
            {
                UserId = OrderRequest.UserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = OrderRequest.TotalAmount,
                ShippingAddress = OrderRequest.ShippingAddress ?? "Default Address",
                Status = OrderRequest.Status ?? "Pending"
            };

            var createdOrder = await _orderRepository.CreateAsync(order);
            if (createdOrder is not null)
            {
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Orders/{createdOrder.OrderId}", createdOrder.Adapt<OrderResponse>());
            }
            return BadRequest("Failed to create order. Check server logs for details.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] OrderRequest orderRequest)
        {
                var Order = await _orderRepository.GetOneAsync(e => e.OrderId == id);

                var user = await _userManager.FindByIdAsync(orderRequest.UserId);
                if (user is null)
                {
                    return BadRequest("Invalid UserId: User does not exist.");
                }

                var updatedOrder = orderRequest.Adapt<Order>();
                updatedOrder.OrderId = id;
                var result = await _orderRepository.EditAsync(updatedOrder);
                if (result is not null)
                {
                    return NoContent();
                }
                return BadRequest("Failed to update order. Check server logs for details.");
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            {
                var order = await _orderRepository.GetOneAsync(e => e.OrderId == id);
                if (order is not null)
                {
                    await _orderRepository.DeleteAsync(order);
                    return NoContent();
                }
                return NotFound();
            }
    }
} 