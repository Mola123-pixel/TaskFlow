using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskFlow
{
    public class StatusChange
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid WorkOrderId { get; set; }

        // Changed from int to string to match WorkOrder.Status
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string FromStatus { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string ToStatus { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string ChangedBy { get; set; } = string.Empty;
    }
}