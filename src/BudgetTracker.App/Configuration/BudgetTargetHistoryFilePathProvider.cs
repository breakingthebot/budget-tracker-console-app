// Configuration/BudgetTargetHistoryFilePathProvider.cs
// Resolves the local JSON file path used for persisted budget-target history.
// Connects to: Program.cs, Core/Persistence/JsonCategoryBudgetTargetHistoryStore.cs
// Created: 2026-07-01

namespace BudgetTracker.App.Configuration;

/// <summary>
/// Resolves data file paths for budget-target history.
/// </summary>
public static class BudgetTargetHistoryFilePathProvider
{
    private const string DataDirectoryName = "data";
    private const string BudgetTargetHistoryFileName = "budget-target-history.json";

    /// <summary>
    /// Returns the budget-target history file path rooted at the current working directory.
    /// </summary>
    /// <returns>The full path to the JSON history file.</returns>
    public static string GetBudgetTargetHistoryFilePath()
    {
        return Path.Combine(Environment.CurrentDirectory, DataDirectoryName, BudgetTargetHistoryFileName);
    }
}
