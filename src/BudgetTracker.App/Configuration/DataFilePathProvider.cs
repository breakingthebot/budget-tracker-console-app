// Configuration/DataFilePathProvider.cs
// Resolves the local JSON file path used for persisted budget data.
// Connects to: Program.cs, Core/Persistence/JsonBudgetEntryStore.cs
// Created: 2026-07-01

namespace BudgetTracker.App.Configuration;

/// <summary>
/// Resolves data file paths for the console application.
/// </summary>
public static class DataFilePathProvider
{
    private const string DataDirectoryName = "data";
    private const string BudgetDataFileName = "budget-entries.json";

    /// <summary>
    /// Returns the budget data file path rooted at the current working directory.
    /// </summary>
    /// <returns>The full path to the JSON budget data file.</returns>
    public static string GetBudgetDataFilePath()
    {
        return Path.Combine(Environment.CurrentDirectory, DataDirectoryName, BudgetDataFileName);
    }
}
