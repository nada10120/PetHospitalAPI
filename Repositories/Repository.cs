using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DataManager;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository;

namespace Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbSet<T> _dbSet;
        private readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Create
        public async Task<T?> CreateAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return null;
            }
        }

        // Update
        public async Task<T?> EditAsync(T entity)
        {
            try
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return null;
            }
        }

        // Delete
        public async Task<T?> DeleteAsync(T entity)
        {
            try
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return null;
            }
        }

        // Get multiple records
        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            Func<IQueryable<T>, IQueryable<T>>? thenIncludes = null,
            bool tracked = true)
        {
            IQueryable<T> query = _dbSet;

            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            if (expression != null)
            {
                query = query.Where(expression);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (thenIncludes != null)
            {
                query = thenIncludes(query); // Apply .ThenInclude() logic
            }

            return await query.ToListAsync();
        }

        // Get a single record (async version)
        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            Func<IQueryable<T>, IQueryable<T>>? thenIncludes = null,
            bool tracked = true)
        {
            var result = await GetAsync(expression, includes, thenIncludes, tracked);
            return result.FirstOrDefault();
        }

        // Optional: Remove blocking GetOne if not needed
        public T? GetOne(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            Func<IQueryable<T>, IQueryable<T>>? thenIncludes = null,
            bool tracked = true)
        {
            return GetOneAsync(expression, includes, thenIncludes, tracked).GetAwaiter().GetResult();
        }
    }
}
