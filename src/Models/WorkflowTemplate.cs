using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Workflow.Models
{
    /// <summary>
    /// Template định nghĩa các level duyệt cho workflow
    /// </summary>
    public class WorkflowTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<WorkflowLevel> Levels { get; set; } = new List<WorkflowLevel>();
    }
}
