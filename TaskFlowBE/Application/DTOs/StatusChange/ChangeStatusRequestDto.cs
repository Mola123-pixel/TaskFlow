using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskFlow.Application.DTOs.StatusChange
{
    public class ChangeStatusRequestDto
    {
        [Required, StringLength(100)]
        public string ChangedBy { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string NewStatus { get; set; } = string.Empty;
    }
}
