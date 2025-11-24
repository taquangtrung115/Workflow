using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Workflow.Services.Validators
{
    /// <summary>
    /// Composable validation pipeline that runs multiple validators in sequence.
    /// Stops at the first validation failure.
    /// </summary>
    public class ValidationPipeline
    {
        private readonly List<IWorkflowValidator> _validators = new();

        /// <summary>
        /// Adds a validator to the pipeline
        /// </summary>
        public ValidationPipeline AddValidator(IWorkflowValidator validator)
        {
            _validators.Add(validator);
            return this;
        }

        /// <summary>
        /// Runs all validators in sequence, stopping at first failure
        /// </summary>
        public async Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
        {
            foreach (var validator in _validators)
            {
                var result = await validator.ValidateAsync(context);
                if (!result.IsValid)
                {
                    return result;
                }
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Gets all registered validator names
        /// </summary>
        public IEnumerable<string> GetValidatorNames()
        {
            return _validators.Select(v => v.Name);
        }
    }
}
