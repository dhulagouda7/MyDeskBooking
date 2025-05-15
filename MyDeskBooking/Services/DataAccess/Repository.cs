using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq;
using MyDeskBooking.DataAccess;

namespace MyDeskBooking.Services.DataAccess
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public Task<T> GetByIdAsync(int id)
        {
            return Task.FromResult(_dbSet.Find(id));
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<T>>(_dbSet.ToList());
        }

        public Task<T> AddAsync(T entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
            return Task.FromResult(entity);
        }        
        public Task UpdateAsync(T entity)
        {
            try
            {
                // Attach the entity and mark it as modified
                var entry = _context.Entry(entity);
                if (entry.State == System.Data.EntityState.Detached)
                {
                    // If the entity is detached, attach it and mark it as modified
                    _dbSet.Attach(entity);
                    entry.State = System.Data.EntityState.Modified;
                }

                _context.SaveChanges();
                return Task.CompletedTask;
            }
            catch (System.InvalidOperationException)
            {
                // If there's an existing entity being tracked, detach all entities of type T
                foreach (var existingEntry in _context.ChangeTracker.Entries<T>())
                {
                    existingEntry.State = System.Data.EntityState.Detached;
                }

                // Try the update again
                var entry = _context.Entry(entity);
                _dbSet.Attach(entity);
                entry.State = System.Data.EntityState.Modified;
                _context.SaveChanges();
                return Task.CompletedTask;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
