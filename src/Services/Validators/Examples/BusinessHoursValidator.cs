using System;
using System.Threading.Tasks;

namespace Workflow.Services.Validators.Examples
{
    /// <summary>
    /// Example: Validates that approvals only happen during business hours.
    /// 
    /// Usage:
    /// Register in Program.cs validation pipeline:
    /// pipeline.AddValidator(new BusinessHoursValidator());
    /// 
    /// This ensures approvals can only be made during designated hours,
    /// preventing after-hours approvals unless explicitly allowed.
    /// 
    /// Can be enhanced to:
    /// - Check holidays
    /// - Different hours for different departments
    /// - Emergency override mechanism
    /// </summary>
    public class BusinessHoursValidator : IWorkflowValidator
    {
        private readonly TimeSpan _startTime = new TimeSpan(8, 0, 0);   // 8:00 AM
        private readonly TimeSpan _endTime = new TimeSpan(18, 0, 0);    // 6:00 PM

        public string Name => "BusinessHours";

        public Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
        {
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            // Check if weekend
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                return Task.FromResult(ValidationResult.Failure(
                    "Approvals không được phép vào cuối tuần"));
            }

            // Check if within business hours
            if (currentTime < _startTime || currentTime > _endTime)
            {
                return Task.FromResult(ValidationResult.Failure(
                    $"Approvals chỉ được phép từ {_startTime.ToString(@"hh\:mm")} đến {_endTime.ToString(@"hh\:mm")}"));
            }

            return Task.FromResult(ValidationResult.Success());
        }
    }
}
