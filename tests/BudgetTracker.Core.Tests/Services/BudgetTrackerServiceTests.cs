// Services/BudgetTrackerServiceTests.cs
// Verifies entry validation, monthly report generation, budget warnings, and CSV export behavior.
// Connects to: Core/Services/BudgetTrackerService.cs, Core/Models/*, Core/Abstractions/*
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;
using BudgetTracker.Core.Services;
using BudgetTracker.Core.Abstractions;

namespace BudgetTracker.Core.Tests.Services;

/// <summary>
/// Tests the budget tracker service.
/// </summary>
[TestClass]
public sealed class BudgetTrackerServiceTests
{
    /// <summary>
    /// Confirms valid entries are stored and returned.
    /// </summary>
    [TestMethod]
    public void AddEntry_StoresValidEntry()
    {
        var service = CreateService();

        service.AddEntry(new BudgetEntry(new DateOnly(2026, 7, 1), BudgetCategory.Food, "Groceries", 95.25m));

        var entries = service.GetEntries();

        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual("Groceries", entries[0].Description);
    }

    /// <summary>
    /// Confirms stored entries are loaded when the service starts.
    /// </summary>
    [TestMethod]
    public void Constructor_LoadsExistingEntriesFromStore()
    {
        var existingEntries = new List<BudgetEntry>
        {
            new(new DateOnly(2026, 7, 1), BudgetCategory.Housing, "Rent", 1400.00m)
        };

        var service = CreateService(existingEntries);

        var entries = service.GetEntries();

        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual("Rent", entries[0].Description);
    }

    /// <summary>
    /// Confirms monthly reports sum totals by category.
    /// </summary>
    [TestMethod]
    public void GetMonthlyReport_ReturnsCategoryBreakdownAndTotals()
    {
        var service = CreateService(targets:
        [
            new CategoryBudgetTarget(BudgetCategory.Food, 75.00m),
            new CategoryBudgetTarget(BudgetCategory.Utilities, 100.00m)
        ]);

        service.AddEntry(new BudgetEntry(new DateOnly(2026, 7, 1), BudgetCategory.Food, "Groceries", 40.00m));
        service.AddEntry(new BudgetEntry(new DateOnly(2026, 7, 3), BudgetCategory.Food, "Lunch", 15.50m));
        service.AddEntry(new BudgetEntry(new DateOnly(2026, 7, 5), BudgetCategory.Utilities, "Electricity", 70.00m));
        service.AddEntry(new BudgetEntry(new DateOnly(2026, 8, 1), BudgetCategory.Food, "Next Month", 12.00m));

        var report = service.GetMonthlyReport(new DateOnly(2026, 7, 1));

        Assert.AreEqual(3, report.EntryCount);
        Assert.AreEqual(125.50m, report.TotalSpent);
        Assert.AreEqual(2, report.CategoryBreakdown.Count);
        Assert.AreEqual(BudgetCategory.Utilities, report.CategoryBreakdown[0].Category);
        Assert.AreEqual(70.00m, report.CategoryBreakdown[0].Total);
        Assert.AreEqual(0, report.OverBudgetCategoryCount);
    }

    /// <summary>
    /// Confirms monthly reports flag categories that exceed their targets.
    /// </summary>
    [TestMethod]
    public void GetMonthlyReport_ReturnsOverBudgetStatuses()
    {
        var service = CreateService(targets:
        [
            new CategoryBudgetTarget(BudgetCategory.Food, 50.00m),
            new CategoryBudgetTarget(BudgetCategory.Utilities, 90.00m)
        ]);

        service.AddEntry(new BudgetEntry(new DateOnly(2026, 7, 1), BudgetCategory.Food, "Groceries", 40.00m));
        service.AddEntry(new BudgetEntry(new DateOnly(2026, 7, 3), BudgetCategory.Food, "Dinner", 25.00m));
        service.AddEntry(new BudgetEntry(new DateOnly(2026, 7, 5), BudgetCategory.Utilities, "Internet", 65.00m));

        var report = service.GetMonthlyReport(new DateOnly(2026, 7, 1));

        Assert.AreEqual(1, report.OverBudgetCategoryCount);
        Assert.AreEqual(2, report.CategoryBudgetStatuses.Count);
        Assert.AreEqual(BudgetCategory.Food, report.CategoryBudgetStatuses[0].Category);
        Assert.IsTrue(report.CategoryBudgetStatuses[0].IsOverBudget);
        Assert.AreEqual(15.00m, report.CategoryBudgetStatuses[0].Variance);
        Assert.IsFalse(report.CategoryBudgetStatuses[1].IsOverBudget);
    }

    /// <summary>
    /// Confirms CSV export includes only the requested month.
    /// </summary>
    [TestMethod]
    public void ExportMonthToCsv_ReturnsRequestedMonthEntries()
    {
        var service = CreateService();
        service.AddEntry(new BudgetEntry(new DateOnly(2026, 7, 10), BudgetCategory.Entertainment, "Movie night", 24.00m));
        service.AddEntry(new BudgetEntry(new DateOnly(2026, 8, 10), BudgetCategory.Entertainment, "Concert", 80.00m));

        var csv = service.ExportMonthToCsv(new DateOnly(2026, 7, 1));

        StringAssert.Contains(csv, "Movie night");
        Assert.IsFalse(csv.Contains("Concert", StringComparison.Ordinal));
    }

    /// <summary>
    /// Confirms CSV file export writes only the requested month.
    /// </summary>
    [TestMethod]
    public void ExportMonthToCsvFile_CreatesRequestedMonthFile()
    {
        var service = CreateService();
        var filePath = Path.Combine(Path.GetTempPath(), "BudgetTrackerServiceTests", Guid.NewGuid().ToString("N"), "month.csv");

        service.AddEntry(new BudgetEntry(new DateOnly(2026, 7, 10), BudgetCategory.Entertainment, "Movie night", 24.00m));
        service.AddEntry(new BudgetEntry(new DateOnly(2026, 8, 10), BudgetCategory.Entertainment, "Concert", 80.00m));

        var result = service.ExportMonthToCsvFile(new DateOnly(2026, 7, 1), filePath, overwriteExisting: false);
        var content = File.ReadAllText(filePath);

        Assert.AreEqual(1, result.EntryCount);
        StringAssert.Contains(content, "Movie night");
        Assert.IsFalse(content.Contains("Concert", StringComparison.Ordinal));
    }

    /// <summary>
    /// Confirms CSV imports add new entries and skip duplicates.
    /// </summary>
    [TestMethod]
    public void ImportEntriesFromCsvFile_AddsNewEntriesAndSkipsDuplicates()
    {
        var existingEntries = new List<BudgetEntry>
        {
            new(new DateOnly(2026, 7, 10), BudgetCategory.Food, "Groceries", 45.50m)
        };

        var service = CreateService(initialEntries: existingEntries);
        var filePath = Path.Combine(Path.GetTempPath(), "BudgetTrackerServiceTests", Guid.NewGuid().ToString("N"), "import.csv");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(
            filePath,
            """
            Date,Category,Description,Amount
            2026-07-10,Food,Groceries,45.50
            2026-07-11,Transportation,Train pass,12.00
            """);

        var result = service.ImportEntriesFromCsvFile(filePath);
        var entries = service.GetEntries();

        Assert.AreEqual(1, result.ImportedCount);
        Assert.AreEqual(1, result.DuplicateCount);
        Assert.AreEqual(2, entries.Count);
    }

    /// <summary>
    /// Confirms invalid entries are rejected.
    /// </summary>
    [TestMethod]
    public void AddEntry_RejectsNonPositiveAmounts()
    {
        var service = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.AddEntry(new BudgetEntry(new DateOnly(2026, 7, 1), BudgetCategory.Food, "Invalid", 0m)));
    }

    /// <summary>
    /// Creates a test-ready service instance.
    /// </summary>
    /// <returns>A configured budget tracker service.</returns>
    private static BudgetTrackerService CreateService(
        IReadOnlyList<BudgetEntry>? initialEntries = null,
        IReadOnlyList<CategoryBudgetTarget>? targets = null)
    {
        return new BudgetTrackerService(
            new InMemoryBudgetEntryStore(initialEntries),
            new InMemoryCategoryBudgetTargetProvider(targets),
            new MonthlyReportBuilder(),
            new CsvExportService(),
            new CsvExportFileService(new StructuredConsoleLogger()),
            new CsvImportService(new StructuredConsoleLogger()),
            new StructuredConsoleLogger());
    }

    /// <summary>
    /// Stores entries in memory for tests.
    /// </summary>
    private sealed class InMemoryBudgetEntryStore : IBudgetEntryStore
    {
        private List<BudgetEntry> storedEntries;

        /// <summary>
        /// Initializes the in-memory test store.
        /// </summary>
        /// <param name="initialEntries">Optional seed entries.</param>
        public InMemoryBudgetEntryStore(IReadOnlyList<BudgetEntry>? initialEntries = null)
        {
            storedEntries = initialEntries?.ToList() ?? [];
        }

        /// <summary>
        /// Returns the currently stored entries.
        /// </summary>
        /// <returns>The stored entries.</returns>
        public IReadOnlyList<BudgetEntry> LoadEntries()
        {
            return storedEntries.ToList();
        }

        /// <summary>
        /// Replaces the stored entries snapshot.
        /// </summary>
        /// <param name="entries">The entries to persist.</param>
        public void SaveEntries(IReadOnlyList<BudgetEntry> entries)
        {
            storedEntries = entries.ToList();
        }
    }

    /// <summary>
    /// Provides configured category targets for tests.
    /// </summary>
    private sealed class InMemoryCategoryBudgetTargetProvider : ICategoryBudgetTargetProvider
    {
        private readonly IReadOnlyList<CategoryBudgetTarget> targets;

        /// <summary>
        /// Initializes the in-memory target provider.
        /// </summary>
        /// <param name="targets">Optional configured targets.</param>
        public InMemoryCategoryBudgetTargetProvider(IReadOnlyList<CategoryBudgetTarget>? targets = null)
        {
            this.targets = targets ?? [];
        }

        /// <summary>
        /// Returns the configured test targets.
        /// </summary>
        /// <returns>The configured targets.</returns>
        public IReadOnlyList<CategoryBudgetTarget> LoadTargets()
        {
            return targets;
        }
    }
}
