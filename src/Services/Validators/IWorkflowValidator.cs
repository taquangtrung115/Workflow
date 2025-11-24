using System;
using System.Threading.Tasks;
using Workflow.Models;

namespace Workflow.Services.Validators
{
    /// <summary>
    /// Interface for workflow validation logic.
    /// Allows composable validation rules.
    /// </summary>
    public interface IWorkflowValidator
    {
        /// <summary>
        /// Gets the name of this validator (for error messages)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Validates if an approval can proceed
        /// </summary>
        /// <param name="context">Validation context with all necessary data</param>
        /// <returns>Validation result</returns>
        Task<ValidationResult> ValidateAsync(WorkflowValidationContext context);
    }

    /// <summary>
    /// Result of a validation check
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        public static ValidationResult Failure(string message) => new ValidationResult { IsValid = false, ErrorMessage = message };
    }

    /// <summary>
    /// Context containing all data needed for validation
    /// </summary>
    public class WorkflowValidationContext
    {
        public required WorkflowInstance Instance { get; init; }
        public required WorkflowLevel CurrentLevel { get; init; }
        public required Document Document { get; init; }
        public required Guid ApproverId { get; init; }
    }
}
