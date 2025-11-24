using System;
using System.ComponentModel.DataAnnotations;

namespace Workflow.Models
{
    /// <summary>
    /// Định nghĩa loại file mà hệ thống nhận diện (mime type + extensions)
    /// </summary>
    public class FileType
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Mime { get; set; } = string.Empty;

        /// <summary>
        /// JSON array của các extensions, ví dụ: [".pdf", ".docx"]
        /// </summary>
        public string ExtensionsJson { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
