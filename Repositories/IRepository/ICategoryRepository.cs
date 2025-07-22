using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Repositories.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task CommitAsync();
        bool Delete(Category category);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        object Update(Category category);
    }
}
