using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Workflow.Models;

namespace Workflow.Services.Strategies
{
    /// <summary>
    /// Strategy for direct user list approver type.
    /// Users are specified explicitly in the level configuration.
    /// </summary>
    public class UsersApproverStrategy : IApproverStrategy
    {
        public string ApproverType => "Users";

        public Task<bool> IsUserInScopeAsync(Guid userId, WorkflowLevel level)
        {
            if (string.IsNullOrEmpty(level.UserIdsJson))
                return Task.FromResult(false);

            var userIds = JsonSerializer.Deserialize<List<Guid>>(level.UserIdsJson) ?? new List<Guid>();
            return Task.FromResult(userIds.Contains(userId));
        }

        public Task<List<Guid>> GetApproverUserIdsAsync(WorkflowLevel level)
        {
            if (string.IsNullOrEmpty(level.UserIdsJson))
                return Task.FromResult(new List<Guid>());

            var userIds = JsonSerializer.Deserialize<List<Guid>>(level.UserIdsJson) ?? new List<Guid>();
            return Task.FromResult(userIds);
        }
    }
}
