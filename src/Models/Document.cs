using System;
using System.ComponentModel.DataAnnotations;

namespace Workflow.Models
{
    /// <summary>
    /// Đại diện cho một file/tài liệu được upload
    /// </summary>
    public class Document
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(500)]
        public string Filename { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? MimeType { get; set; }

        [Required]
        [MaxLength(1000)]
        public string BlobUrl { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public Guid UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
