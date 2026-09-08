using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Data
{
    public class TaskFlowDBContext : DbContext
    {
        public TaskFlowDBContext(DbContextOptions<TaskFlowDBContext> options)
            : base(options)
        {
        }

        public DbSet<WorkOrder> WorkOrders { get; set; } = null!;
        public DbSet<StatusChange> StatusChanges { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure enum values are stored as strings to match existing nvarchar(50) columns
            modelBuilder.Entity<WorkOrder>()
                .Property(w => w.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<StatusChange>()
                .Property(sc => sc.FromStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<StatusChange>()
                .Property(sc => sc.ToStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<StatusChange>()
                .HasOne(s => s.WorkOrder)
                .WithMany(w => w.StatusChanges)
                .HasForeignKey(s => s.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}