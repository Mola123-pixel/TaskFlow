using System;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities
{
    public class StatusChange
    {
        public Guid Id { get; set; }
        public Guid WorkOrderId { get; set; }
        public Status FromStatus { get; set; }
        public Status ToStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }

        public WorkOrder WorkOrder { get; set; }
    }
}
