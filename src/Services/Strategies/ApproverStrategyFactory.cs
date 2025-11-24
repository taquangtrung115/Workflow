using System;
using System.Collections.Generic;
using System.Linq;

namespace Workflow.Services.Strategies
{
    /// <summary>
    /// Factory for resolving approver strategies.
    /// Supports extensibility by allowing registration of custom strategies.
    /// </summary>
    public class ApproverStrategyFactory
    {
        private readonly Dictionary<string, IApproverStrategy> _strategies;

        public ApproverStrategyFactory(IEnumerable<IApproverStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(
                s => s.ApproverType,
                s => s,
                StringComparer.OrdinalIgnoreCase
            );
        }

        /// <summary>
        /// Gets the strategy for the specified approver type
        /// </summary>
        public IApproverStrategy? GetStrategy(string approverType)
        {
            _strategies.TryGetValue(approverType, out var strategy);
            return strategy;
        }

        /// <summary>
        /// Registers a custom approver strategy
        /// </summary>
        public void RegisterStrategy(IApproverStrategy strategy)
        {
            _strategies[strategy.ApproverType] = strategy;
        }

        /// <summary>
        /// Gets all registered approver types
        /// </summary>
        public IEnumerable<string> GetSupportedApproverTypes()
        {
            return _strategies.Keys;
        }
    }
}
