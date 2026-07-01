// Abstractions/ICategoryBudgetTargetProvider.cs
// Defines the contract for retrieving configured monthly category targets.
// Connects to: Services/BudgetTrackerService.cs, Persistence/JsonCategoryBudgetTargetProvider.cs
// Created: 2026-07-01

using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Abstractions;

/// <summary>
/// Retrieves configured monthly targets for budget categories.
/// </summary>
public interface ICategoryBudgetTargetProvider
{
    /// <summary>
    /// Loads the configured monthly targets.
    /// </summary>
    /// <returns>The configured category targets.</returns>
    IReadOnlyList<CategoryBudgetTarget> LoadTargets();
}
