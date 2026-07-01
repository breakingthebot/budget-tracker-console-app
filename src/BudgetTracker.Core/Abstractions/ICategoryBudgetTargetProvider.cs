// Abstractions/ICategoryBudgetTargetProvider.cs
// Defines the contract for loading and saving configured monthly category targets.
// Connects to: Services/BudgetTrackerService.cs, Persistence/JsonCategoryBudgetTargetProvider.cs
// Created: 2026-07-01

using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Abstractions;

/// <summary>
/// Loads and saves configured monthly targets for budget categories.
/// </summary>
public interface ICategoryBudgetTargetProvider
{
    /// <summary>
    /// Loads the configured monthly targets.
    /// </summary>
    /// <returns>The configured category targets.</returns>
    IReadOnlyList<CategoryBudgetTarget> LoadTargets();

    /// <summary>
    /// Saves the configured monthly targets.
    /// </summary>
    /// <param name="targets">The targets to persist.</param>
    void SaveTargets(IReadOnlyList<CategoryBudgetTarget> targets);
}
