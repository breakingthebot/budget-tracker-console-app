// Services/MonthlyReportCsvExportService.cs
// Converts monthly report summaries into CSV output for reporting export workflows.
// Connects to: Models/MonthlyReport.cs, Models/CategoryBudgetStatus.cs, Models/SavingsProgress.cs, Services/BudgetTrackerService.cs
// Created: 2026-07-01

using System.Text;
using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Services;

/// <summary>
/// Exports monthly reports as CSV text.
/// </summary>
public sealed class MonthlyReportCsvExportService
{
    /// <summary>
    /// Creates a CSV document for the provided monthly report.
    /// </summary>
    /// <param name="report">The report to export.</param>
    /// <returns>The CSV document content.</returns>
    public string Export(MonthlyReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("Section,Metric,Value");
        builder.AppendLine($"Summary,Month,{report.Month:yyyy-MM}");
        builder.AppendLine($"Summary,Status,{Escape(report.MonthEndSummary.Status)}");
        builder.AppendLine($"Summary,EntryCount,{report.EntryCount}");
        builder.AppendLine($"Summary,TotalSpent,{report.TotalSpent:0.00}");
        builder.AppendLine($"Summary,OverBudgetCategoryCount,{report.OverBudgetCategoryCount}");
        builder.AppendLine($"Summary,SpendingTargetsOnTrack,{report.MonthEndSummary.SpendingTargetsOnTrack}");
        builder.AppendLine($"Summary,SpendingTargetsConfigured,{report.MonthEndSummary.SpendingTargetsConfigured}");
        builder.AppendLine($"Summary,NetSpendingVariance,{report.MonthEndSummary.NetSpendingVariance:0.00}");

        if (report.SavingsProgress is not null)
        {
            builder.AppendLine($"Savings,Category,{Escape(report.SavingsProgress.Category)}");
            builder.AppendLine($"Savings,SavedAmount,{report.SavingsProgress.SavedAmount:0.00}");
            builder.AppendLine($"Savings,TargetAmount,{report.SavingsProgress.TargetAmount:0.00}");
            builder.AppendLine($"Savings,RemainingAmount,{report.SavingsProgress.RemainingAmount:0.00}");
            builder.AppendLine($"Savings,ProgressPercentage,{report.SavingsProgress.ProgressPercentage:0.##}");
            builder.AppendLine($"Savings,IsGoalMet,{report.SavingsProgress.IsGoalMet}");
        }

        builder.AppendLine();
        builder.AppendLine("CategoryBreakdown,Category,Total");

        foreach (var item in report.CategoryBreakdown)
        {
            builder.AppendLine($"CategoryBreakdown,{Escape(item.Category)},{item.Total:0.00}");
        }

        builder.AppendLine();
        builder.AppendLine("BudgetStatus,Category,Spent,Target,Variance,IsOverBudget");

        foreach (var status in report.CategoryBudgetStatuses)
        {
            builder.AppendLine(
                $"BudgetStatus,{Escape(status.Category)},{status.Spent:0.00},{status.Target:0.00},{status.Variance:0.00},{status.IsOverBudget}");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Escapes CSV values that contain separators or quotes.
    /// </summary>
    /// <param name="value">The raw value to encode.</param>
    /// <returns>A CSV-safe value.</returns>
    private static string Escape(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return escaped.Contains(',') || escaped.Contains('"')
            ? $"\"{escaped}\""
            : escaped;
    }
}
