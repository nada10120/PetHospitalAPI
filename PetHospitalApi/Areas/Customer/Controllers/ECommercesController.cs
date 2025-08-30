using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Request;
using Repositories.IRepository;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Models.DTOs.Response;
using Repositories;
using ServiceStack;
using Mapster;
using Models;

namespace PetHospitalApi.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ECommercesController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<ECommercesController> _logger;
        private readonly IAppointmentRepository _appointmentRepository;

        public ECommercesController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ILogger<ECommercesController> logger,
            IAppointmentRepository appointmentRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appointmentRepository = appointmentRepository;
        }

        [HttpGet("index")]
        public async Task<ActionResult> GetProducts([FromQuery] FilterItemsRequest filterItemsVM, int page = 1)
        {
            try
            {
                if (page < 1)
                {
                    _logger.LogWarning("Invalid page number: {Page}", page);
                    return BadRequest(new { Errors = new[] { "Page number must be greater than 0." } });
                }

                const int pageSize = 8;
                _logger.LogInformation("Fetching products with filter: Search={Search}, ProductName={ProductName}, MinPrice={MinPrice}, MaxPrice={MaxPrice}, CategoryId={CategoryId}, Page={Page}",
                    filterItemsVM.Search ?? "null",
                    filterItemsVM.ProductName ?? "null",
                    filterItemsVM.MinPrice??0,
                    filterItemsVM.MaxPrice??3000,
                    filterItemsVM.CategoryId,
                    page);

                var categories = await _categoryRepository.GetAllAsync();
                if (categories == null)
                {
                    _logger.LogError("Categories returned null from GetAllAsync");
                    return StatusCode(205, new { Errors = new[] { "Failed to retrieve categories." } });
                }

                var filteredProducts = await _productRepository.FilterAsync(filterItemsVM, page, pageSize);
                if (filteredProducts == null)
                {
                    _logger.LogError("Filtered products returned null from FilterAsync");
                    return StatusCode(206, new { Errors = new[] { "Failed to retrieve products." } });
                }

                var totalItems = await _productRepository.CountAsync(filterItemsVM);
                var totalPageNumber = Math.Ceiling(totalItems / (double)pageSize);

                var response = new
                {
                    Products = filteredProducts,
                    Categories = categories,
                    FilterItemsVM = filterItemsVM,
                    TotalPageNumber = totalPageNumber
                };

                _logger.LogInformation("Returning {Count} products", filteredProducts.Count());
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetProducts request: {Message}", ex.Message);
                return StatusCode(500, new { Errors = new[] { "An error occurred while fetching products.", ex.Message } });
            }
        }

    
        [HttpGet("{id}")]
        public async Task<ActionResult> GetProductDetails([FromRoute]int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid product ID: {Id}", id);
                    return BadRequest(new { Errors = new[] { "Invalid product ID." } });
                }
                _logger.LogInformation("Fetching details for product ID: {Id}", id);
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product not found with ID: {Id}", id);
                    return NotFound(new { Errors = new[] { "Product not found." } });
                }
                await _productRepository.UpdateTrafficAsync(product);
                var relatedProducts = await _productRepository.GetRelatedProductsAsync(product);
                var sameCategoryProducts = await _productRepository.GetSameCategoryProductsAsync(product);
                var topProducts = await _productRepository.GetTopProductsAsync(product.ProductId);
                var response = new
                {
                    Product = product,
                    RelatedProducts = relatedProducts,
                    SameCategoryProducts = sameCategoryProducts,
                    TopProducts = topProducts
                };
                _logger.LogInformation("Returning details for product ID: {Id}", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetProductDetails request for ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { Errors = new[] { "An error occurred while fetching product details.", ex.Message } });
            }
        }



        // Existing methods here...

        /// <summary>
        /// Book a new appointment
        /// </summary>
        [HttpPost("appointments")]
        public async Task<ActionResult<AppointmentResponse>> CreateAppointment([FromBody] AppointmentRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.UserId) || request.PetId <= 0 || string.IsNullOrEmpty(request.VetId))
                {
                    return BadRequest(new { Errors = new[] { "Invalid appointment data." } });
                }

                // Default status if not provided
                request.Status ??= "Pending";
                request.DateTime ??= DateTime.UtcNow.AddDays(1);

                var appointment = await _appointmentRepository.CreateAsync(request.Adapt<Appointment>());

                _logger.LogInformation("Appointment created for User {UserId} with Vet {VetId} at {DateTime}", request.UserId, request.VetId, request.DateTime);

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment for user {UserId}: {Message}", request.UserId, ex.Message);
                return StatusCode(500, new { Errors = new[] { "An error occurred while creating the appointment.", ex.Message } });
            }
        }

        /// <summary>
        /// Get all appointments for a user
        /// </summary>
        [HttpGet("appointments/{userId}")]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAppointments(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { Errors = new[] { "UserId is required." } });
                }

                var appointments = await _appointmentRepository.GetOneAsync(e=>e.UserId==userId);

                _logger.LogInformation("Retrieved {Count} appointments for User {UserId}", appointments?.AppointmentId.InList().Count() ?? 0, userId);

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointments for user {UserId}: {Message}", userId, ex.Message);
                return StatusCode(500, new { Errors = new[] { "An error occurred while fetching appointments.", ex.Message } });
            }
        }
    }
}

   