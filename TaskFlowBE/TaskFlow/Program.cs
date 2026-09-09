using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence.Data;
using TaskFlow.Infrastructure.Persistence.Seed_Data;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Database
// ============================================================

builder.Services.AddDbContext<TaskFlowDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// ============================================================
// Swagger
// ============================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================
// CORS
// ============================================================

var allowedOrigins =
    builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[]
    {
        "http://localhost:5173",
        "https://localhost:5173",
        "http://127.0.0.1:5173",
        "https://127.0.0.1:5173"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("TaskFlowCors", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ============================================================
// Build application
// ============================================================

var app = builder.Build();

// ============================================================
// Swagger
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "TaskFlow API v1");

        c.RoutePrefix = string.Empty;
    });
}

// ============================================================
// Seed Data
// ============================================================

if (builder.Configuration.GetValue<bool>("SeedData:Enable"))
{
    using var scope = app.Services.CreateScope();

    var ctx = scope.ServiceProvider
        .GetRequiredService<TaskFlowDBContext>();

    // Apply pending migrations
    ctx.Database.Migrate();

    // Only seed when there are no existing work orders
    if (!ctx.WorkOrders.Any())
    {
        var now = DateTime.UtcNow;

        var seed =
            SeedData.GetDemoWorkOrders()
            ?? Enumerable.Empty<dynamic>();

        var workOrders =
            new List<TaskFlow.Domain.Entities.WorkOrder>();

        foreach (var s in seed)
        {
            var statusParsed =
                TaskFlow.Domain.Enums.Status.Open;

            var priorityParsed =
                TaskFlow.Domain.Enums.Priority.Medium;

            // Parse Status from seed data
            if (!string.IsNullOrWhiteSpace(s?.Status))
            {
                Enum.TryParse(
                    s.Status,
                    true,
                    out statusParsed);
            }

            // Parse Priority from seed data
            if (!string.IsNullOrWhiteSpace(s?.Priority))
            {
                Enum.TryParse(
                    s.Priority,
                    true,
                    out priorityParsed);
            }

            workOrders.Add(
                new TaskFlow.Domain.Entities.WorkOrder
                {
                    Id = Guid.NewGuid(),

                    Title = s?.Title,

                    Status = statusParsed,

                    Priority = priorityParsed,

                    AssignedTo = s?.AssignedTo,

                    DueDate = s?.DueDate,

                    CreatedAt = now,

                    UpdatedAt = now,

                    IsDeleted = false
                });
        }

        if (workOrders.Count > 0)
        {
            ctx.WorkOrders.AddRange(workOrders);

            ctx.SaveChanges();
        }
    }
}

// ============================================================
// Middleware
// ============================================================

app.UseHttpsRedirection();

app.UseCors("TaskFlowCors");

app.UseAuthorization();

app.MapControllers();

app.Run();