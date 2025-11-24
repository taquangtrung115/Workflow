using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflow.Models
{
    /// <summary>
    /// Ghi lại một lần approve hoặc reject
    /// </summary>
    public class ApprovalRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InstanceId { get; set; }

        [ForeignKey(nameof(InstanceId))]
        public virtual WorkflowInstance? Instance { get; set; }

        /// <summary>
        /// Level mà approval này thuộc về
        /// </summary>
        [Required]
        public int LevelOrder { get; set; }

        [Required]
        public Guid ApproverUserId { get; set; }

        /// <summary>
        /// true = approved, false = rejected
        /// </summary>
        [Required]
        public bool Approved { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }

        /// <summary>
        /// Chữ ký số (base64 hoặc blob reference)
        /// </summary>
        public string? SignatureBlob { get; set; }

        public DateTime SignedAt { get; set; } = DateTime.UtcNow;
    }
}
