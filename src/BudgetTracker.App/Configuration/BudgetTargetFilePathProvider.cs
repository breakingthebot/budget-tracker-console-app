// Configuration/BudgetTargetFilePathProvider.cs
// Resolves the checked-in JSON file path used for monthly category targets.
// Connects to: Program.cs, Core/Persistence/JsonCategoryBudgetTargetProvider.cs
// Created: 2026-07-01

namespace BudgetTracker.App.Configuration;

/// <summary>
/// Resolves the configuration file path for category targets.
/// </summary>
public static class BudgetTargetFilePathProvider
{
    private const string RelativeBudgetTargetPath = "src\\BudgetTracker.App\\Configuration\\budget-targets.json";

    /// <summary>
    /// Returns the full path to the category target configuration file.
    /// </summary>
    /// <returns>The full path to the JSON target file.</returns>
    public static string GetBudgetTargetFilePath()
    {
        return Path.Combine(Environment.CurrentDirectory, RelativeBudgetTargetPath);
    }
}
