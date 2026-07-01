// Models/MonthlyReport.cs
// Represents the calculated totals for one month of budget activity.
// Connects to: Models/CategorySpend.cs, Services/MonthlyReportBuilder.cs, App/ConsoleWorkflow.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents a report for one calendar month.
/// </summary>
/// <param name="Month">The first day of the reported month.</param>
/// <param name="TotalSpent">The total spent during the month.</param>
/// <param name="EntryCount">The number of entries in the month.</param>
/// <param name="CategoryBreakdown">The grouped totals by category.</param>
public sealed record MonthlyReport(
    DateOnly Month,
    decimal TotalSpent,
    int EntryCount,
    IReadOnlyList<CategorySpend> CategoryBreakdown);
