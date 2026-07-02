// Persistence/JsonCategoryBudgetTargetHistoryStoreTests.cs
// Verifies JSON-backed budget-target history loading, saving, and validation behavior.
// Connects to: Core/Persistence/JsonCategoryBudgetTargetHistoryStore.cs, Core/Models/CategoryBudgetTargetHistoryEntry.cs
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;
using BudgetTracker.Core.Persistence;

namespace BudgetTracker.Core.Tests.Persistence;

/// <summary>
/// Tests the JSON-backed budget-target history store.
/// </summary>
[TestClass]
public sealed class JsonCategoryBudgetTargetHistoryStoreTests
{
    /// <summary>
    /// Saves history entries and loads them back.
    /// </summary>
    [TestMethod]
    public void SaveHistory_WithValidEntries_WritesReadableJson()
    {
        var filePath = CreateTempFilePath();
        var store = CreateStore(filePath);

        store.SaveHistory(
        [
            new CategoryBudgetTargetHistoryEntry(
                new DateTimeOffset(2026, 7, 1, 12, 0, 0, TimeSpan.Zero),
                "Food",
                450m,
                500m,
                BudgetTargetEvaluationModes.MaxSpend),
            new CategoryBudgetTargetHistoryEntry(
                new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero),
                "Savings",
                600m,
                700m,
                BudgetTargetEvaluationModes.MinProgress)
        ]);

        var historyEntries = store.LoadHistory();

        Assert.AreEqual(2, historyEntries.Count);
        Assert.AreEqual("Food", historyEntries[0].Category);
        Assert.AreEqual(700m, historyEntries[1].UpdatedMonthlyTarget);
    }

    /// <summary>
    /// Returns an empty list when the history file is missing.
    /// </summary>
    [TestMethod]
    public void LoadHistory_WithMissingFile_ReturnsEmptyList()
    {
        var store = CreateStore(CreateTempFilePath());

        var historyEntries = store.LoadHistory();

        Assert.AreEqual(0, historyEntries.Count);
    }

    /// <summary>
    /// Rejects invalid evaluation modes in persisted history.
    /// </summary>
    [TestMethod]
    public void LoadHistory_WithInvalidEvaluationMode_ThrowsInvalidOperationException()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(
            filePath,
            """
            [
              {
                "changedAtUtc": "2026-07-01T12:00:00+00:00",
                "category": "Savings",
                "previousMonthlyTarget": 600,
                "updatedMonthlyTarget": 700,
                "evaluationMode": "unknown-mode"
              }
            ]
            """);

        var store = CreateStore(filePath);

        Assert.Throws<InvalidOperationException>(() => store.LoadHistory());
    }

    /// <summary>
    /// Creates a store for a test file path.
    /// </summary>
    /// <param name="filePath">The file path to use.</param>
    /// <returns>A configured history store.</returns>
    private static JsonCategoryBudgetTargetHistoryStore CreateStore(string filePath)
    {
        return new JsonCategoryBudgetTargetHistoryStore(filePath, new StructuredConsoleLogger());
    }

    /// <summary>
    /// Creates an isolated file path for history store tests.
    /// </summary>
    /// <returns>A unique JSON history file path inside the temp directory.</returns>
    private static string CreateTempFilePath()
    {
        return Path.Combine(Path.GetTempPath(), "BudgetTrackerTargetHistoryTests", Guid.NewGuid().ToString("N"), "history.json");
    }
}
