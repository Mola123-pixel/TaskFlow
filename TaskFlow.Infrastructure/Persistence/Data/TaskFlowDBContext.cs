using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Data
{
    public class TaskFlowDBContext : DbContext
    {
        public TaskFlowDBContext(DbContextOptions<TaskFlowDBContext> options) : base(options)
        {
        }

        public DbSet<WorkOrder> WorkOrders { get; set; } = null!;
        public DbSet<StatusChange> StatusChanges { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // store enums as strings for readability
            modelBuilder.Entity<WorkOrder>()
                .Property(w => w.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<WorkOrder>()
                .Property(w => w.Priority)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<StatusChange>()
                .Property(s => s.FromStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<StatusChange>()
                .Property(s => s.ToStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<StatusChange>()
                .HasOne(s => s.WorkOrder)
                .WithMany(w => w.StatusChanges)
                .HasForeignKey(s => s.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}