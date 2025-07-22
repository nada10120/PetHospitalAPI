using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DataManager;
using Models;
using Repositories.IRepository;

namespace Repositories
{
    public class OrderItemRepository : Repository<Order> , IOrderRepository
    {
        private readonly ApplicationDbContext context;
        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public Task CommitAsync()
        {
            throw new NotImplementedException();
        }

        public bool Delete(Order Order)
        {
            throw new NotImplementedException();
        }

        public Order GetOne(Expression<Func<Order, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public object Update(Order Order)
        {
            throw new NotImplementedException();
        }
    }
}
