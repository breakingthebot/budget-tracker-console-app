// Services/MonthlyReportBuilder.cs
// Builds monthly spending summaries and target comparisons from a set of budget entries.
// Connects to: Models/BudgetEntry.cs, Models/CategoryDefinition.cs, Models/CategorySpend.cs, Models/CategoryBudgetTarget.cs, Models/CategoryBudgetStatus.cs, Models/MonthlyReport.cs
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
    /// <param name="configuredCategories">The configured categories for display ordering.</param>
    /// <param name="targets">The configured category targets.</param>
    /// <returns>A report with totals and category breakdown.</returns>
    public MonthlyReport Build(
        IEnumerable<BudgetEntry> entries,
        DateOnly month,
        IEnumerable<CategoryDefinition> configuredCategories,
        IEnumerable<CategoryBudgetTarget> targets)
    {
        var normalizedMonth = new DateOnly(month.Year, month.Month, 1);
        var monthlyEntries = entries
            .Where(entry => entry.Date.Year == normalizedMonth.Year && entry.Date.Month == normalizedMonth.Month)
            .ToList();

        var configuredCategoryOrder = configuredCategories
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select((category, index) => new { category.Name, Index = index })
            .ToDictionary(item => item.Name, item => item.Index, StringComparer.OrdinalIgnoreCase);

        var categoryBreakdown = monthlyEntries
            .GroupBy(entry => entry.Category)
            .Select(group => new CategorySpend(group.Key, group.Sum(item => item.Amount)))
            .OrderByDescending(item => item.Total)
            .ThenBy(item => configuredCategoryOrder.GetValueOrDefault(item.Category, int.MaxValue))
            .ThenBy(item => item.Category)
            .ToList();

        var spendByCategory = categoryBreakdown.ToDictionary(item => item.Category, item => item.Total, StringComparer.OrdinalIgnoreCase);
        var categoryBudgetStatuses = targets
            .Select(target =>
            {
                var spent = spendByCategory.GetValueOrDefault(target.Category, 0m);
                var variance = spent - target.MonthlyTarget;
                return new CategoryBudgetStatus(
                    target.Category,
                    spent,
                    target.MonthlyTarget,
                    variance,
                    variance > 0);
            })
            .OrderByDescending(item => item.IsOverBudget)
            .ThenByDescending(item => Math.Abs(item.Variance))
            .ThenBy(item => configuredCategoryOrder.GetValueOrDefault(item.Category, int.MaxValue))
            .ThenBy(item => item.Category)
            .ToList();

        return new MonthlyReport(
            normalizedMonth,
            monthlyEntries.Sum(entry => entry.Amount),
            monthlyEntries.Count,
            categoryBreakdown,
            categoryBudgetStatuses,
            categoryBudgetStatuses.Count(item => item.IsOverBudget));
    }
}
