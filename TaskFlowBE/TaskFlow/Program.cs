using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence.Data;
using TaskFlow.Infrastructure.Persistence.Seed_Data;

var builder = WebApplication.CreateBuilder(args);

// DbContext (Infrastructure project)
builder.Services.AddDbContext<TaskFlowDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers + JSON enum as strings
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
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
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Optional seed: guarded by configuration key "SeedData:Enable" (default false)
if (builder.Configuration.GetValue<bool>("SeedData:Enable"))
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<TaskFlowDBContext>();
    // apply migrations if any / ensure database exists
    ctx.Database.Migrate();

    if (!ctx.WorkOrders.Any())
    {
        var now = DateTime.UtcNow;

        var seed = SeedData.GetDemoWorkOrders() ?? Enumerable.Empty<dynamic>();
        var workOrders = new List<TaskFlow.Domain.Entities.WorkOrder>();

        foreach (var s in seed)
        {
            var statusParsed = TaskFlow.Domain.Enums.Status.Open;
            var priorityParsed = TaskFlow.Domain.Enums.Priority.Medium;

            if (!string.IsNullOrWhiteSpace(s?.Status))
                Enum.TryParse<TaskFlow.Domain.Enums.Status>(s.Status, true, out statusParsed);

            if (!string.IsNullOrWhiteSpace(s?.Priority))
                Enum.TryParse<TaskFlow.Domain.Enums.Priority>(s.Priority, true, out priorityParsed);

            workOrders.Add(new TaskFlow.Domain.Entities.WorkOrder
            {
                Title = s?.Title,
                Status = statusParsed,
                Priority = priorityParsed,
                AssignedTo = s?.AssignedTo,
                DueDate = s?.DueDate,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (workOrders.Count > 0)
        {
            ctx.WorkOrders.AddRange(workOrders);
            ctx.SaveChanges();
        }
    }
}

app.UseHttpsRedirection();
app.UseCors("TaskFlowCors");
app.UseAuthorization();
app.MapControllers();
app.Run();