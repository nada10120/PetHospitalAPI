using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DataManager;
using Repositories.IRepository;

namespace Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger) : base(context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Product>> FilterAsync(FilterItemsRequest filterItems, int page, int pageSize)
        {
            try
            {
                if (context.Products == null)
                {
                    _logger.LogError("Products DbSet is null");
                    return new List<Product>();
                }

                var query = context.Products.AsQueryable();

                if (!string.IsNullOrEmpty(filterItems.Search))
                {
                    query = query.Where(p => p.Name.Contains(filterItems.Search, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(filterItems.ProductName))
                {
                    query = query.Where(p => p.Name.Contains(filterItems.ProductName, StringComparison.OrdinalIgnoreCase));
                }

                if (filterItems.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= filterItems.MinPrice.Value);
                }

                if (filterItems.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= filterItems.MaxPrice.Value);
                }

                if (filterItems.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == filterItems.CategoryId.Value);
                }

                var products = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} products for filter: {Filter}", products.Count, System.Text.Json.JsonSerializer.Serialize(filterItems));
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FilterAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<int> CountAsync(FilterItemsRequest filterItems)
        {
            try
            {
                if (context.Products == null)
                {
                    _logger.LogError("Products DbSet is null");
                    return 0;
                }

                var query = context.Products.AsQueryable();

                if (!string.IsNullOrEmpty(filterItems.Search))
                {
                    query = query.Where(p => p.Name.Contains(filterItems.Search, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(filterItems.ProductName))
                {
                    query = query.Where(p => p.Name.Contains(filterItems.ProductName, StringComparison.OrdinalIgnoreCase));
                }

                if (filterItems.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= filterItems.MinPrice.Value);
                }

                if (filterItems.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= filterItems.MaxPrice.Value);
                }

                if (filterItems.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == filterItems.CategoryId.Value);
                }

                var count = await query.CountAsync();
                _logger.LogInformation("Counted {Count} products for filter: {Filter}", count, System.Text.Json.JsonSerializer.Serialize(filterItems));
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CountAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            try
            {
                if (context.Products == null)
                {
                    _logger.LogError("Products DbSet is null");
                    return null;
                }

                var product = await context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                _logger.LogInformation("Retrieved product with ID {Id}: {Product}", id, product != null ? System.Text.Json.JsonSerializer.Serialize(product) : "null");
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAsync for ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(Product product)
        {
            try
            {
                if (context.Products == null)
                {
                    _logger.LogError("Products DbSet is null");
                    return new List<Product>();
                }

                var products = await context.Products
                    .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                    .Take(4)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} related products for product ID {Id}", products.Count, product.ProductId);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRelatedProductsAsync for product ID {Id}: {Message}", product.ProductId, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetTopProductsAsync(int currentProductId)
        {
            try
            {
                if (context.Products == null)
                {
                    _logger.LogError("Products DbSet is null");
                    return new List<Product>();
                }

                var products = await context.Products
                    .Where(p => p.ProductId != currentProductId)
                    .OrderByDescending(p => p.Traffic)
                    .Take(5)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} top products excluding product ID {Id}", products.Count, currentProductId);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTopProductsAsync for product ID {Id}: {Message}", currentProductId, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetSameCategoryProductsAsync(Product product)
        {
            try
            {
                if (context.Products == null)
                {
                    _logger.LogError("Products DbSet is null");
                    return new List<Product>();
                }

                var products = await context.Products
                    .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                    .Take(6)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} same category products for product ID {Id}", products.Count, product.ProductId);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSameCategoryProductsAsync for product ID {Id}: {Message}", product.ProductId, ex.Message);
                throw;
            }
        }

        public async Task UpdateTrafficAsync(Product product)
        {
            try
            {
                if (context.Products == null)
                {
                    _logger.LogError("Products DbSet is null");
                    throw new InvalidOperationException("Products DbSet is null");
                }

                context.Products.Update(product);
                await context.SaveChangesAsync();
                _logger.LogInformation("Updated traffic for product ID {Id}", product.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateTrafficAsync for product ID {Id}: {Message}", product.ProductId, ex.Message);
                throw;
            }
        }

        public Task CommitAsync()
        {
            throw new NotImplementedException();
        }

        public bool Delete(Product product)
        {
            throw new NotImplementedException();
        }

        public object Update(Product product)
        {
            throw new NotImplementedException();
        }
    }
}