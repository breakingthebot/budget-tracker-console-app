// Abstractions/IBudgetEntryStore.cs
// Defines the contract for loading and saving tracked budget entries.
// Connects to: Services/BudgetTrackerService.cs, Persistence/JsonBudgetEntryStore.cs
// Created: 2026-07-01

using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Abstractions;

/// <summary>
/// Loads and saves budget entries for the tracker.
/// </summary>
public interface IBudgetEntryStore
{
    /// <summary>
    /// Loads all stored budget entries.
    /// </summary>
    /// <returns>The stored entries, or an empty list when no data exists.</returns>
    IReadOnlyList<BudgetEntry> LoadEntries();

    /// <summary>
    /// Persists the provided entries to storage.
    /// </summary>
    /// <param name="entries">The entries to save.</param>
    void SaveEntries(IReadOnlyList<BudgetEntry> entries);
}
