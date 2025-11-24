using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflow.Models
{
    /// <summary>
    /// Một instance cụ thể của workflow đang chạy
    /// </summary>
    public class WorkflowInstance
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TemplateId { get; set; }

        [ForeignKey(nameof(TemplateId))]
        public virtual WorkflowTemplate? Template { get; set; }

        [Required]
        public Guid DocumentId { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document? Document { get; set; }

        /// <summary>
        /// Level hiện tại (Order của WorkflowLevel)
        /// </summary>
        public int CurrentLevelOrder { get; set; } = 1;

        /// <summary>
        /// Trạng thái: InProgress, Approved, Rejected, Cancelled
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "InProgress";

        [Required]
        public Guid RequestedBy { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }

        public virtual ICollection<ApprovalRecord> ApprovalRecords { get; set; } = new List<ApprovalRecord>();
    }
}
