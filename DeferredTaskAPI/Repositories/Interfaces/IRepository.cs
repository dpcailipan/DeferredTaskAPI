using DeferredTaskAPI.Entities;

namespace DeferredTaskAPI.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task CreateAsync(T entity);
        IEnumerable<ScheduledTask> GetAll(bool trackChanges = true);
        ScheduledTask? Get(Guid id, bool trackChanges = true);
        void Update(ScheduledTask scheduledTask);
        void Delete(ScheduledTask scheduledTask);
        Task<int> SaveChangesAsync();
    }
}
