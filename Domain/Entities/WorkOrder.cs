using TaskFlow.Domain.Enums;

using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities
{
    public class WorkOrder : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        public Status Status { get; set; } = Status.Open;

        public Priority Priority { get; set; } = Priority.Medium;

        public string AssignedTo { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }
    }
}
