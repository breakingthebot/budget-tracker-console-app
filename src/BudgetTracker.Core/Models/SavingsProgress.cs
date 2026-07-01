// Models/SavingsProgress.cs
// Represents monthly progress toward a configured savings goal.
// Connects to: Models/CategoryBudgetTarget.cs, Models/MonthlyReport.cs, Services/MonthlyReportBuilder.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents one month's progress toward a savings target.
/// </summary>
/// <param name="Category">The configured savings category name.</param>
/// <param name="SavedAmount">The amount recorded for the savings category.</param>
/// <param name="TargetAmount">The configured target amount.</param>
/// <param name="RemainingAmount">The remaining amount needed to hit the target.</param>
/// <param name="IsGoalMet">Indicates whether the savings goal was reached.</param>
/// <param name="ProgressPercentage">The percent of the target that has been reached.</param>
public sealed record SavingsProgress(
    string Category,
    decimal SavedAmount,
    decimal TargetAmount,
    decimal RemainingAmount,
    bool IsGoalMet,
    decimal ProgressPercentage);
