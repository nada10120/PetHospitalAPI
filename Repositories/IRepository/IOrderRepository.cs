using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Repositories.IRepository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task CommitAsync();
        bool Delete(Order Order);
        object Update(Order Order);
    }
}
