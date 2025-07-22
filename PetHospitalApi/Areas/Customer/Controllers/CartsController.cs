using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Models;
using Repositories.IRepository;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using DataManager;
using Microsoft.Extensions.FileProviders;

namespace PetHospitalApi.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("api/[Area]/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ICartRepository _cartRepository;
        private readonly ApplicationDbContext _context;
        private readonly IOrderRepository _orderRepository;

        public CartsController(UserManager<User> userManager, ICartRepository cartRepository, ApplicationDbContext context, IOrderRepository orderRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _context = context;
            _orderRepository = orderRepository;
        }
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromRoute] int id, [FromQuery] int count)
        {
            var appUserId = _userManager.GetUserId(User);

            if (appUserId is not null)
            {
                Cart cart = new Cart()
                {
                    ApplicationUserId = appUserId,
                    ProductId = id,
                    Count = count
                };

                var cartInDb = _cartRepository.GetOne(e => e.ApplicationUserId == appUserId && e.ProductId == id);

                if (cartInDb != null)
                    cartInDb.Count += count;
                else
                    await _cartRepository.CreateAsync(cart);

                return NoContent();
            }


            return NotFound();
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var applicationUser = await _userManager.GetUserAsync(User);

            if (applicationUser is not null)
            {
                var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == applicationUser.Id, includes: [e => e.Product]);

                var totalPrice = carts.Sum(e => e.Product.Price * e.Count);

                return Ok(carts.Adapt<IEnumerable<CartResponse>>());
            }  

            return BadRequest();
        }
        [HttpPatch("IncrementCount")]
        public async Task<IActionResult> IncrementCount([FromQuery]int productId)
        {
            var applicationUser = await _userManager.GetUserAsync(User);

            if (applicationUser is not null)
            {
                var cart = _cartRepository.GetOne(e => e.ProductId == productId && e.ApplicationUserId == applicationUser.Id);
               
                if (cart is null)
                {
                    cart.Count++;
                    await _cartRepository.CommitAsync();
                    return NoContent();

                }

                return BadRequest();

            }

            return NotFound();
        }
        [HttpPatch("DecrementCount")]
        public async Task<IActionResult> DecrementCount([FromQuery]int productId)
        {
            var applicationUser = await _userManager.GetUserAsync(User);

            if (applicationUser is not null)
            {
                var cart = _cartRepository.GetOne(e => e.ProductId == productId && e.ApplicationUserId == applicationUser.Id);
                if (cart is not null)
                {
                    if (cart.Count > 1)
                    {
                        cart.Count--;
                        await _cartRepository.CommitAsync();
                    }
                    else
                    {
                        await _cartRepository.DeleteAsync(cart);
                    }
                    return NoContent();
                }
                return BadRequest();
            }

            return NotFound();
        }
        [HttpPatch("DeleteItem")]
        public async Task<IActionResult> DeleteItem([FromQuery]int productId)
        {
            var applicationUser = await _userManager.GetUserAsync(User);

            if (applicationUser is not null)
            {
                var cart = _cartRepository.GetOne(e => e.ProductId == productId && e.ApplicationUserId == applicationUser.Id);
                if (cart is not null)
                {
                    await _cartRepository.DeleteAsync(cart);
                    return NoContent();
                }

                return BadRequest();
            }

            return NotFound();
        }

      

            //[HttpPost]
            //public async Task<IActionResult> Pay()
            //{
            //    var applicationUser = await _userManager.GetUserAsync(User);

            //    var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == applicationUser.Id, includes: [e => e.Product]);

            //    if (applicationUser is not null && carts is not null)
            //    {
            //        Order order = new()
            //        {
            //            ApplicationUserId = applicationUser.Id,
            //            OrderDate = DateTime.UtcNow,
            //            OrderStatus = OrderStatus.Pending,
            //            TotalPrice = carts.Sum(e => e.Product.Price * e.Count),
            //            TransactionStatus = false
            //        };

            //        await _orderRepository.CreateAsync(order);

            //        var options = new SessionCreateOptions
            //        {
            //            PaymentMethodTypes = new List<string> { "card" },
            //            LineItems = new List<SessionLineItemOptions>(),
            //            Mode = "payment",
            //            SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Success?orderId={order.Id}",
            //            CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Cancel",
            //        };

            //        foreach (var item in carts)
            //        {
            //            options.LineItems.Add(new SessionLineItemOptions
            //            {
            //                PriceData = new SessionLineItemPriceDataOptions
            //                {
            //                    Currency = "egp",
            //                    ProductData = new SessionLineItemPriceDataProductDataOptions
            //                    {
            //                        Name = item.Product.Name,
            //                        Description = item.Product.Description,
            //                    },
            //                    UnitAmount = (long)item.Product.Price * 100,
            //                },
            //                Quantity = item.Count,
            //            });
            //        }

            //        var service = new SessionService();
            //        var session = service.Create(options);

            //        order.SessionId = session.Id;
            //        await _orderRepository.CommitAsync();


            //        return Redirect(session.Url);
            //    }

            //    return BadRequest();

            //}
        }
    }
