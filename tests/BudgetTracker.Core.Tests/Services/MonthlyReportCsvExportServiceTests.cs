// Services/MonthlyReportCsvExportServiceTests.cs
// Verifies monthly report CSV export structure and key report sections.
// Connects to: Core/Services/MonthlyReportCsvExportService.cs, Core/Models/MonthlyReport.cs
// Created: 2026-07-01

using BudgetTracker.Core.Models;
using BudgetTracker.Core.Services;

namespace BudgetTracker.Core.Tests.Services;

/// <summary>
/// Tests the monthly report CSV export service.
/// </summary>
[TestClass]
public sealed class MonthlyReportCsvExportServiceTests
{
    /// <summary>
    /// Exports a monthly report with summary, savings, and status sections.
    /// </summary>
    [TestMethod]
    public void Export_WithCompleteReport_ReturnsExpectedSections()
    {
        var service = new MonthlyReportCsvExportService();
        var report = new MonthlyReport(
            new DateOnly(2026, 7, 1),
            175.50m,
            4,
            [new CategorySpend("Food", 55.50m), new CategorySpend("Utilities", 70.00m)],
            [new CategoryBudgetStatus("Food", 55.50m, 75.00m, -19.50m, false)],
            0,
            new SavingsProgress("Savings", 50.00m, 60.00m, 10.00m, false, 83.33m),
            new MonthEndSummary("On track", 1, 1, -19.50m));

        var csv = service.Export(report);

        StringAssert.Contains(csv, "Section,Metric,Value");
        StringAssert.Contains(csv, "Summary,Month,2026-07");
        StringAssert.Contains(csv, "Savings,Category,Savings");
        StringAssert.Contains(csv, "CategoryBreakdown,Category,Total");
        StringAssert.Contains(csv, "BudgetStatus,Category,Spent,Target,Variance,IsOverBudget");
    }
}
