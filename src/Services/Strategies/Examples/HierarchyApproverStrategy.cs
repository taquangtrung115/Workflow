using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workflow.Models;

namespace Workflow.Services.Strategies.Examples
{
    /// <summary>
    /// Example: Hierarchical approver strategy.
    /// Approves if user is in the management hierarchy of the requester.
    /// 
    /// Usage scenario:
    /// - Level 1: Direct manager must approve
    /// - Level 2: Manager's manager must approve
    /// 
    /// To use:
    /// 1. Add ManagerId field to Users table
    /// 2. Register in Program.cs:
    ///    builder.Services.AddSingleton<IApproverStrategy, HierarchyApproverStrategy>();
    /// 3. Use in template:
    ///    {
    ///      "approverType": "DirectManager",
    ///      "hierarchyLevel": 1
    ///    }
    /// </summary>
    public class HierarchyApproverStrategy : IApproverStrategy
    {
        public string ApproverType => "DirectManager";

        public Task<bool> IsUserInScopeAsync(Guid userId, WorkflowLevel level)
        {
            // TODO: Implement hierarchical check
            // Example:
            // 1. Get the requester from workflow instance
            // 2. Get requester's manager chain
            // 3. Check if userId is in the chain at appropriate level
            
            // var instance = await _context.WorkflowInstances.FindAsync(level.TemplateId);
            // var requester = await _context.Users.Include(u => u.Manager).FirstOrDefaultAsync(u => u.Id == instance.RequestedBy);
            // var manager = requester.Manager;
            // 
            // // For level 1, check direct manager
            // if (hierarchyLevel == 1)
            //     return manager?.Id == userId;
            // 
            // // For level 2+, traverse up the hierarchy
            // for (int i = 1; i < hierarchyLevel && manager != null; i++)
            // {
            //     manager = await _context.Users.Include(u => u.Manager).FirstOrDefaultAsync(u => u.Id == manager.ManagerId);
            // }
            // return manager?.Id == userId;

            return Task.FromResult(false);
        }

        public Task<List<Guid>> GetApproverUserIdsAsync(WorkflowLevel level)
        {
            // Return list of managers at the specified hierarchy level
            // This would need access to the workflow instance to know who requested it
            return Task.FromResult(new List<Guid>());
        }
    }
}
