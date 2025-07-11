﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager;
using Models;
using Repositories.IRepository;

namespace Repositories
{
    public class PetRepository : Repository<Pet>, IPetRepository
    {
        private readonly ApplicationDbContext context;
        public PetRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public Task CommitAsync()
        {
            throw new NotImplementedException();
        }

        public bool Delete(Pet pet)
        {
            throw new NotImplementedException();
        }

        public object Update(Pet pet)
        {
            throw new NotImplementedException();
        }
    }
}
