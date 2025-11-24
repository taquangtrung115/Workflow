using System;

namespace Workflow.DTOs
{
    public class DocumentResponse
    {
        public Guid Id { get; set; }
        public string Filename { get; set; } = string.Empty;
        public string? MimeType { get; set; }
        public string BlobUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Guid UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
