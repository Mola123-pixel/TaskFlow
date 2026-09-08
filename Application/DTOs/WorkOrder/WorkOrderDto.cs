using System;
using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.DTOs.StatusChange;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.WorkOrder
{
    public class WorkOrderDto
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [EnumDataType(typeof(Status))]
        public Status Status { get; set; } = Status.Open;

        [Required]
        [EnumDataType(typeof(Priority))]
        public Priority Priority { get; set; } = Priority.Medium;

        [Required]
        [StringLength(100)]
        public string AssignedTo { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
        public ICollection<WorkOrderStatusChangeDto> StatusChanges { get; set; } = new List<WorkOrderStatusChangeDto>();
    }
}
