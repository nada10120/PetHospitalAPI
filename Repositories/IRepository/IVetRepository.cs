using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Repositories.IRepository
{
    public interface IVetRepository : IRepository<Vet>
    {
        Task CommitAsync();
        bool Delete(Category category);
        bool Delete(Vet vet);
        object Update(Category category);
        object Update(Vet vet);
    }
}
