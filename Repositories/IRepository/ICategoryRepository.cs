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
        object Update(Category category);
    }
}
