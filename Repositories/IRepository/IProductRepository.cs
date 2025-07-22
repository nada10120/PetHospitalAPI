using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager;
using Models;
using Models.DTOs.Request;

namespace Repositories.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        Task CommitAsync();
        bool Delete(Product product);
        object Update(Product product);
        Task<IEnumerable<Product>> FilterAsync(FilterItemsRequest filterItems, int page, int pageSize);
        Task<int> CountAsync(FilterItemsRequest filterItems);
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetRelatedProductsAsync(Product product);
        Task<IEnumerable<Product>> GetTopProductsAsync(int currentProductId);
        Task<IEnumerable<Product>> GetSameCategoryProductsAsync(Product product);
        Task UpdateTrafficAsync(Product product);
    }
}
