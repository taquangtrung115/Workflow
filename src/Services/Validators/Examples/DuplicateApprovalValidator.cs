using System.Linq;
using System.Threading.Tasks;

namespace Workflow.Services.Validators.Examples
{
    /// <summary>
    /// Example: Prevents the same user from approving multiple levels.
    /// 
    /// Usage:
    /// Register in Program.cs validation pipeline:
    /// pipeline.AddValidator(new DuplicateApprovalValidator());
    /// 
    /// This ensures that the same person doesn't approve at multiple levels,
    /// which could be a conflict of interest or segregation of duties violation.
    /// 
    /// Useful for:
    /// - Financial approvals (separation of duties)
    /// - Compliance requirements
    /// - Fraud prevention
    /// </summary>
    public class DuplicateApprovalValidator : IWorkflowValidator
    {
        public string Name => "DuplicateApproval";

        public Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
        {
            // Check if user has already approved at a previous level
            var hasApprovedBefore = context.Instance.ApprovalRecords
                .Any(r => r.LevelOrder < context.CurrentLevel.Order 
                       && r.ApproverUserId == context.ApproverId 
                       && r.Approved);

            if (hasApprovedBefore)
            {
                return Task.FromResult(ValidationResult.Failure(
                    "User đã approve ở level trước đó. Không thể approve nhiều lần trong cùng workflow."));
            }

            return Task.FromResult(ValidationResult.Success());
        }
    }
}
