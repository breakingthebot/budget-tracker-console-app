// Abstractions/ICategoryBudgetTargetHistoryStore.cs
// Defines the contract for loading and saving budget-target audit history.
// Connects to: Services/BudgetTrackerService.cs, Persistence/JsonCategoryBudgetTargetHistoryStore.cs
// Created: 2026-07-01

using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Abstractions;

/// <summary>
/// Loads and saves persisted budget-target history entries.
/// </summary>
public interface ICategoryBudgetTargetHistoryStore
{
    /// <summary>
    /// Loads the persisted target-change history.
    /// </summary>
    /// <returns>The saved history entries.</returns>
    IReadOnlyList<CategoryBudgetTargetHistoryEntry> LoadHistory();

    /// <summary>
    /// Saves the target-change history snapshot.
    /// </summary>
    /// <param name="entries">The entries to persist.</param>
    void SaveHistory(IReadOnlyList<CategoryBudgetTargetHistoryEntry> entries);
}
