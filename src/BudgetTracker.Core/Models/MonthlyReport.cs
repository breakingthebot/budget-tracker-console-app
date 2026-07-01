// Models/MonthlyReport.cs
// Represents the calculated totals and budget status for one month of activity.
// Connects to: Models/CategorySpend.cs, Models/CategoryBudgetStatus.cs, Services/MonthlyReportBuilder.cs, App/ConsoleWorkflow.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents a report for one calendar month.
/// </summary>
/// <param name="Month">The first day of the reported month.</param>
/// <param name="TotalSpent">The total spent during the month.</param>
/// <param name="EntryCount">The number of entries in the month.</param>
/// <param name="CategoryBreakdown">The grouped totals by category.</param>
/// <param name="CategoryBudgetStatuses">The grouped status for categories with configured targets.</param>
/// <param name="OverBudgetCategoryCount">The number of configured categories that exceeded their targets.</param>
public sealed record MonthlyReport(
    DateOnly Month,
    decimal TotalSpent,
    int EntryCount,
    IReadOnlyList<CategorySpend> CategoryBreakdown,
    IReadOnlyList<CategoryBudgetStatus> CategoryBudgetStatuses,
    int OverBudgetCategoryCount);
