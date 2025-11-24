using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Workflow.Models;

namespace Workflow.Services.Strategies.Examples
{
    /// <summary>
    /// Example: Conditional approver strategy based on document properties.
    /// Selects different approvers based on conditions like document amount, type, etc.
    /// 
    /// Usage scenario:
    /// - Amounts < $1000: Department head approves
    /// - Amounts >= $1000: Director approves
    /// 
    /// To use:
    /// 1. Store condition rules in level configuration
    /// 2. Register in Program.cs
    /// 3. Define conditions in template:
    ///    {
    ///      "approverType": "Conditional",
    ///      "conditionsJson": "[
    ///        {\"field\": \"amount\", \"operator\": \"<\", \"value\": 1000, \"approvers\": [\"user1\"]},
    ///        {\"field\": \"amount\", \"operator\": \">=\", \"value\": 1000, \"approvers\": [\"user2\"]}
    ///      ]"
    ///    }
    /// </summary>
    public class ConditionalApproverStrategy : IApproverStrategy
    {
        public string ApproverType => "Conditional";

        public Task<bool> IsUserInScopeAsync(Guid userId, WorkflowLevel level)
        {
            // TODO: Implement conditional logic
            // 1. Parse conditions from level configuration
            // 2. Get document metadata (might need to pass instance or document)
            // 3. Evaluate conditions
            // 4. Return appropriate approvers based on matched condition
            
            // Example:
            // var conditions = JsonSerializer.Deserialize<List<ApprovalCondition>>(level.ConditionsJson);
            // var document = await GetDocumentForLevel(level);
            // 
            // foreach (var condition in conditions)
            // {
            //     if (EvaluateCondition(document, condition))
            //     {
            //         return condition.Approvers.Contains(userId);
            //     }
            // }

            return Task.FromResult(false);
        }

        public Task<List<Guid>> GetApproverUserIdsAsync(WorkflowLevel level)
        {
            // Return all possible approvers from all conditions
            // In practice, you'd evaluate conditions to return the right set
            return Task.FromResult(new List<Guid>());
        }
    }

    // Example condition model (not included in main code - just for reference)
    /*
    public class ApprovalCondition
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
        public List<Guid> Approvers { get; set; }
    }
    */
}
