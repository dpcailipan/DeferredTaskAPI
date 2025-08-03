using DeferredTaskAPI.Entities;
using DeferredTaskAPI.Models;
using DeferredTaskAPI.Repositories.Interfaces;
using DeferredTaskAPI.Services.Interfaces;
using System.Net;

namespace DeferredTaskAPI.Services
{
    public class ScheduledTaskService : IScheduledTaskService
    {
        private readonly ILogger<ScheduledTaskService> _logger;
        private readonly IScheduledTasksRepository _scheduledTasksRepository;

        public ScheduledTaskService(ILogger<ScheduledTaskService> logger,
            IScheduledTasksRepository scheduledTasksRepository)
        {
            _logger = logger;
            _scheduledTasksRepository = scheduledTasksRepository;
        }

        public async Task<ApiResult<ScheduledTask>> CreateScheduledTaskAsync(
            ScheduledTaskRequest request)
        {
            var errors = ValidateRequest(request);

            if (errors.Count > 0)
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsFailed = true,
                    Errors = errors,
                };
            }

            var createdAt = DateTime.UtcNow;
            var scheduledTask = new ScheduledTask
            {
                Title = request.Title,
                Description = request.Description,
                ScheduledTime = request.ScheduledTime,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            };

            await _scheduledTasksRepository.CreateAsync(scheduledTask);

            var changeCount = await _scheduledTasksRepository.SaveChangesAsync();
            if (changeCount > 0)
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.Created,
                    IsSuccess = true,
                    Value = scheduledTask
                };
            }
            else
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsFailed = true,
                    Errors = new List<string>
                    { "An unexpected result was encountered while saving the changes." }
                };
            }
        }

        public ApiResult<IEnumerable<ScheduledTask>> GetScheduledTasks()
        {
            var scheduledTasks = _scheduledTasksRepository.GetAll(trackChanges: false);

            return new ApiResult<IEnumerable<ScheduledTask>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Value = scheduledTasks
            };
        }

        public ApiResult<ScheduledTask> GetScheduledTask(Guid id)
        {
            var scheduledTask = _scheduledTasksRepository.Get(id, trackChanges: false);

            if (scheduledTask == null)
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsFailed = true,
                    Errors = new List<string> { "Scheduled task not found." }
                };
            }

            return new ApiResult<ScheduledTask>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Value = scheduledTask
            };
        }

        public async Task<ApiResult<ScheduledTask>> UpdateScheduledTaskAsync(
            Guid id,
            ScheduledTaskRequest request)
        {
            var scheduledTask = _scheduledTasksRepository.Get(id);

            if (scheduledTask == null)
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsFailed = true,
                    Errors = new List<string> { "Scheduled task not found." }
                };
            }

            if (scheduledTask.IsExecuted)
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsFailed = true,
                    Errors = new List<string> { "Scheduled task already executed." }
                };
            }

            var errors = ValidateRequest(request);

            if (errors.Count > 0)
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsFailed = true,
                    Errors = errors,
                };
            }

            scheduledTask.Title = request.Title;
            scheduledTask.Description = request.Description;
            scheduledTask.ScheduledTime = request.ScheduledTime;
            scheduledTask.UpdatedAt = DateTime.UtcNow;

            _scheduledTasksRepository.Update(scheduledTask);

            var changeCount = await _scheduledTasksRepository.SaveChangesAsync();
            if (changeCount > 0)
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Value = scheduledTask
                };
            }
            else
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsFailed = true,
                    Errors = new List<string>
                    { "An unexpected result was encountered while saving the changes." }
                };
            }
        }

        public async Task<ApiResult> DeleteScheduledTaskAsync(Guid id)
        {
            var scheduledTask = _scheduledTasksRepository.Get(id);

            if (scheduledTask == null)
            {
                return new ApiResult
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsFailed = true,
                    Errors = new List<string> { "Scheduled task not found." }
                };
            }

            if (scheduledTask.IsExecuted)
            {
                return new ApiResult
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsFailed = true,
                    Errors = new List<string> { "Scheduled task already executed." }
                };
            }

            _scheduledTasksRepository.Delete(scheduledTask);

            var changeCount = await _scheduledTasksRepository.SaveChangesAsync();
            if (changeCount > 0)
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                };
            }
            else
            {
                return new ApiResult<ScheduledTask>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsFailed = true,
                    Errors = new List<string>
                    { "An unexpected result was encountered while saving the changes." }
                };
            }
        }

        public async Task RunScheduledTasksAsync(CancellationToken cancellationToken)
        {
            var scheduledTasks = _scheduledTasksRepository.GetAllExecutable();

            foreach (var scheduledTask in scheduledTasks)
            {
                _logger.LogInformation("Executing scheduled task {Title}", scheduledTask.Title);
                scheduledTask.IsExecuted = true;
                scheduledTask.UpdatedAt = DateTime.UtcNow;
                scheduledTask.ExecutedAt = scheduledTask.UpdatedAt;
            }

            _ = await _scheduledTasksRepository.SaveChangesAsync();
        }

        private ICollection<string> ValidateRequest(ScheduledTaskRequest request)
        {
            var errors = new List<string>();

            if (request.ScheduledTime <= DateTime.UtcNow)
            {
                errors.Add("ScheduledTime must be on a future date/time.");
            }

            if (string.IsNullOrEmpty(request.Title))
            {
                errors.Add("Title should not be empty.");
            }

            if (string.IsNullOrEmpty(request.Description))
            {
                errors.Add("Description should not be empty.");
            }

            if (request.Title.Length > 100)
            {
                errors.Add("Title should be at most 100 characters long.");
            }

            if (request.Description.Length > 200)
            {
                errors.Add("Description should be at most 200 characters long.");
            }

            return errors;
        }
    }
}
