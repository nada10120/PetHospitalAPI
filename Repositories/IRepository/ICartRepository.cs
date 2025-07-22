using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Repositories.IRepository
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task CommitAsync();
        Task<bool> DeleteRangeAsync(IEnumerable<Cart> entities);
    }
}
