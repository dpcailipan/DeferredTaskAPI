using DeferredTaskAPI.Services.Interfaces;

namespace DeferredTaskAPI.Services
{
    public sealed class ScheduledTaskProcessorService : BackgroundService
    {
        private readonly ILogger<ScheduledTaskProcessorService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScheduledTaskProcessorService(
            ILogger<ScheduledTaskProcessorService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting ScheduledTaskProcessorService");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    var scheduledTaskService = scope.ServiceProvider
                        .GetService<IScheduledTaskService>();

                    if (scheduledTaskService == null)
                    {
                        throw new NotImplementedException(
                            "No implementation found for IScheduledTaskService");
                    }

                    await scheduledTaskService.RunScheduledTasksAsync(stoppingToken);

                    using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

                    while (await timer.WaitForNextTickAsync(stoppingToken))
                    {
                        _logger.LogInformation("Running task again at {time}", DateTime.Now);
                        await scheduledTaskService.RunScheduledTasksAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error encountered while running ScheduledTaskProcessorService.");
                }
            }
        }
    }
}
