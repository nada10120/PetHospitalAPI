using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Models;
using Repositories.IRepository;
using System.Linq.Expressions;

[Area("Customer")]
[Route("api/[Area]/[controller]")]
[ApiController]
[Authorize] // خلي الـ cart مرتبط بالـ user اللي مسجل دخول
public class CartsController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ICartRepository _cartRepository;

    public CartsController(UserManager<User> userManager, ICartRepository cartRepository)
    {
        _userManager = userManager;
        _cartRepository = cartRepository;
    }

    private async Task<User?> GetCurrentUserAsync() => await _userManager.GetUserAsync(User);

    [HttpPost("AddToCart")]
    public async Task<IActionResult> AddToCart([FromBody] CartRequest request)
    {
        if (request.Count <= 0) return BadRequest("Count must be greater than zero.");

        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        var cartItem = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == request.ProductId);

        if (cartItem != null)
        {
            cartItem.Count += request.Count;
            await _cartRepository.CommitAsync();
        }
        else
        {
            var cart = new Cart
            {
                ApplicationUserId = user.Id,
                ProductId = request.ProductId,
                Count = request.Count
            };
            await _cartRepository.CreateAsync(cart);
        }

        return Ok(new { message = "Item added successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        var includes = new List<Expression<Func<Cart, object>>> { e => e.Product };
        var carts = await _cartRepository.GetAsync(
            e => e.ApplicationUserId == user.Id,
            includes: includes.ToArray()
        );

        var response = carts.Select(e => new CartResponse
        {
            ProductId = e.ProductId,
            ProductName = e.Product.Name,
            ProductPrice = e.Product.Price,
            ImageUrl = e.Product.ImageUrl,
            Count = e.Count
        });

        return Ok(new
        {
            items = response,
            totalPrice = response.Sum(i => i.ProductPrice * i.Count)
        });
    }

    [HttpPatch("IncrementCount/{productId}")]
    public async Task<IActionResult> IncrementCount([FromRoute] int productId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        var cart = await _cartRepository.GetOneAsync(
            e => e.ApplicationUserId == user.Id && e.ProductId == productId
        );
        if (cart == null) return NotFound("Item not found");

        cart.Count++;                // عدلي count للكائن الأصلي
        await _cartRepository.EditAsync(cart); // استخدمي الكائن نفسه، متعمليش new

        return Ok(new { message = "Item incremented" });
    }

    [HttpPatch("DecrementCount/{productId}")]
    public async Task<IActionResult> DecrementCount([FromRoute] int productId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        var cart = await _cartRepository.GetOneAsync(
            e => e.ApplicationUserId == user.Id && e.ProductId == productId
        );
        if (cart == null) return NotFound("Item not found");

        if (cart.Count > 1)
        {
            cart.Count--;               // عدلي count للكائن الأصلي
            await _cartRepository.EditAsync(cart);
        }
        else
        {
            await _cartRepository.DeleteAsync(cart);
        }

        return Ok(new { message = "Item decremented/removed" });
    }


    [HttpDelete("DeleteItem/{productId}")]
    public async Task<IActionResult> DeleteItem(int productId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        var cart = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == productId);
        if (cart == null) return NotFound("Item not found");

        await _cartRepository.DeleteAsync(cart);

        return Ok(new { message = "Item deleted" });
    }
}
