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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

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

            // Configure Mapster for traffic mapping
            TypeAdapterConfig<ProductRequest, Product>
                .NewConfig()
                .Map(dest => dest.Traffic, src => src.traffic);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productRepository.GetAsync();
                var categories = await _categoryRepository.GetAsync();
                var productsResponse = products.Adapt<IEnumerable<ProductResponse>>();

                foreach (var product in productsResponse)
                {
                    var dbProduct = products.FirstOrDefault(p => p.ProductId == product.ProductId);
                    if (dbProduct != null)
                    {
                        product.ImageUrl = dbProduct.ImageUrl ?? "/images/default.jpg";
                        product.Category = dbProduct.CategoryId.HasValue
                            ? categories.FirstOrDefault(c => c.CategoryId == dbProduct.CategoryId.Value)?.Name ?? "Unknown"
                            : "Unknown";
                    }
                }
                return Ok(productsResponse);
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
                var category = product.CategoryId.HasValue
                    ? await _categoryRepository.GetOneAsync(e => e.CategoryId == product.CategoryId.Value)
                    : null;
                var productResponse = product.Adapt<ProductResponse>();
                productResponse.Category = category?.Name ?? "Unknown";
                productResponse.ImageUrl = product.ImageUrl ?? "/images/default.jpg";
                return Ok(productResponse);
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
                Console.WriteLine($"Received POST request with ProductRequest: {JsonSerializer.Serialize(productRequest)}");

                // Check ModelState for binding and validation errors
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    Console.WriteLine($"Model validation failed: {JsonSerializer.Serialize(errors)}");
                    return BadRequest(new
                    {
                        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        Title = "One or more validation errors occurred.",
                        Status = 400,
                        Errors = errors
                    });
                }

                // Explicit null check for ProductRequest
                if (productRequest == null)
                {
                    Console.WriteLine("ProductRequest is null.");
                    return BadRequest(new
                    {
                        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        Title = "One or more validation errors occurred.",
                        Status = 400,
                        Errors = new Dictionary<string, string[]> { { "ProductRequest", new[] { "Product data is required." } } }
                    });
                }

                // Validate ImageFile
                if (productRequest.ImageFile != null && !IsValidImage(productRequest.ImageFile))
                {
                    Console.WriteLine($"Invalid image file: {productRequest.ImageFile.FileName}");
                    return BadRequest(new
                    {
                        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        Title = "One or more validation errors occurred.",
                        Status = 400,
                        Errors = new Dictionary<string, string[]> { { "ImageFile", new[] { "Invalid image file format. Allowed formats: .jpg, .jpeg, .png, .gif." } } }
                    });
                }

                // Validate CategoryId
                if (productRequest.CategoryId.HasValue)
                {
                    var category = await _categoryRepository.GetByIdAsync(productRequest.CategoryId.Value);
                    if (category == null)
                    {
                        Console.WriteLine($"Category with ID {productRequest.CategoryId} not found.");
                        return BadRequest(new
                        {
                            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                            Title = "One or more validation errors occurred.",
                            Status = 400,
                            Errors = new Dictionary<string, string[]> { { "CategoryId", new[] { $"Category with ID {productRequest.CategoryId} not found." } } }
                        });
                    }
                    productRequest.CategoryId = category.CategoryId;
                }

                // Map ProductRequest to Product
                var product = productRequest.Adapt<Product>();

                // Handle ImageFile upload
                if (productRequest.ImageFile != null)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(productRequest.ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await productRequest.ImageFile.CopyToAsync(stream);
                        }
                        product.ImageUrl = $"/images/{fileName}";
                        Console.WriteLine($"Image uploaded: {product.ImageUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading image: {ex.Message}\n{ex.StackTrace}");
                        return BadRequest(new
                        {
                            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                            Title = "One or more validation errors occurred.",
                            Status = 400,
                            Errors = new Dictionary<string, string[]> { { "ImageFile", new[] { $"Failed to upload image: {ex.Message}" } } }
                        });
                    }
                }
                else
                {
                    product.ImageUrl = "/images/default.jpg";
                }

                // Create product in repository
                var createdProduct = await _productRepository.CreateAsync(product);
                if (createdProduct == null)
                {
                    Console.WriteLine("CreateAsync returned null.");
                    return StatusCode(500, new
                    {
                        Errors = new[] { "Failed to create product in repository." }
                    });
                }

                var productResponse = createdProduct.Adapt<ProductResponse>();
                productResponse.Category = productRequest.CategoryId.HasValue
                    ? (await _categoryRepository.GetByIdAsync(productRequest.CategoryId.Value))?.Name ?? "Unknown"
                    : "Unknown";
                Console.WriteLine($"Created product: ProductId={createdProduct.ProductId}");
                return CreatedAtAction(nameof(GetOne), new { id = createdProduct.ProductId }, productResponse);
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
        public async Task<IActionResult> Update([FromRoute] int id, [FromForm] ProductRequest productRequest)
        {
            try
            {
                Console.WriteLine($"Received PUT request for ProductId {id}: {JsonSerializer.Serialize(productRequest)}");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    Console.WriteLine($"Model validation failed: {JsonSerializer.Serialize(errors)}");
                    return BadRequest(new
                    {
                        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        Title = "One or more validation errors occurred.",
                        Status = 400,
                        Errors = errors
                    });
                }

                if (productRequest == null)
                {
                    Console.WriteLine("ProductRequest is null.");
                    return BadRequest(new
                    {
                        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        Title = "One or more validation errors occurred.",
                        Status = 400,
                        Errors = new Dictionary<string, string[]> { { "ProductRequest", new[] { "Product data is required." } } }
                    });
                }

                if (productRequest.ImageFile != null && !IsValidImage(productRequest.ImageFile))
                {
                    Console.WriteLine($"Invalid image file: {productRequest.ImageFile.FileName}");
                    return BadRequest(new
                    {
                        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        Title = "One or more validation errors occurred.",
                        Status = 400,
                        Errors = new Dictionary<string, string[]> { { "ImageFile", new[] { "Invalid image file format. Allowed formats: .jpg, .jpeg, .png, .gif." } } }
                    });
                }

                var product = await _productRepository.GetOneAsync(e => e.ProductId == id);
                if (product == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    return NotFound(new { Errors = new[] { $"Product with ID {id} not found." } });
                }

                var category = productRequest.CategoryId.HasValue
                    ? await _categoryRepository.GetByIdAsync(productRequest.CategoryId.Value)
                    : null;
                if (productRequest.CategoryId.HasValue && category == null)
                {
                    Console.WriteLine($"Category with ID {productRequest.CategoryId} not found.");
                    return BadRequest(new
                    {
                        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        Title = "One or more validation errors occurred.",
                        Status = 400,
                        Errors = new Dictionary<string, string[]> { { "CategoryId", new[] { $"Category with ID {productRequest.CategoryId} not found." } } }
                    });
                }

                productRequest.Adapt(product);
                product.CategoryId = category?.CategoryId;

                if (productRequest.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "/images/default.jpg")
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                            Console.WriteLine($"Deleted old image: {oldFilePath}");
                        }
                    }
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(productRequest.ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await productRequest.ImageFile.CopyToAsync(stream);
                        }
                        product.ImageUrl = $"/images/{fileName}";
                        Console.WriteLine($"Image uploaded: {product.ImageUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading image: {ex.Message}\n{ex.StackTrace}");
                        return BadRequest(new
                        {
                            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                            Title = "One or more validation errors occurred.",
                            Status = 400,
                            Errors = new Dictionary<string, string[]> { { "ImageFile", new[] { $"Failed to upload image: {ex.Message}" } } }
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(productRequest.ExistingImageUrl))
                {
                    product.ImageUrl = productRequest.ExistingImageUrl;
                }

                var updatedProduct = await _productRepository.EditAsync(product);
                if (updatedProduct == null)
                {
                    Console.WriteLine("UpdateAsync returned null.");
                    return StatusCode(500, new
                    {
                        Errors = new[] { "Failed to update product in repository." }
                    });
                }

                var productResponse = updatedProduct.Adapt<ProductResponse>();
                productResponse.Category = category?.Name ?? "Unknown";
                Console.WriteLine($"Updated product: ProductId={updatedProduct.ProductId}");
                return Ok(productResponse);
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
                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "/images/default.jpg")
                {
                    var filePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        Console.WriteLine($"Deleted image: {filePath}");
                    }
                }
                await _productRepository.DeleteAsync(product);
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
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("Image file is null or empty.");
                return false;
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            bool isValid = allowedExtensions.Contains(extension);
            Console.WriteLine($"Image validation: File={file.FileName}, Extension={extension}, IsValid={isValid}");
            return isValid;
        }
    }
}