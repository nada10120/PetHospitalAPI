using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager;
using Models;

namespace Repositories.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        Task CommitAsync();
        bool Delete(Product product);
        object Update(Product product);
    }
}
