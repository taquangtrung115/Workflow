using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflow.Models
{
    /// <summary>
    /// Một level/cấp độ trong workflow template
    /// </summary>
    public class WorkflowLevel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TemplateId { get; set; }

        [ForeignKey(nameof(TemplateId))]
        public virtual WorkflowTemplate? Template { get; set; }

        /// <summary>
        /// Thứ tự level (1, 2, 3, ...)
        /// </summary>
        [Required]
        public int Order { get; set; }

        /// <summary>
        /// Loại approver: "Department" hoặc "Users"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ApproverType { get; set; } = "Users";

        /// <summary>
        /// ID phòng ban (nếu ApproverType == "Department")
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// JSON array của User IDs (nếu ApproverType == "Users")
        /// Ví dụ: ["guid1", "guid2"]
        /// </summary>
        public string? UserIdsJson { get; set; }

        /// <summary>
        /// Số lượng approvals cần thiết tại level này
        /// </summary>
        [Required]
        public int RequiredApprovals { get; set; } = 1;

        /// <summary>
        /// JSON array của allowed file types (mime types hoặc extensions)
        /// Ví dụ: ["application/pdf", ".pdf", ".docx"]
        /// </summary>
        public string? AllowedFileTypesJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
