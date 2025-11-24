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
            // EXAMPLE ONLY - NOT FULLY IMPLEMENTED
            // To use this validator:
            // 1. Add Amount field to Document model
            // 2. Add approval limit configuration (per-level, per-user, or per-role)
            // 3. Implement the validation logic below
            
            // Example implementation:
            // var documentAmount = context.Document.Amount;
            // var userApprovalLimit = await GetUserApprovalLimit(context.ApproverId);
            // 
            // if (documentAmount > userApprovalLimit)
            // {
            //     return ValidationResult.Failure(
            //         $"Document amount ${documentAmount:N2} vượt quá quyền hạn duyệt của user (${userApprovalLimit:N2})");
            // }

            // NOTE: Currently returns success to not block workflow
            // In production, implement properly or remove this validator
            return Task.FromResult(ValidationResult.Success());
        }
    }
}
