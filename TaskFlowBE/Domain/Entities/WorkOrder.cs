using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities
{
    public class WorkOrder : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        public Status Status { get; set; } = Status.Open;

        public Priority Priority { get; set; } = Priority.Medium;

        public string AssignedTo { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public ICollection<StatusChange> StatusChanges { get; set; } = new List<StatusChange>();
    }
}
