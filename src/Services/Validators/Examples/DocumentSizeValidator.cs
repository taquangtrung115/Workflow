using System.Threading.Tasks;

namespace Workflow.Services.Validators.Examples
{
    /// <summary>
    /// Example: Validates document size limits per level.
    /// 
    /// Usage:
    /// 1. Add MaxFileSizeBytes field to WorkflowLevel model
    /// 2. Register in Program.cs validation pipeline:
    ///    pipeline.AddValidator(new DocumentSizeValidator());
    /// 3. Configure in template:
    ///    {
    ///      "order": 1,
    ///      "maxFileSizeBytes": 5242880, // 5MB
    ///      ...
    ///    }
    /// 
    /// This allows different levels to have different size restrictions.
    /// For example:
    /// - Level 1 (dept approval): Max 5MB
    /// - Level 2 (director approval): Max 50MB
    /// </summary>
    public class DocumentSizeValidator : IWorkflowValidator
    {
        private const long DefaultMaxSize = 10 * 1024 * 1024; // 10MB default

        public string Name => "DocumentSize";

        public Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
        {
            // TODO: Add FileSize field to Document model
            // var fileSize = context.Document.FileSize;
            
            // TODO: Add MaxFileSizeBytes field to WorkflowLevel model
            // var maxSize = context.CurrentLevel.MaxFileSizeBytes ?? DefaultMaxSize;

            // if (fileSize > maxSize)
            // {
            //     var maxSizeMB = maxSize / (1024 * 1024);
            //     var fileSizeMB = fileSize / (1024 * 1024);
            //     return ValidationResult.Failure(
            //         $"File size ({fileSizeMB:F2}MB) vượt quá giới hạn cho phép ({maxSizeMB:F2}MB) ở level này");
            // }

            // Placeholder - always passes
            return Task.FromResult(ValidationResult.Success());
        }
    }
}
