using System;

namespace Workflow.DTOs
{
    public class GrantFileTypePermissionRequest
    {
        public Guid UserId { get; set; }
        public Guid FileTypeId { get; set; }
    }

    public class UserFileTypePermissionResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid FileTypeId { get; set; }
        public string FileTypeName { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
    }
}
