using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflow.Models
{
    /// <summary>
    /// Gán quyền duyệt loại file cho user
    /// </summary>
    public class UserFileTypePermission
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid FileTypeId { get; set; }

        [ForeignKey(nameof(FileTypeId))]
        public virtual FileType? FileType { get; set; }

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    }
}
