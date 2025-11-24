using System;
using System.Collections.Generic;

namespace Workflow.DTOs
{
    public class CreateTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public List<CreateLevelRequest> Levels { get; set; } = new();
    }

    public class CreateLevelRequest
    {
        public int Order { get; set; }
        public string ApproverType { get; set; } = "Users"; // "Department" or "Users"
        public Guid? DepartmentId { get; set; }
        public List<Guid>? UserIds { get; set; }
        public int RequiredApprovals { get; set; } = 1;
        public List<string>? AllowedFileTypes { get; set; }
    }

    public class TemplateResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<LevelResponse> Levels { get; set; } = new();
    }

    public class LevelResponse
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string ApproverType { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public List<Guid>? UserIds { get; set; }
        public int RequiredApprovals { get; set; }
        public List<string>? AllowedFileTypes { get; set; }
    }

    public class StartWorkflowRequest
    {
        public Guid DocumentId { get; set; }
    }

    public class WorkflowInstanceResponse
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string DocumentFilename { get; set; } = string.Empty;
        public int CurrentLevelOrder { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid RequestedBy { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public List<ApprovalRecordResponse> ApprovalRecords { get; set; } = new();
    }

    public class ApprovalRecordResponse
    {
        public Guid Id { get; set; }
        public int LevelOrder { get; set; }
        public Guid ApproverUserId { get; set; }
        public bool Approved { get; set; }
        public string? Comment { get; set; }
        public DateTime SignedAt { get; set; }
    }

    public class ApproveRequest
    {
        public string? SignatureBase64 { get; set; }
        public string? Comment { get; set; }
    }

    public class RejectRequest
    {
        public string Comment { get; set; } = string.Empty;
    }
}
