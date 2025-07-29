using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.IRepository;
using Stripe.BillingPortal;


namespace PetHospitalApi.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IEmailSender _emailSender;

        public CheckoutController(
            UserManager<User> userManager,
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _emailSender = emailSender;
        }

        public object Status { get; private set; }

        [HttpPost("success/{orderId}")]
        public async Task<IActionResult> Success(int orderId, SessionService service)
        {
            var applicationUser = await _userManager.GetUserAsync(User);
            if (applicationUser == null)
                return Unauthorized();

            var carts = await _cartRepository.GetAsync(
                e => e.ApplicationUserId == applicationUser.Id,
                includes: [e => e.Product]);

            if (carts == null || !carts.Any())
                return BadRequest("Cart is empty");

            List<OrderItem> orderItems = carts.Select(item => new OrderItem
            {
                OrderId = orderId,
                ProductId = item.ProductId,
                Count = item.Count,
                Price = item.Product.Price
            }).ToList();

            await _orderItemRepository.CreateRangeAsync(orderItems);

            foreach (var item in carts)
            {
                item.Product.Quantity -= item.Count;
            }
            await _cartRepository.CommitAsync();

            await _cartRepository.DeleteRangeAsync(carts);

            var order = _orderRepository.GetOne(e => e.OrderId == orderId);

            var servicee = new SessionService();
            

            await _orderRepository.CommitAsync();

            await _emailSender.SendEmailAsync(applicationUser.Email, "Thanks", "Order Completed");

            return Ok(new { orderId });
        }
    }
}