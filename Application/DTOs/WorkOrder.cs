using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs
{
    public class WorkOrder
    {
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
        public ICollection<StatusChange> StatusChanges { get; set; } = new List<StatusChange>();
    }
}
