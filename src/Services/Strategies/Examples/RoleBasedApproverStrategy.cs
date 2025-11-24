using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Workflow.Models;

namespace Workflow.Services.Strategies.Examples
{
    /// <summary>
    /// Example: Role-based approver strategy.
    /// This demonstrates how to extend the system with a new approver type.
    /// 
    /// Usage:
    /// 1. Register this strategy in Program.cs:
    ///    builder.Services.AddSingleton<IApproverStrategy, RoleBasedApproverStrategy>();
    /// 
    /// 2. Use in template level configuration:
    ///    {
    ///      "approverType": "Role",
    ///      "roleNamesJson": "[\"Manager\", \"Director\"]",
    ///      ...
    ///    }
    /// 
    /// 3. Add RoleNamesJson field to WorkflowLevel model (optional, or use existing fields)
    /// </summary>
    public class RoleBasedApproverStrategy : IApproverStrategy
    {
        // If you have a database context for Users/Roles, inject it here
        // private readonly WorkflowDbContext _context;
        
        public string ApproverType => "Role";

        public Task<bool> IsUserInScopeAsync(Guid userId, WorkflowLevel level)
        {
            // EXAMPLE ONLY - NOT FULLY IMPLEMENTED
            // To use this strategy, you must implement the logic below:
            // 
            // 1. Get user's roles from database
            // 2. Parse allowed roles from level.RoleNamesJson (or similar field)
            // 3. Check if user has any of the required roles
            
            // Example implementation:
            // var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            // var requiredRoles = JsonSerializer.Deserialize<List<string>>(level.RoleNamesJson);
            // return user.Roles.Any(r => requiredRoles.Contains(r.Name));

            throw new NotImplementedException(
                "RoleBasedApproverStrategy is an example. Implement the logic or remove this strategy.");
        }

        public Task<List<Guid>> GetApproverUserIdsAsync(WorkflowLevel level)
        {
            // EXAMPLE ONLY - NOT FULLY IMPLEMENTED
            // To use: implement logic to get all users with required roles
            // Example:
            // var requiredRoles = JsonSerializer.Deserialize<List<string>>(level.RoleNamesJson);
            // var userIds = await _context.Users
            //     .Where(u => u.Roles.Any(r => requiredRoles.Contains(r.Name)))
            //     .Select(u => u.Id)
            //     .ToListAsync();

            throw new NotImplementedException(
                "RoleBasedApproverStrategy is an example. Implement the logic or remove this strategy.");
        }
    }
}
