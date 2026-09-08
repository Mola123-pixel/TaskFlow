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

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] System.Text.Json.JsonElement body)
    {
        if (body.ValueKind == System.Text.Json.JsonValueKind.Undefined ||
            body.ValueKind == System.Text.Json.JsonValueKind.Null)
        {
            ModelState.AddModelError("dto", "The dto field is required.");
            return ValidationProblem(ModelState);
        }

        var dto = new WorkOrderStatusChangeDto();

        if (body.TryGetProperty("changedBy", out var changedByProp) &&
            changedByProp.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            dto.ChangedBy = changedByProp.GetString();
        }

        TaskFlow.Domain.Enums.Status newStatus;
        if (!body.TryGetProperty("toStatus", out var toStatusProp))
        {
            ModelState.AddModelError("$.toStatus", "The dto.toStatus field is required.");
            return ValidationProblem(ModelState);
        }

        var parsed = false;
        if (toStatusProp.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            var s = toStatusProp.GetString();
            parsed = Enum.TryParse<TaskFlow.Domain.Enums.Status>(s, true, out newStatus);
        }
        else if (toStatusProp.ValueKind == System.Text.Json.JsonValueKind.Number &&
                 toStatusProp.TryGetInt32(out var intVal))
        {
            newStatus = (TaskFlow.Domain.Enums.Status)intVal;
            parsed = Enum.IsDefined(typeof(TaskFlow.Domain.Enums.Status), newStatus);
        }
        else
        {
            parsed = false;
            newStatus = default!;
        }

        if (!parsed)
        {
            ModelState.AddModelError("$.toStatus",
                "The JSON value could not be converted to TaskFlow.Domain.Enums.Status.");
            return ValidationProblem(ModelState);
        }

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
        var requestedStatus = newStatus;

        if (oldStatus == requestedStatus)
        {
            return BadRequest(
                "The work order is already in the requested status.");
        }

        var now = DateTime.UtcNow;

        workOrder.Status = requestedStatus;
        workOrder.UpdatedAt = now;

        // Create status history
        var statusChange = new TaskFlow.Domain.Entities.StatusChange
        {
            Id = Guid.NewGuid(),
            WorkOrderId = workOrder.Id,
            FromStatus = oldStatus,
            ToStatus = requestedStatus,
            ChangedAt = now,
            ChangedBy = dto.ChangedBy,
        };

        _context.StatusChanges.Add(statusChange);

        // Persist both changes
        await _context.SaveChangesAsync();

        return NoContent();
    }
}