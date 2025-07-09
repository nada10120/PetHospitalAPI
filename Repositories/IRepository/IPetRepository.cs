using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
namespace Repositories.IRepository
{
    public interface IPetRepository : IRepository<Pet>
    {
        Task CommitAsync();
        bool Delete(Pet pet);
        object Update(Pet pet);
    }
}
