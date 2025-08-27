using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Request;
using Repositories.IRepository;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

        public ECommercesController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ILogger<ECommercesController> logger)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                    filterItemsVM.MinPrice,
                    filterItemsVM.MaxPrice,
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



    }
}