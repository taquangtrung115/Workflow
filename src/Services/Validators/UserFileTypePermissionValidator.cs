using System;
using System.IO;
using System.Threading.Tasks;

namespace Workflow.Services.Validators
{
    /// <summary>
    /// Validates that the user has permission to approve the file type
    /// </summary>
    public class UserFileTypePermissionValidator : IWorkflowValidator
    {
        private readonly IFileTypeService _fileTypeService;
        private readonly IPermissionService _permissionService;

        public string Name => "UserFileTypePermission";

        public UserFileTypePermissionValidator(
            IFileTypeService fileTypeService,
            IPermissionService permissionService)
        {
            _fileTypeService = fileTypeService;
            _permissionService = permissionService;
        }

        public async Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
        {
            var docMime = context.Document.MimeType ?? "";
            var docExt = Path.GetExtension(context.Document.Filename);

            // Find FileType matching the document
            var fileType = await _fileTypeService.FindByMimeOrExtensionAsync(docMime, docExt);

            if (fileType == null)
            {
                return ValidationResult.Failure(
                    $"File type không được nhận diện trong hệ thống: mime={docMime}, ext={docExt}");
            }

            // Check if user has permission for this file type
            var hasPermission = await _permissionService.HasPermissionAsync(
                context.ApproverId, fileType.Id);

            if (!hasPermission)
            {
                return ValidationResult.Failure(
                    $"User không có quyền duyệt file type '{fileType.Name}'");
            }

            return ValidationResult.Success();
        }
    }
}
