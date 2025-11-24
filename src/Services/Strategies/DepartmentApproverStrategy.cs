using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workflow.Models;

namespace Workflow.Services.Strategies
{
    /// <summary>
    /// Strategy for department-based approver type.
    /// Note: Full implementation requires a Users table with DepartmentId.
    /// Currently returns false for security - implement proper User-Department relationship.
    /// </summary>
    public class DepartmentApproverStrategy : IApproverStrategy
    {
        public string ApproverType => "Department";

        public Task<bool> IsUserInScopeAsync(Guid userId, WorkflowLevel level)
        {
            // TODO: Implement proper User-Department relationship
            // Current implementation: deny by default for security
            // To enable: check if user.DepartmentId == level.DepartmentId
            // Example:
            // var user = await _context.Users.FindAsync(userId);
            // return user?.DepartmentId == level.DepartmentId;
            
            return Task.FromResult(false);
        }

        public Task<List<Guid>> GetApproverUserIdsAsync(WorkflowLevel level)
        {
            // TODO: Implement proper User-Department relationship
            // Should query all users in the department
            // Example:
            // var users = await _context.Users
            //     .Where(u => u.DepartmentId == level.DepartmentId)
            //     .Select(u => u.Id)
            //     .ToListAsync();
            
            return Task.FromResult(new List<Guid>());
        }
    }
}
