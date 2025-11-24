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
            _strategies = new Dictionary<string, IApproverStrategy>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var strategy in strategies)
            {
                if (_strategies.ContainsKey(strategy.ApproverType))
                {
                    // Log warning or throw exception - for now, last one wins
                    // In production, consider logging this conflict
                    _strategies[strategy.ApproverType] = strategy;
                }
                else
                {
                    _strategies.Add(strategy.ApproverType, strategy);
                }
            }
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
