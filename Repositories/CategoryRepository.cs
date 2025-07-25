﻿using System;
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
    public class CategoryRepository : Repository<Category> , ICategoryRepository
    {
        private readonly ApplicationDbContext context;
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public Task CommitAsync()
        {
            throw new NotImplementedException();
        }

        public bool Delete(Category category)
        {
            throw new NotImplementedException();
        }

    

        public object Update(Category category)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await context.Categories.ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        }
    }
}
