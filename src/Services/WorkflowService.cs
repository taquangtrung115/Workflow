using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Workflow.Data;
using Workflow.DTOs;
using Workflow.Models;

namespace Workflow.Services
{
    /// <summary>
    /// Service quản lý workflow instances và approval process
    /// </summary>
    public interface IWorkflowService
    {
        Task<WorkflowInstanceResponse> StartWorkflowAsync(Guid templateId, StartWorkflowRequest request, Guid requestedBy);
        Task<WorkflowInstanceResponse> ApproveAsync(Guid instanceId, ApproveRequest request, Guid approverId);
        Task<WorkflowInstanceResponse> RejectAsync(Guid instanceId, RejectRequest request, Guid approverId);
        Task<WorkflowInstanceResponse?> GetInstanceAsync(Guid instanceId);
        Task<List<WorkflowInstanceResponse>> GetUserPendingApprovalsAsync(Guid userId);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly WorkflowDbContext _context;
        private readonly IFileTypeService _fileTypeService;
        private readonly IPermissionService _permissionService;

        public WorkflowService(
            WorkflowDbContext context,
            IFileTypeService fileTypeService,
            IPermissionService permissionService)
        {
            _context = context;
            _fileTypeService = fileTypeService;
            _permissionService = permissionService;
        }

        public async Task<WorkflowInstanceResponse> StartWorkflowAsync(Guid templateId, StartWorkflowRequest request, Guid requestedBy)
        {
            // Validate template exists
            var template = await _context.WorkflowTemplates
                .Include(t => t.Levels)
                .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive);

            if (template == null)
                throw new InvalidOperationException("Template không tồn tại hoặc không active");

            // Validate document exists
            var document = await _context.Documents.FindAsync(request.DocumentId);
            if (document == null)
                throw new InvalidOperationException("Document không tồn tại");

            // Get first level
            var firstLevel = template.Levels.OrderBy(l => l.Order).FirstOrDefault();
            if (firstLevel == null)
                throw new InvalidOperationException("Template phải có ít nhất một level");

            // Create workflow instance
            var instance = new WorkflowInstance
            {
                TemplateId = templateId,
                DocumentId = request.DocumentId,
                CurrentLevelOrder = firstLevel.Order,
                Status = "InProgress",
                RequestedBy = requestedBy
            };

            _context.WorkflowInstances.Add(instance);
            await _context.SaveChangesAsync();

            return await MapToResponseAsync(instance);
        }

        public async Task<WorkflowInstanceResponse> ApproveAsync(Guid instanceId, ApproveRequest request, Guid approverId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Load instance với các related data
                var instance = await _context.WorkflowInstances
                    .Include(i => i.Template)
                        .ThenInclude(t => t.Levels)
                    .Include(i => i.Document)
                    .Include(i => i.ApprovalRecords)
                    .FirstOrDefaultAsync(i => i.Id == instanceId);

                if (instance == null)
                    throw new InvalidOperationException("Workflow instance không tồn tại");

                if (instance.Status != "InProgress")
                    throw new InvalidOperationException($"Workflow đã ở trạng thái {instance.Status}, không thể approve");

                // 2. Get current level
                var currentLevel = instance.Template?.Levels
                    .FirstOrDefault(l => l.Order == instance.CurrentLevelOrder);

                if (currentLevel == null)
                    throw new InvalidOperationException("Level hiện tại không tồn tại");

                // 3. Validate document file type với AllowedFileTypes của level
                await ValidateDocumentFileTypeAsync(instance.Document!, currentLevel);

                // 4. Validate user có quyền approve file type
                await ValidateUserFileTypePermissionAsync(approverId, instance.Document!);

                // 5. Validate user có trong scope của level
                await ValidateUserInLevelScopeAsync(approverId, currentLevel);

                // 6. Kiểm tra user đã approve chưa
                var existingApproval = instance.ApprovalRecords
                    .FirstOrDefault(r => r.LevelOrder == instance.CurrentLevelOrder && r.ApproverUserId == approverId);

                if (existingApproval != null)
                    throw new InvalidOperationException("User đã approve ở level này rồi");

                // 7. Create approval record
                var approvalRecord = new ApprovalRecord
                {
                    InstanceId = instanceId,
                    LevelOrder = instance.CurrentLevelOrder,
                    ApproverUserId = approverId,
                    Approved = true,
                    Comment = request.Comment,
                    SignatureBlob = request.SignatureBase64
                };

                _context.ApprovalRecords.Add(approvalRecord);

                // 8. Kiểm tra số approvals đã đủ chưa
                var approvalCount = instance.ApprovalRecords
                    .Count(r => r.LevelOrder == instance.CurrentLevelOrder && r.Approved) + 1; // +1 for current approval

                if (approvalCount >= currentLevel.RequiredApprovals)
                {
                    // Move to next level or complete
                    var nextLevel = instance.Template.Levels
                        .Where(l => l.Order > instance.CurrentLevelOrder)
                        .OrderBy(l => l.Order)
                        .FirstOrDefault();

                    if (nextLevel != null)
                    {
                        instance.CurrentLevelOrder = nextLevel.Order;
                    }
                    else
                    {
                        // Workflow completed
                        instance.Status = "Approved";
                        instance.ClosedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await MapToResponseAsync(instance);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<WorkflowInstanceResponse> RejectAsync(Guid instanceId, RejectRequest request, Guid approverId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var instance = await _context.WorkflowInstances
                    .Include(i => i.Template)
                        .ThenInclude(t => t.Levels)
                    .Include(i => i.Document)
                    .Include(i => i.ApprovalRecords)
                    .FirstOrDefaultAsync(i => i.Id == instanceId);

                if (instance == null)
                    throw new InvalidOperationException("Workflow instance không tồn tại");

                if (instance.Status != "InProgress")
                    throw new InvalidOperationException($"Workflow đã ở trạng thái {instance.Status}, không thể reject");

                var currentLevel = instance.Template?.Levels
                    .FirstOrDefault(l => l.Order == instance.CurrentLevelOrder);

                if (currentLevel == null)
                    throw new InvalidOperationException("Level hiện tại không tồn tại");

                // Validate user permissions (similar to approve)
                await ValidateDocumentFileTypeAsync(instance.Document!, currentLevel);
                await ValidateUserFileTypePermissionAsync(approverId, instance.Document!);
                await ValidateUserInLevelScopeAsync(approverId, currentLevel);

                // Create rejection record
                var approvalRecord = new ApprovalRecord
                {
                    InstanceId = instanceId,
                    LevelOrder = instance.CurrentLevelOrder,
                    ApproverUserId = approverId,
                    Approved = false,
                    Comment = request.Comment
                };

                _context.ApprovalRecords.Add(approvalRecord);

                // Set workflow to rejected
                instance.Status = "Rejected";
                instance.ClosedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await MapToResponseAsync(instance);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<WorkflowInstanceResponse?> GetInstanceAsync(Guid instanceId)
        {
            var instance = await _context.WorkflowInstances
                .Include(i => i.Template)
                .Include(i => i.Document)
                .Include(i => i.ApprovalRecords)
                .FirstOrDefaultAsync(i => i.Id == instanceId);

            return instance != null ? await MapToResponseAsync(instance) : null;
        }

        public async Task<List<WorkflowInstanceResponse>> GetUserPendingApprovalsAsync(Guid userId)
        {
            // Lấy tất cả instances đang InProgress
            var instances = await _context.WorkflowInstances
                .Include(i => i.Template)
                    .ThenInclude(t => t.Levels)
                .Include(i => i.Document)
                .Include(i => i.ApprovalRecords)
                .Where(i => i.Status == "InProgress")
                .ToListAsync();

            var pendingInstances = new List<WorkflowInstance>();

            foreach (var instance in instances)
            {
                var currentLevel = instance.Template?.Levels
                    .FirstOrDefault(l => l.Order == instance.CurrentLevelOrder);

                if (currentLevel == null)
                    continue;

                // Kiểm tra user có trong scope của level không
                if (await IsUserInLevelScopeAsync(userId, currentLevel))
                {
                    // Kiểm tra user chưa approve
                    var hasApproved = instance.ApprovalRecords
                        .Any(r => r.LevelOrder == instance.CurrentLevelOrder && r.ApproverUserId == userId);

                    if (!hasApproved)
                    {
                        pendingInstances.Add(instance);
                    }
                }
            }

            var responses = new List<WorkflowInstanceResponse>();
            foreach (var instance in pendingInstances)
            {
                responses.Add(await MapToResponseAsync(instance));
            }

            return responses;
        }

        // Helper methods for validation

        private async Task ValidateDocumentFileTypeAsync(Document document, WorkflowLevel level)
        {
            if (string.IsNullOrEmpty(level.AllowedFileTypesJson))
            {
                // Policy: deny-by-default nếu không có AllowedFileTypes
                throw new UnauthorizedAccessException("Level này không cho phép duyệt bất kỳ loại file nào");
            }

            var allowedFileTypes = JsonSerializer.Deserialize<List<string>>(level.AllowedFileTypesJson) ?? new List<string>();

            if (allowedFileTypes.Count == 0)
            {
                throw new UnauthorizedAccessException("Level này không cho phép duyệt bất kỳ loại file nào");
            }

            var docMime = document.MimeType ?? "";
            var docExt = Path.GetExtension(document.Filename);

            // Kiểm tra mime type hoặc extension có trong allowed list không
            bool isAllowed = allowedFileTypes.Any(allowed =>
                allowed.Equals(docMime, StringComparison.OrdinalIgnoreCase) ||
                allowed.Equals(docExt, StringComparison.OrdinalIgnoreCase));

            if (!isAllowed)
            {
                throw new UnauthorizedAccessException(
                    $"File type '{docMime}' hoặc extension '{docExt}' không được phép ở level này");
            }
        }

        private async Task ValidateUserFileTypePermissionAsync(Guid userId, Document document)
        {
            var docMime = document.MimeType ?? "";
            var docExt = Path.GetExtension(document.Filename);

            // Tìm FileType matching với document
            var fileType = await _fileTypeService.FindByMimeOrExtensionAsync(docMime, docExt);

            if (fileType == null)
            {
                throw new UnauthorizedAccessException(
                    $"File type không được nhận diện trong hệ thống: mime={docMime}, ext={docExt}");
            }

            // Kiểm tra user có permission cho file type này không
            var hasPermission = await _permissionService.HasPermissionAsync(userId, fileType.Id);

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException(
                    $"User không có quyền duyệt file type '{fileType.Name}'");
            }
        }

        private async Task ValidateUserInLevelScopeAsync(Guid userId, WorkflowLevel level)
        {
            if (!await IsUserInLevelScopeAsync(userId, level))
            {
                throw new UnauthorizedAccessException("User không có quyền approve ở level này");
            }
        }

        private async Task<bool> IsUserInLevelScopeAsync(Guid userId, WorkflowLevel level)
        {
            if (level.ApproverType == "Users")
            {
                if (string.IsNullOrEmpty(level.UserIdsJson))
                    return false;

                var userIds = JsonSerializer.Deserialize<List<Guid>>(level.UserIdsJson) ?? new List<Guid>();
                return userIds.Contains(userId);
            }
            else if (level.ApproverType == "Department")
            {
                // TODO: Implement department check
                // Cần table Users với DepartmentId để kiểm tra
                // Tạm thời return true cho demo
                return true;
            }

            return false;
        }

        private async Task<WorkflowInstanceResponse> MapToResponseAsync(WorkflowInstance instance)
        {
            // Reload nếu cần
            if (instance.Template == null)
            {
                await _context.Entry(instance).Reference(i => i.Template).LoadAsync();
            }
            if (instance.Document == null)
            {
                await _context.Entry(instance).Reference(i => i.Document).LoadAsync();
            }
            if (!instance.ApprovalRecords.Any())
            {
                await _context.Entry(instance).Collection(i => i.ApprovalRecords).LoadAsync();
            }

            return new WorkflowInstanceResponse
            {
                Id = instance.Id,
                TemplateId = instance.TemplateId,
                TemplateName = instance.Template?.Name ?? "",
                DocumentId = instance.DocumentId,
                DocumentFilename = instance.Document?.Filename ?? "",
                CurrentLevelOrder = instance.CurrentLevelOrder,
                Status = instance.Status,
                RequestedBy = instance.RequestedBy,
                RequestedAt = instance.RequestedAt,
                ClosedAt = instance.ClosedAt,
                ApprovalRecords = instance.ApprovalRecords.Select(r => new ApprovalRecordResponse
                {
                    Id = r.Id,
                    LevelOrder = r.LevelOrder,
                    ApproverUserId = r.ApproverUserId,
                    Approved = r.Approved,
                    Comment = r.Comment,
                    SignedAt = r.SignedAt
                }).ToList()
            };
        }
    }
}
