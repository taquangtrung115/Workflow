using System;
using System.Threading.Tasks;

namespace Workflow.Services.Validators.Examples
{
    /// <summary>
    /// Example: Validates approval based on document amount thresholds.
    /// 
    /// Usage:
    /// 1. Add Amount field to Document model
    /// 2. Add MaxApprovalAmount field to WorkflowLevel or use metadata
    /// 3. Register in Program.cs:
    ///    pipeline.AddValidator(new AmountThresholdValidator());
    /// 
    /// Scenario:
    /// - Department head can approve up to $10,000
    /// - Director can approve up to $50,000
    /// - VP required for amounts > $50,000
    /// 
    /// This is common in:
    /// - Purchase requisitions
    /// - Expense reports
    /// - Contract approvals
    /// - Financial workflows
    /// </summary>
    public class AmountThresholdValidator : IWorkflowValidator
    {
        public string Name => "AmountThreshold";

        public Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
        {
            // TODO: Add Amount field to Document model
            // var documentAmount = context.Document.Amount;
            
            // TODO: Add approval limit configuration
            // This could be:
            // 1. Per-level configuration in WorkflowLevel
            // 2. Per-user configuration in user profile
            // 3. Per-role configuration
            
            // Example implementation:
            // var userApprovalLimit = await GetUserApprovalLimit(context.ApproverId);
            // 
            // if (documentAmount > userApprovalLimit)
            // {
            //     return ValidationResult.Failure(
            //         $"Document amount ${documentAmount:N2} vượt quá quyền hạn duyệt của user (${userApprovalLimit:N2})");
            // }

            // Placeholder - always passes
            return Task.FromResult(ValidationResult.Success());
        }
    }
}
