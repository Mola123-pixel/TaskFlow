using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs
{
    public class StatusChangeDto
    {
        [Required]
        [ForeignKey(nameof(WorkOrder))]
        public Guid WorkOrderId { get; set; }

        [Required]
        [EnumDataType(typeof(Status))]
        public Status FromStatus { get; set; }

        [Required]
        [EnumDataType(typeof(Status))]
        public Status ToStatus { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ChangedAt { get; set; }

        [Required]
        [StringLength(100)]
        public string ChangedBy { get; set; } = string.Empty;

        [Required]
        public WorkOrder WorkOrder { get; set; } = null!;
    }
}
