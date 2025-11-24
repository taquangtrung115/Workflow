using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Workflow.Services.Validators
{
    /// <summary>
    /// Validates that the document file type is allowed at the current level
    /// </summary>
    public class FileTypeValidator : IWorkflowValidator
    {
        public string Name => "FileType";

        public Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
        {
            if (string.IsNullOrEmpty(context.CurrentLevel.AllowedFileTypesJson))
            {
                // Policy: deny-by-default if no AllowedFileTypes specified
                return Task.FromResult(ValidationResult.Failure(
                    "Level này không cho phép duyệt bất kỳ loại file nào"));
            }

            var allowedFileTypes = JsonSerializer.Deserialize<List<string>>(
                context.CurrentLevel.AllowedFileTypesJson) ?? new List<string>();

            if (allowedFileTypes.Count == 0)
            {
                return Task.FromResult(ValidationResult.Failure(
                    "Level này không cho phép duyệt bất kỳ loại file nào"));
            }

            var docMime = context.Document.MimeType ?? "";
            var docExt = Path.GetExtension(context.Document.Filename);

            // Check if mime type or extension is in allowed list
            bool isAllowed = allowedFileTypes.Any(allowed =>
                allowed.Equals(docMime, StringComparison.OrdinalIgnoreCase) ||
                allowed.Equals(docExt, StringComparison.OrdinalIgnoreCase));

            if (!isAllowed)
            {
                return Task.FromResult(ValidationResult.Failure(
                    $"File type '{docMime}' hoặc extension '{docExt}' không được phép ở level này"));
            }

            return Task.FromResult(ValidationResult.Success());
        }
    }
}
