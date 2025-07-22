using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Repositories.IRepository
{
    public interface IOrderItemRepository : IRepository<Order>
    {
        Task CreateRangeAsync(IEnumerable<OrderItem> orderItems);

    }
}
