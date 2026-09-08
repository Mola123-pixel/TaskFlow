using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskFlow.Application.DTOs.WorkOrder
{
    public class WorkOrderCreateDto
    {
        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Status { get; set; }

        [StringLength(2000)]
        public string? Priority { get; set; }

        [StringLength(100)]
        public string? AssignedTo { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
    }
}
