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
            // EXAMPLE ONLY - NOT FULLY IMPLEMENTED
            // To use this validator:
            // 1. Add FileSize field to Document model
            // 2. Add MaxFileSizeBytes field to WorkflowLevel model
            // 3. Implement the validation logic below
            
            // Example implementation:
            // var fileSize = context.Document.FileSize;
            // var maxSize = context.CurrentLevel.MaxFileSizeBytes ?? DefaultMaxSize;
            // if (fileSize > maxSize)
            // {
            //     var maxSizeMB = maxSize / (1024 * 1024);
            //     var fileSizeMB = fileSize / (1024 * 1024);
            //     return ValidationResult.Failure(
            //         $"File size ({fileSizeMB:F2}MB) vượt quá giới hạn cho phép ({maxSizeMB:F2}MB) ở level này");
            // }

            // NOTE: Currently returns success to not block workflow
            // In production, implement properly or remove this validator
            return Task.FromResult(ValidationResult.Success());
        }
    }
}
