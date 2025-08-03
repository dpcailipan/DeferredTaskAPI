using DeferredTaskAPI.Entities;

namespace DeferredTaskAPI.Repositories.Interfaces
{
    public interface IScheduledTasksRepository: IRepository<ScheduledTask>
    {
        IEnumerable<ScheduledTask> GetAllExecutable();
    }
}
