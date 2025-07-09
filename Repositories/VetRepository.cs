using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager;
using Models;
using Repositories.IRepository;

namespace Repositories
{
    public class VetRepository : Repository<Vet>, IVetRepository
    {
    
        public VetRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task CommitAsync()
        {
            throw new NotImplementedException();
        }

        public bool Delete(Category category)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Vet vet)
        {
            throw new NotImplementedException();
        }

        public object Update(Category category)
        {
            throw new NotImplementedException();
        }

        public object Update(Vet vet)
        {
            throw new NotImplementedException();
        }
    }
}
