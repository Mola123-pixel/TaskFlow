using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.WorkOrder;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkOrdersController : ControllerBase
    {
        // Replace ApplicationDbContext with your actual DbContext type if different.
        private readonly TaskFlowDBContext _context;

        public WorkOrdersController(TaskFlowDBContext context)
        {       
            _context = context;
        }

        // GET: api/workorders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetAll()
        {
            var list = await _context.Set<WorkOrderDto>()
                                     .OrderBy(w => w.Id)
                                     .ToListAsync();
            return Ok(list);
        }

        // GET: api/workorders/{id}
        [HttpGet("{id:Guid}", Name = nameof(Get))]
        public async Task<IActionResult> Get(Guid id)
        {
            var workOrder = await _context.Set<WorkOrderDto>().FindAsync(id);
            if (workOrder == null)
                return NotFound();

            return Ok(workOrder);
        }

        // POST: api/workorders
        [HttpPost]
        public async Task<ActionResult<WorkOrderDto>> Create([FromBody] WorkOrderCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var workOrder = new WorkOrderDto
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Status = Enum.TryParse<Status>(dto.Status, true, out var parsedStatus) ? parsedStatus : Status.Open,
                Priority = Enum.TryParse<Priority>(dto.Priority, true, out var parsedPriority) ? parsedPriority : Priority.Medium,
                AssignedTo = dto.AssignedTo,
                DueDate = dto.DueDate ?? DateTime.UtcNow,
                StatusChanges = new List<StatusChange>()
            };

            _context.Set<WorkOrderDto>().Add(workOrder);
            await _context.SaveChangesAsync();

            return CreatedAtRoute(nameof(Get), new { id = workOrder.Id }, workOrder);
        }

        // PUT: api/workorders/{id}
        [HttpPut("{id:Guid}")]
        public async Task<ActionResult<WorkOrderDto>> Update(Guid id, [FromBody] WorkOrderUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Set<WorkOrderDto>().FindAsync(id);
            if (existing == null)
                return NotFound();

            // parse Status string -> Status enum, falling back to existing.Status
            var newStatus = existing.Status;
            if (!string.IsNullOrWhiteSpace(dto.Status) &&
                Enum.TryParse<Status>(dto.Status, true, out var parsedStatus))
            {
                newStatus = parsedStatus;
            }

            // parse Priority string -> Priority enum, falling back to existing.Priority
            var newPriority = existing.Priority;
            if (!string.IsNullOrWhiteSpace(dto.Priority) &&
                Enum.TryParse<Priority>(dto.Priority, true, out var parsedPriority))
            {
                newPriority = parsedPriority;
            }

            existing.Title = dto.Title ?? existing.Title;
            existing.Status = newStatus;
            existing.Priority = newPriority;
            existing.AssignedTo = dto.AssignedTo ?? existing.AssignedTo;
            existing.DueDate = dto.DueDate ?? existing.DueDate;
            // keep existing.StatusChanges unless DTO provides modifications (not handled here)

            _context.Set<WorkOrderDto>().Update(existing);
            await _context.SaveChangesAsync();

            return Ok(existing);
        }
    }
}
