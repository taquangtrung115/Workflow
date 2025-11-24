using System;
using System.Threading.Tasks;
using Workflow.Services.Strategies;

namespace Workflow.Services.Validators
{
    /// <summary>
    /// Validates that the user is in the approver scope for the current level
    /// </summary>
    public class ApproverScopeValidator : IWorkflowValidator
    {
        private readonly ApproverStrategyFactory _strategyFactory;

        public string Name => "ApproverScope";

        public ApproverScopeValidator(ApproverStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        public async Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
        {
            var strategy = _strategyFactory.GetStrategy(context.CurrentLevel.ApproverType);
            
            if (strategy == null)
            {
                return ValidationResult.Failure(
                    $"Approver type '{context.CurrentLevel.ApproverType}' không được hỗ trợ");
            }

            var isInScope = await strategy.IsUserInScopeAsync(
                context.ApproverId, context.CurrentLevel);

            if (!isInScope)
            {
                return ValidationResult.Failure(
                    "User không có quyền approve ở level này");
            }

            return ValidationResult.Success();
        }
    }
}
