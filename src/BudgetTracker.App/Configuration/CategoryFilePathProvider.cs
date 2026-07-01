// Configuration/CategoryFilePathProvider.cs
// Resolves the checked-in JSON file path used for configured budget categories.
// Connects to: Program.cs, Core/Persistence/JsonCategoryDefinitionProvider.cs
// Created: 2026-07-01

namespace BudgetTracker.App.Configuration;

/// <summary>
/// Resolves the configuration file path for budget categories.
/// </summary>
public static class CategoryFilePathProvider
{
    private const string RelativeCategoryPath = "src\\BudgetTracker.App\\Configuration\\categories.json";

    /// <summary>
    /// Returns the full path to the category configuration file.
    /// </summary>
    /// <returns>The full path to the JSON category file.</returns>
    public static string GetCategoryFilePath()
    {
        return Path.Combine(Environment.CurrentDirectory, RelativeCategoryPath);
    }
}
