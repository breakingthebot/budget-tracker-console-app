// Models/MonthEndSummary.cs
// Represents the high-level monthly budget outcome shown at the top of reports.
// Connects to: Models/MonthlyReport.cs, Models/SavingsProgress.cs, Services/MonthlyReportBuilder.cs, App/ConsoleWorkflow.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents the overall status of one month's budget performance.
/// </summary>
/// <param name="Status">The user-facing summary status line.</param>
/// <param name="SpendingTargetsOnTrack">The number of spending targets currently on track.</param>
/// <param name="SpendingTargetsConfigured">The total number of configured spending targets.</param>
/// <param name="NetSpendingVariance">The aggregate variance across configured spending targets. Negative means under target.</param>
public sealed record MonthEndSummary(
    string Status,
    int SpendingTargetsOnTrack,
    int SpendingTargetsConfigured,
    decimal NetSpendingVariance);
