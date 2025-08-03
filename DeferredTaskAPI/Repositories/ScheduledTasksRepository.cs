using DeferredTaskAPI.Data;
using DeferredTaskAPI.Entities;
using DeferredTaskAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeferredTaskAPI.Repositories
{
    public class ScheduledTasksRepository : IScheduledTasksRepository
    {
        private readonly ILogger<ScheduledTasksRepository> _logger;
        private readonly DeferredTaskDbContext _dbContext;
        private readonly DbSet<ScheduledTask> _dbSet;

        public ScheduledTasksRepository(ILogger<ScheduledTasksRepository> logger,
            DeferredTaskDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _dbSet = dbContext.Set<ScheduledTask>();
        }

        public async Task CreateAsync(ScheduledTask entity)
        {
            _ = await _dbSet.AddAsync(entity);
        }

        public IEnumerable<ScheduledTask> GetAll(bool trackChanges = true)
        {
            var query = _dbSet.AsQueryable();

            if (!trackChanges)
            {
                _ = query.AsNoTracking();
            }

            return query;
        }

        public IEnumerable<ScheduledTask> GetAllExecutable()
        {
            return GetAll()
                .Where(st => st.ScheduledTime < DateTime.UtcNow
                    && !st.IsExecuted);
        }

        public ScheduledTask? Get(Guid id, bool trackChanges = true)
        {
            var query = GetAll(trackChanges);

            return query.FirstOrDefault(st => st.Id == id);
        }

        public void Update(ScheduledTask scheduledTask)
        {
            _dbContext.Entry(scheduledTask).State = EntityState.Modified;
        }

        public void Delete(ScheduledTask scheduledTask)
        {
            _ = _dbSet.Remove(scheduledTask);
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error was encountered while saving changes.");
                return -1;
            }
        }
    }
}
