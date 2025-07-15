using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Models.DTOs.Request;
using Models.DTOs.Response;

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
=======
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
>>>>>>> 3d6975ec877b2f96f82fbced73ebf5dff70967e7
