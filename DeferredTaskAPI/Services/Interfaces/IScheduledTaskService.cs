using DeferredTaskAPI.Entities;
using DeferredTaskAPI.Models;

namespace DeferredTaskAPI.Services.Interfaces
{
    public interface IScheduledTaskService
    {
        Task<ApiResult<ScheduledTask>> CreateScheduledTaskAsync(ScheduledTaskRequest request);
        ApiResult<IEnumerable<ScheduledTask>> GetScheduledTasks();
        ApiResult<ScheduledTask> GetScheduledTask(Guid id);
        Task<ApiResult<ScheduledTask>> UpdateScheduledTaskAsync(
            Guid id,
            ScheduledTaskRequest request);
        Task<ApiResult> DeleteScheduledTaskAsync(Guid id);
        Task RunScheduledTasksAsync(CancellationToken cancellationToken);
    }
}