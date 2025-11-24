using System;
using System.Collections.Generic;

namespace Workflow.DTOs
{
    public class CreateFileTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Mime { get; set; } = string.Empty;
        public List<string> Extensions { get; set; } = new();
    }

    public class FileTypeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Mime { get; set; } = string.Empty;
        public List<string> Extensions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
