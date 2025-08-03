using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeferredTaskAPI.Entities
{
    public class ScheduledTask
    {
        [Key]
        public Guid Id { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public required string Title { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public required string Description { get; set; }
        public DateTime ScheduledTime { get; set; }
        public bool IsExecuted { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
