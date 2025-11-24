using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workflow.Models;

namespace Workflow.Services.Strategies
{
    /// <summary>
    /// Interface for approver resolution strategies.
    /// Allows extensible approver type implementations.
    /// </summary>
    public interface IApproverStrategy
    {
        /// <summary>
        /// Gets the approver type this strategy handles (e.g., "Users", "Department", "Role")
        /// </summary>
        string ApproverType { get; }

        /// <summary>
        /// Checks if a user is in the scope of approvers for this level
        /// </summary>
        Task<bool> IsUserInScopeAsync(Guid userId, WorkflowLevel level);

        /// <summary>
        /// Gets all user IDs that can approve at this level (for notifications)
        /// </summary>
        Task<List<Guid>> GetApproverUserIdsAsync(WorkflowLevel level);
    }
}
