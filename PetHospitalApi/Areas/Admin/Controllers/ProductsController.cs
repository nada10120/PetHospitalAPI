using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories.IRepository;
using Mapster;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IWebHostEnvironment environment)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productRepository.GetAsync();
                return Ok(products.Adapt<IEnumerable<ProductResponse>>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching products: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Errors = new[] { "Internal server error.", ex.Message } });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            try
            {
                var product = await _productRepository.GetOneAsync(e => e.ProductId == id);
                if (product == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    return NotFound(new { Errors = new[] { $"Product with ID {id} not found." } });
                }
                return Ok(product.Adapt<ProductResponse>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching product {id}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Errors = new[] { "Internal server error.", ex.Message } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductRequest productRequest)
        {
            try
            {
                if (productRequest == null)
                {
                    Console.WriteLine("ProductRequest is null.");
                    return BadRequest(new { Errors = new[] { "Product request data is null." } });
                }

                Console.WriteLine($"Received ProductRequest: Name={productRequest.Name ?? "null"}, CategoryId={productRequest.CategoryId}, Price={productRequest.Price}, StockQuantity={productRequest.StockQuantity}, Image={(productRequest.Image != null ? productRequest.Image.FileName : "null")}");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { Errors = errors });
                }

                var category = await _categoryRepository.GetOneAsync(c => c.CategoryId == productRequest.CategoryId);
                if (category == null)
                {
                    Console.WriteLine($"Category with ID {productRequest.CategoryId} not found.");
                    return BadRequest(new { Errors = new[] { $"Category with ID {productRequest.CategoryId} not found." } });
                }

                string imageUrl = null;
                if (productRequest.Image != null)
                {
                    if (!IsValidImage(productRequest.Image))
                    {
                        Console.WriteLine($"Invalid image file: {productRequest.Image.FileName}");
                        return BadRequest(new { Errors = new[] { "Invalid image file. Only JPG, PNG, and GIF are allowed." } });
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath ?? throw new InvalidOperationException("WebRootPath is null"), "images/products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                        Console.WriteLine($"Created uploads folder: {uploadsFolder}");
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productRequest.Image.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await productRequest.Image.CopyToAsync(fileStream);
                    }
                    Console.WriteLine($"Saved image to: {filePath}");
                    imageUrl = $"/images/products/{fileName}";
                }

                // Configure Mapster to ignore Category navigation property
                TypeAdapterConfig<ProductRequest, Product>
                    .NewConfig()
                    .Ignore(dest => dest.Category);

                var product = productRequest.Adapt<Product>();
                if (product == null)
                {
                    Console.WriteLine("Failed to map ProductRequest to Product.");
                    return StatusCode(500, new { Errors = new[] { "Internal server error.", "Product mapping failed." } });
                }

                product.CategoryId = category.CategoryId;
                product.Category = category;
                product.ImageUrl = imageUrl;
                product.Quantity = productRequest.StockQuantity;
                product.Traffic = 0;

                Console.WriteLine($"Product to create: Name={product.Name}, CategoryId={product.CategoryId}, Category={product.Category?.Name ?? "null"}, Price={product.Price}, StockQuantity={product.StockQuantity}, ImageUrl={product.ImageUrl ?? "null"}");

                var createdProduct = await _productRepository.CreateAsync(product);
                if (createdProduct == null)
                {
                    Console.WriteLine("CreateAsync returned null.");
                    return BadRequest(new { Errors = new[] { "Failed to create product in repository." } });
                }

                await _productRepository.CommitAsync();
                Console.WriteLine($"Created product: ProductId={createdProduct.ProductId}");
                return Created($"{Request.Scheme}://{Request.Host}/api/Admin/Products/{createdProduct.ProductId}", createdProduct.Adapt<ProductResponse>());
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error creating product: {dbEx.Message}\nInnerException: {dbEx.InnerException?.Message}\n{dbEx.StackTrace}");
                return StatusCode(500, new { Errors = new[] { "Database error creating product.", dbEx.InnerException?.Message ?? dbEx.Message } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Errors = new[] { "Internal server error.", ex.Message } });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] ProductRequest productRequest)
        {
            try
            {
                if (productRequest == null)
                {
                    Console.WriteLine("ProductRequest is null.");
                    return BadRequest(new { Errors = new[] { "Product request data is null." } });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { Errors = errors });
                }

                var existingProduct = await _productRepository.GetOneAsync(e => e.ProductId == id);
                if (existingProduct == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    return NotFound(new { Errors = new[] { $"Product with ID {id} not found." } });
                }

                var category = await _categoryRepository.GetOneAsync(c => c.CategoryId == productRequest.CategoryId);
                if (category == null)
                {
                    Console.WriteLine($"Category with ID {productRequest.CategoryId} not found.");
                    return BadRequest(new { Errors = new[] { $"Category with ID {productRequest.CategoryId} not found." } });
                }

                string imageUrl = existingProduct.ImageUrl;
                if (productRequest.Image != null)
                {
                    if (!IsValidImage(productRequest.Image))
                    {
                        Console.WriteLine($"Invalid image file: {productRequest.Image.FileName}");
                        return BadRequest(new { Errors = new[] { "Invalid image file. Only JPG, PNG, and GIF are allowed." } });
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath ?? throw new InvalidOperationException("WebRootPath is null"), "images/products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productRequest.Image.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await productRequest.Image.CopyToAsync(fileStream);
                    }

                    imageUrl = $"/images/products/{fileName}";

                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existingProduct.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                            Console.WriteLine($"Deleted old image: {oldFilePath}");
                        }
                    }
                }

                // Configure Mapster to ignore Category navigation property
                TypeAdapterConfig<ProductRequest, Product>
                    .NewConfig()
                    .Ignore(dest => dest.Category);

                productRequest.Adapt(existingProduct);
                existingProduct.CategoryId = category.CategoryId;
                existingProduct.Category = category;
                existingProduct.ImageUrl = imageUrl;
                existingProduct.Quantity = productRequest.StockQuantity;
                existingProduct.Traffic = existingProduct.Traffic; // Preserve existing Traffic

                var updatedProduct = await _productRepository.EditAsync(existingProduct);
                if (updatedProduct == null)
                {
                    Console.WriteLine("UpdateAsync returned null.");
                    return BadRequest(new { Errors = new[] { "Failed to update product in repository." } });
                }

                await _productRepository.CommitAsync();
                Console.WriteLine($"Updated product: ProductId={updatedProduct.ProductId}");
                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error updating product {id}: {dbEx.Message}\nInnerException: {dbEx.InnerException?.Message}\n{dbEx.StackTrace}");
                return StatusCode(500, new { Errors = new[] { "Database error updating product.", dbEx.InnerException?.Message ?? dbEx.Message } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product {id}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Errors = new[] { "Internal server error.", ex.Message } });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var product = await _productRepository.GetOneAsync(e => e.ProductId == id);
                if (product == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    return NotFound(new { Errors = new[] { $"Product with ID {id} not found." } });
                }

                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        Console.WriteLine($"Deleted image: {filePath}");
                    }
                }

                var result = await _productRepository.DeleteAsync(product);
                //if (!result)
                //{
                //    Console.WriteLine("DeleteAsync returned false.");
                //    return BadRequest(new { Errors = new[] { "Failed to delete product in repository." } });
                //}

                Console.WriteLine($"Deleted product: ProductId={id}");
                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error deleting product {id}: {dbEx.Message}\nInnerException: {dbEx.InnerException?.Message}\n{dbEx.StackTrace}");
                return StatusCode(500, new { Errors = new[] { "Database error deleting product.", dbEx.InnerException?.Message ?? dbEx.Message } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product {id}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Errors = new[] { "Internal server error.", ex.Message } });
            }
        }

        private bool IsValidImage(IFormFile file)
        {
            if (file == null) return false;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }
    }
}