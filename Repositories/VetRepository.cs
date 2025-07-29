using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.IRepository;

namespace Repositories
{
    public class VetRepository : Repository<Vet>, IVetRepository
    {
        private readonly ApplicationDbContext context;

        public VetRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;

        }

        public async Task CommitAsync()
        {
            await context.SaveChangesAsync();
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
