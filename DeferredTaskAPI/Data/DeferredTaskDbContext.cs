using DeferredTaskAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeferredTaskAPI.Data
{
    public class DeferredTaskDbContext : DbContext
    {
        public DeferredTaskDbContext(DbContextOptions<DeferredTaskDbContext> options)
            : base(options) { }

        public DbSet<ScheduledTask> ScheduledTasks { get; set; }
    }
}
