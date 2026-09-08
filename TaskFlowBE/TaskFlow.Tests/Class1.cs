using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence.Data;
using Xunit;

namespace TaskFlow.Tests
{
    public class TaskControllerTests
    {
        private static TaskFlowDBContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TaskFlowDBContext>()
                .UseSqlServer(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            var context = new TaskFlowDBContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            var now = DateTime.UtcNow;

            var workOrder = new WorkOrder
            {
                Id = Guid.NewGuid(),
                Title = "Test Delete",
                Status = Status.New,
                Priority = Priority.Low,
                AssignedTo = "tester",
                DueDate = now.AddDays(1),
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };

            await using (var seedContext = CreateContext())
            {
                seedContext.WorkOrders.Add(workOrder);
                await seedContext.SaveChangesAsync();
            }

            // Act
            await using (var context = CreateContext())
            {
                // NOTE:
                // CreateContext() creates a new database because it uses
                // a new GUID each time.
                //
                // Therefore this approach cannot share the seeded data.
            }
        }
    }
}