// Persistence/JsonBudgetEntryStoreTests.cs
// Verifies JSON storage reads, writes, and invalid-file handling.
// Connects to: Core/Persistence/JsonBudgetEntryStore.cs, Core/Models/BudgetEntry.cs
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;
using BudgetTracker.Core.Persistence;

namespace BudgetTracker.Core.Tests.Persistence;

/// <summary>
/// Tests the JSON-backed budget entry store.
/// </summary>
[TestClass]
public sealed class JsonBudgetEntryStoreTests
{
    /// <summary>
    /// Persists and reloads entries from disk.
    /// </summary>
    [TestMethod]
    public void SaveEntries_ThenLoadEntries_RoundTripsEntries()
    {
        var filePath = CreateTempFilePath();
        var store = CreateStore(filePath);
        var entries = new List<BudgetEntry>
        {
            new(new DateOnly(2026, 7, 1), BudgetCategory.Food, "Groceries", 25.75m),
            new(new DateOnly(2026, 7, 2), BudgetCategory.Transportation, "Train pass", 12.00m)
        };

        store.SaveEntries(entries);

        var loadedEntries = store.LoadEntries();

        Assert.AreEqual(2, loadedEntries.Count);
        Assert.AreEqual("Groceries", loadedEntries[0].Description);
        Assert.AreEqual(12.00m, loadedEntries[1].Amount);
    }

    /// <summary>
    /// Returns an empty list when no file exists yet.
    /// </summary>
    [TestMethod]
    public void LoadEntries_WithMissingFile_ReturnsEmptyList()
    {
        var filePath = CreateTempFilePath();
        var store = CreateStore(filePath);

        var loadedEntries = store.LoadEntries();

        Assert.AreEqual(0, loadedEntries.Count);
    }

    /// <summary>
    /// Rejects invalid JSON content with a clear exception.
    /// </summary>
    [TestMethod]
    public void LoadEntries_WithInvalidJson_ThrowsInvalidOperationException()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "{ this is invalid json");
        var store = CreateStore(filePath);

        Assert.Throws<InvalidOperationException>(() => store.LoadEntries());
    }

    /// <summary>
    /// Creates a store instance for a temp file path.
    /// </summary>
    /// <param name="filePath">The path to use for the test file.</param>
    /// <returns>A configured JSON entry store.</returns>
    private static JsonBudgetEntryStore CreateStore(string filePath)
    {
        return new JsonBudgetEntryStore(filePath, new StructuredConsoleLogger());
    }

    /// <summary>
    /// Creates an isolated file path for a test run.
    /// </summary>
    /// <returns>A unique JSON file path inside the temp directory.</returns>
    private static string CreateTempFilePath()
    {
        return Path.Combine(Path.GetTempPath(), "BudgetTrackerTests", Guid.NewGuid().ToString("N"), "entries.json");
    }
}
