namespace SwiftDrop.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        public void Create(T entity);
        public void Update(T entity);
        public void Delete(T entity);
        public Task<T?> GetByIdAsync(int Id, bool asTracking = true);
        public Task<IEnumerable<T>> GetAllAsync(bool asTracking = true);


        public void Attach(T entity);
        public Task BeginTransactionAsync();
        public Task RollbackTransactionAsync();
        public Task CommitTransactionAsync();
        public Task<bool> SaveChangesAsync();

    }
}
