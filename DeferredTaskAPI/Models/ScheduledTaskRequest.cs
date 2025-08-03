namespace DeferredTaskAPI.Models
{
    public class ScheduledTaskRequest
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime ScheduledTime { get; set; }
    }
}
