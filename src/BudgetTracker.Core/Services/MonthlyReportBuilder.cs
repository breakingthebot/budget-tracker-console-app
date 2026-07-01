// Services/MonthlyReportBuilder.cs
// Builds monthly spending summaries, savings progress, and target comparisons from a set of budget entries.
// Connects to: Models/BudgetEntry.cs, Models/CategoryDefinition.cs, Models/CategorySpend.cs, Models/CategoryBudgetTarget.cs, Models/CategoryBudgetStatus.cs, Models/SavingsProgress.cs, Models/MonthEndSummary.cs, Models/MonthlyReport.cs
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
        var spendingTargets = targets
            .Where(target => string.Equals(target.EvaluationMode, BudgetTargetEvaluationModes.MaxSpend, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var categoryBudgetStatuses = spendingTargets
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

        var savingsTarget = targets.FirstOrDefault(
            target => string.Equals(target.EvaluationMode, BudgetTargetEvaluationModes.MinProgress, StringComparison.OrdinalIgnoreCase));

        SavingsProgress? savingsProgress = null;

        if (savingsTarget is not null)
        {
            var savedAmount = spendByCategory.GetValueOrDefault(savingsTarget.Category, 0m);
            var remainingAmount = Math.Max(0m, savingsTarget.MonthlyTarget - savedAmount);
            var progressPercentage = savingsTarget.MonthlyTarget == 0
                ? 0m
                : Math.Min(100m, Math.Round((savedAmount / savingsTarget.MonthlyTarget) * 100m, 2));

            savingsProgress = new SavingsProgress(
                savingsTarget.Category,
                savedAmount,
                savingsTarget.MonthlyTarget,
                remainingAmount,
                savedAmount >= savingsTarget.MonthlyTarget,
                progressPercentage);
        }

        var netSpendingVariance = categoryBudgetStatuses.Sum(item => item.Variance);
        var monthEndStatus = BuildMonthEndStatus(categoryBudgetStatuses, savingsProgress);
        var monthEndSummary = new MonthEndSummary(
            monthEndStatus,
            categoryBudgetStatuses.Count(item => !item.IsOverBudget),
            categoryBudgetStatuses.Count,
            netSpendingVariance);

        return new MonthlyReport(
            normalizedMonth,
            monthlyEntries.Sum(entry => entry.Amount),
            monthlyEntries.Count,
            categoryBreakdown,
            categoryBudgetStatuses,
            categoryBudgetStatuses.Count(item => item.IsOverBudget),
            savingsProgress,
            monthEndSummary);
    }

    /// <summary>
    /// Builds the top-level month-end status message from spending and savings signals.
    /// </summary>
    /// <param name="categoryBudgetStatuses">The evaluated spending target statuses.</param>
    /// <param name="savingsProgress">The evaluated savings progress, when configured.</param>
    /// <returns>The user-facing month-end status line.</returns>
    private static string BuildMonthEndStatus(
        IReadOnlyList<CategoryBudgetStatus> categoryBudgetStatuses,
        SavingsProgress? savingsProgress)
    {
        var hasOverBudgetCategories = categoryBudgetStatuses.Any(status => status.IsOverBudget);

        if (hasOverBudgetCategories)
        {
            return savingsProgress is { IsGoalMet: true }
                ? "Mixed month: savings goal met, but some spending targets were missed."
                : "Needs attention: spending targets were missed this month.";
        }

        if (savingsProgress is { IsGoalMet: true })
        {
            return "Strong month: spending stayed on track and the savings goal was met.";
        }

        if (savingsProgress is not null)
        {
            return "On track: spending stayed within target, and savings are still in progress.";
        }

        return "On track: spending stayed within configured targets.";
    }
}
