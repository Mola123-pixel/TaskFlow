using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TaskFlow.Infrastructure.Persistence.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TaskFlowDBContext>
    {
        public TaskFlowDBContext CreateDbContext(string[] args)
        {
            // EF may run from the infrastructure folder, startup folder, or repo root.
            var basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile(Path.Combine("..", "TaskFlow", "appsettings.json"), optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = config.GetConnectionString("DefaultConnection")
                       ?? "Server=(localdb)\\MSSQLLocalDB;Database=TaskFlow;Trusted_Connection=True;TrustServerCertificate=True;";

            var options = new DbContextOptionsBuilder<TaskFlowDBContext>();
            options.UseSqlServer(conn);

            return new TaskFlowDBContext(options.Options);
        }
    }
}