// Persistence/JsonCategoryDefinitionProviderTests.cs
// Verifies JSON-backed category loading and validation behavior.
// Connects to: Core/Persistence/JsonCategoryDefinitionProvider.cs, Core/Models/CategoryDefinition.cs
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Persistence;

namespace BudgetTracker.Core.Tests.Persistence;

/// <summary>
/// Tests the JSON-backed category definition provider.
/// </summary>
[TestClass]
public sealed class JsonCategoryDefinitionProviderTests
{
    /// <summary>
    /// Loads valid configured categories in display order.
    /// </summary>
    [TestMethod]
    public void LoadCategories_WithValidJson_ReturnsConfiguredCategories()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(
            filePath,
            """
            [
              { "name": "Food", "displayOrder": 2 },
              { "name": "Housing", "displayOrder": 1 }
            ]
            """);

        var provider = CreateProvider(filePath);

        var categories = provider.LoadCategories();

        Assert.AreEqual(2, categories.Count);
        Assert.AreEqual("Housing", categories[0].Name);
        Assert.AreEqual("Food", categories[1].Name);
    }

    /// <summary>
    /// Rejects duplicate category names.
    /// </summary>
    [TestMethod]
    public void LoadCategories_WithDuplicateNames_ThrowsInvalidOperationException()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(
            filePath,
            """
            [
              { "name": "Food", "displayOrder": 1 },
              { "name": "food", "displayOrder": 2 }
            ]
            """);

        var provider = CreateProvider(filePath);

        Assert.Throws<InvalidOperationException>(() => provider.LoadCategories());
    }

    /// <summary>
    /// Creates a category provider for a test file path.
    /// </summary>
    /// <param name="filePath">The file path to load.</param>
    /// <returns>A configured provider.</returns>
    private static JsonCategoryDefinitionProvider CreateProvider(string filePath)
    {
        return new JsonCategoryDefinitionProvider(filePath, new StructuredConsoleLogger());
    }

    /// <summary>
    /// Creates an isolated category config file path.
    /// </summary>
    /// <returns>A unique JSON file path inside the temp directory.</returns>
    private static string CreateTempFilePath()
    {
        return Path.Combine(Path.GetTempPath(), "BudgetTrackerCategoryTests", Guid.NewGuid().ToString("N"), "categories.json");
    }
}
