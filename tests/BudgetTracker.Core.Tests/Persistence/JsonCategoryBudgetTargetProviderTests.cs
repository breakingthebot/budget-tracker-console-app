// Persistence/JsonCategoryBudgetTargetProviderTests.cs
// Verifies JSON-backed category target loading and validation behavior.
// Connects to: Core/Persistence/JsonCategoryBudgetTargetProvider.cs, Core/Models/CategoryBudgetTarget.cs
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;
using BudgetTracker.Core.Persistence;

namespace BudgetTracker.Core.Tests.Persistence;

/// <summary>
/// Tests the JSON-backed category target provider.
/// </summary>
[TestClass]
public sealed class JsonCategoryBudgetTargetProviderTests
{
    /// <summary>
    /// Loads valid targets from a JSON file.
    /// </summary>
    [TestMethod]
    public void LoadTargets_WithValidJson_ReturnsConfiguredTargets()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(
            filePath,
            """
            [
              { "category": "Food", "monthlyTarget": 450 },
              { "category": "Utilities", "monthlyTarget": 200 }
            ]
            """);

        var provider = CreateProvider(filePath);

        var targets = provider.LoadTargets();

        Assert.AreEqual(2, targets.Count);
        Assert.AreEqual(BudgetCategory.Food, targets[0].Category);
        Assert.AreEqual(450m, targets[0].MonthlyTarget);
    }

    /// <summary>
    /// Returns an empty list when the file is missing.
    /// </summary>
    [TestMethod]
    public void LoadTargets_WithMissingFile_ReturnsEmptyList()
    {
        var provider = CreateProvider(CreateTempFilePath());

        var targets = provider.LoadTargets();

        Assert.AreEqual(0, targets.Count);
    }

    /// <summary>
    /// Rejects duplicate configured categories.
    /// </summary>
    [TestMethod]
    public void LoadTargets_WithDuplicateCategories_ThrowsInvalidOperationException()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(
            filePath,
            """
            [
              { "category": "Food", "monthlyTarget": 450 },
              { "category": "Food", "monthlyTarget": 500 }
            ]
            """);

        var provider = CreateProvider(filePath);

        Assert.Throws<InvalidOperationException>(() => provider.LoadTargets());
    }

    /// <summary>
    /// Creates a provider for a test file path.
    /// </summary>
    /// <param name="filePath">The file path to load.</param>
    /// <returns>A configured target provider.</returns>
    private static JsonCategoryBudgetTargetProvider CreateProvider(string filePath)
    {
        return new JsonCategoryBudgetTargetProvider(filePath, new StructuredConsoleLogger());
    }

    /// <summary>
    /// Creates an isolated file path for target provider tests.
    /// </summary>
    /// <returns>A unique JSON file path inside the temp directory.</returns>
    private static string CreateTempFilePath()
    {
        return Path.Combine(Path.GetTempPath(), "BudgetTrackerTargetTests", Guid.NewGuid().ToString("N"), "targets.json");
    }
}
