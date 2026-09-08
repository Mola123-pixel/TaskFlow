using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;
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
            modelBuilder.Entity<WorkOrder>()
                .Property(w => w.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            base.OnModelCreating(modelBuilder);
        }

    }
}
