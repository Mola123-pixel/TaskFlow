using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.StatusChange;
using TaskFlow.Application.DTOs.WorkOrder;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence.Data;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly TaskFlowDBContext _context;

    public WorkOrdersController(TaskFlowDBContext context)
    {
        _context = context;
    }

    // GET: api/WorkOrders
    // GET: api/WorkOrders?status=InProgress
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetAll(
        [FromQuery] string? status)
    {
        var query = _context.WorkOrders
            .AsNoTracking()
            .Where(w => !w.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<Status>(status, true, out var parsedStatus))
            {
                return BadRequest($"Invalid status: {status}");
            }

            query = query.Where(w => w.Status == parsedStatus);
        }

        var workOrders = await query
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WorkOrderDto
            {
                Id = w.Id,
                Title = w.Title,
                Status = w.Status,
                Priority = w.Priority,
                AssignedTo = w.AssignedTo,
                DueDate = w.DueDate
            })
            .ToListAsync();

        return Ok(workOrders);
    }

    // GET: api/WorkOrders/{id}
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    public async Task<ActionResult<WorkOrderDto>> GetById(Guid id)
    {
        var workOrder = await _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.StatusChanges)
            .FirstOrDefaultAsync(w =>
                w.Id == id &&
                !w.IsDeleted);

        if (workOrder == null)
        {
            return NotFound();
        }

        var result = new WorkOrderDto
        {
            Id = workOrder.Id,
            Title = workOrder.Title,
            Status = workOrder.Status,
            Priority = workOrder.Priority,
            AssignedTo = workOrder.AssignedTo,
            DueDate = workOrder.DueDate,

            StatusChanges = workOrder.StatusChanges?
                .OrderByDescending(x => x.ChangedAt)
                .Take(10)
                .Select(x => new WorkOrderStatusChangeDto
                {
                    Id = x.Id,
                    WorkOrderId = x.WorkOrderId,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    ChangedAt = x.ChangedAt,
                    ChangedBy = x.ChangedBy
                })
                .ToList()
                ?? new List<WorkOrderStatusChangeDto>()
        };

        return Ok(result);
    }

    // POST: api/WorkOrders
    [HttpPost]
    public async Task<ActionResult<WorkOrderDto>> Create(
        [FromBody] WorkOrderCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!Enum.TryParse<Status>(
                dto.Status,
                true,
                out var status))
        {
            return BadRequest($"Invalid status: {dto.Status}");
        }

        if (!Enum.TryParse<Priority>(
                dto.Priority,
                true,
                out var priority))
        {
            return BadRequest($"Invalid priority: {dto.Priority}");
        }

        var now = DateTime.UtcNow;

        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Status = status,
            Priority = priority,
            AssignedTo = dto.AssignedTo,
            DueDate = (DateTime)dto.DueDate,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync();

        var result = new WorkOrderDto
        {
            Id = workOrder.Id,
            Title = workOrder.Title,
            Status = workOrder.Status,
            Priority = workOrder.Priority,
            AssignedTo = workOrder.AssignedTo,
            DueDate = workOrder.DueDate
        };

        return CreatedAtRoute(
            nameof(GetById),
            new { id = workOrder.Id },
            result);
    }

    // PUT: api/WorkOrders/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkOrderDto>> Update(
        Guid id,
        [FromBody] WorkOrderUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w =>
                w.Id == id &&
                !w.IsDeleted);

        if (workOrder == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(dto.Priority))
        {
            if (!Enum.TryParse<Priority>(
                    dto.Priority,
                    true,
                    out var priority))
            {
                return BadRequest($"Invalid priority: {dto.Priority}");
            }

            workOrder.Priority = priority;
        }

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            workOrder.Title = dto.Title;
        }

        if (!string.IsNullOrWhiteSpace(dto.AssignedTo))
        {
            workOrder.AssignedTo = dto.AssignedTo;
        }

        if (dto.DueDate.HasValue)
        {
            workOrder.DueDate = dto.DueDate.Value;
        }

        // Status is deliberately not updated here.
        // Use PATCH /api/WorkOrders/{id}/status

        workOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var result = new WorkOrderDto
        {
            Id = workOrder.Id,
            Title = workOrder.Title,
            Status = workOrder.Status,
            Priority = workOrder.Priority,
            AssignedTo = workOrder.AssignedTo,
            DueDate = workOrder.DueDate
        };

        return Ok(result);
    }

    // PATCH: api/WorkOrders/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] WorkOrderStatusChangeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w =>
                w.Id == id &&
                !w.IsDeleted);

        if (workOrder == null)
        {
            return NotFound();
        }

        var oldStatus = workOrder.Status;
        var newStatus = dto.ToStatus;

        if (oldStatus == newStatus)
        {
            return BadRequest(
                "The work order is already in the requested status.");
        }

        var now = DateTime.UtcNow;

        // Update work order
        workOrder.Status = newStatus;
        workOrder.UpdatedAt = now;

        // Create status history
        var statusChange =
            new StatusChange
            {
                Id = Guid.NewGuid(),
                WorkOrderId = workOrder.Id,
                FromStatus = oldStatus,
                ToStatus = newStatus,
                ChangedAt = now,
                ChangedBy = dto.ChangedBy
            };

        _context.StatusChanges.Add(statusChange);

        // Persist both changes in the same transaction
        await _context.SaveChangesAsync();

        return NoContent();
    }
}