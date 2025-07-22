using DataManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories.IRepository;

namespace PetHospitalApi.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ECommercesController : ControllerBase
    {
        private readonly ILogger<ECommercesController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ECommercesController(
            ILogger<ECommercesController> logger,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        [HttpGet("index")]
        public async Task<ActionResult<ProductWithFilterRequest>> GetProducts([FromQuery] FilterItemsRequest filterItemsVM, int page = 1)
        {
            const int pageSize = 8;

            var categories = await _categoryRepository.GetAllAsync();

            var filteredProducts = await _productRepository.FilterAsync(filterItemsVM, page, pageSize);
            var totalItems = await _productRepository.CountAsync(filterItemsVM);
            var totalPageNumber = Math.Ceiling(totalItems / (double)pageSize);

            var ProductWithFilterResponse = new
            {
                Products = filteredProducts,
                Categories = categories,
                FilterItemsVM = filterItemsVM,
                TotalPageNumber = totalPageNumber
            };

            return Ok(ProductWithFilterResponse);
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<ProductWithRelatedResponse>> GetProductDetails(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                return NotFound("Product not found");

            var relatedProducts = await _productRepository.GetRelatedProductsAsync(product);
            var topProducts = await _productRepository.GetTopProductsAsync(product.ProductId);
            var sameCategoryProducts = await _productRepository.GetSameCategoryProductsAsync(product);

            ProductWithRelatedResponse productWithRelated = new()
            {
                Product = product,
                RelatedProducts = (List<Product>)relatedProducts,
                TopProducts = (List<Product>)topProducts,
                SameCategoryProducts = (List<Product>)sameCategoryProducts
            };

            product.Traffic++;
            await _productRepository.UpdateTrafficAsync(product);

            return Ok(productWithRelated);
        }
    }
}