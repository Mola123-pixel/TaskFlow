using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities
{
    public class StatusChange : BaseEntity
    {
        public Guid WorkOrderId { get; set; }

        public Status FromStatus { get; set; }

        public Status ToStatus { get; set; }

        public DateTime ChangedAt { get; set; }

        public string ChangedBy { get; set; } = string.Empty;

        public WorkOrder WorkOrder { get; set; } = null!;
    }
}
