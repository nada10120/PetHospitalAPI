using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTOs.Request;
using Repositories.IRepository;

namespace Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<Product>> FilterAsync(FilterItemsRequest filterItems, int page, int pageSize)
        {
            var query = context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(filterItems.Search))
                query = query.Where(p => p.Name.Contains(filterItems.Search));

            if (filterItems.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filterItems.CategoryId.Value);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync(FilterItemsRequest filterItems)
        {
            var query = context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(filterItems.Search))
                query = query.Where(p => p.Name.Contains(filterItems.Search));

            if (filterItems.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filterItems.CategoryId.Value);

            return await query.CountAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(Product product)
        {
            return await context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                .Take(4)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetTopProductsAsync(int currentProductId)
        {
            return await context.Products
                .Where(p => p.ProductId != currentProductId)
                .OrderByDescending(p => p.Traffic)
                .Take(5)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetSameCategoryProductsAsync(Product product)
        {
            return await context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                .Take(6)
                .ToListAsync();
        }

        public async Task UpdateTrafficAsync(Product product)
        {
            context.Products.Update(product);
            await context.SaveChangesAsync();
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
