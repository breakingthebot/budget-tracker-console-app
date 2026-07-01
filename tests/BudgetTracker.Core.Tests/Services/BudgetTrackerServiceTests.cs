// Services/BudgetTrackerServiceTests.cs
// Verifies entry validation, monthly report generation, and CSV export behavior.
// Connects to: Core/Services/BudgetTrackerService.cs, Core/Models/*
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;
using BudgetTracker.Core.Services;

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
    /// Confirms monthly reports sum totals by category.
    /// </summary>
    [TestMethod]
    public void GetMonthlyReport_ReturnsCategoryBreakdownAndTotals()
    {
        var service = CreateService();
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
    private static BudgetTrackerService CreateService()
    {
        return new BudgetTrackerService(
            new MonthlyReportBuilder(),
            new CsvExportService(),
            new StructuredConsoleLogger());
    }
}
