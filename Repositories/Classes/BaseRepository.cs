using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SwiftDrop.Models.Entities;
using SwiftDrop.Repositories.Interfaces;

namespace SwiftDrop.Repositories.Classes
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        private IDbContextTransaction? _currentTransaction;
        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual void Create(T entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual async Task<T?> GetByIdAsync(int Id, bool asTracking = true)
        {
            try
            {
                if (asTracking)
                {
                    return await _dbSet.FindAsync(Id);
                }

                return await _dbSet.AsNoTracking()
                                   .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == Id);
            }
            catch
            {
                return null;
            }

        }


        public virtual async Task<IEnumerable<T>> GetAllAsync(bool asTracking = true)
        {
            try
            {
                if (asTracking)
                {
                    return await _dbSet.ToListAsync();
                }

                return await _dbSet.AsNoTracking().ToListAsync();

            }
            catch
            {
                return Enumerable.Empty<T>();
            }
        }

        public virtual async Task<bool> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch { return false; }
        }




        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null) return;
            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_currentTransaction != null)
                    await _currentTransaction.CommitAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_currentTransaction != null)
                    await _currentTransaction.RollbackAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public void Attach(T entity)
        {
            _context.Attach(entity);
        }
    }
}
