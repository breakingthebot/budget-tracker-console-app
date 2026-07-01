// Services/MonthlyReportBuilder.cs
// Builds monthly spending summaries from a set of budget entries.
// Connects to: Models/BudgetEntry.cs, Models/CategorySpend.cs, Models/MonthlyReport.cs
// Created: 2026-07-01

using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Services;

/// <summary>
/// Creates monthly reports from budget entries.
/// </summary>
public sealed class MonthlyReportBuilder
{
    /// <summary>
    /// Builds a report for the requested month.
    /// </summary>
    /// <param name="entries">All available entries.</param>
    /// <param name="month">Any date within the month to summarize.</param>
    /// <returns>A report with totals and category breakdown.</returns>
    public MonthlyReport Build(IEnumerable<BudgetEntry> entries, DateOnly month)
    {
        var normalizedMonth = new DateOnly(month.Year, month.Month, 1);
        var monthlyEntries = entries
            .Where(entry => entry.Date.Year == normalizedMonth.Year && entry.Date.Month == normalizedMonth.Month)
            .ToList();

        var categoryBreakdown = monthlyEntries
            .GroupBy(entry => entry.Category)
            .Select(group => new CategorySpend(group.Key, group.Sum(item => item.Amount)))
            .OrderByDescending(item => item.Total)
            .ThenBy(item => item.Category)
            .ToList();

        return new MonthlyReport(
            normalizedMonth,
            monthlyEntries.Sum(entry => entry.Amount),
            monthlyEntries.Count,
            categoryBreakdown);
    }
}
