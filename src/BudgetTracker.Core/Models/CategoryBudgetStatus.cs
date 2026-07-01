// Models/CategoryBudgetStatus.cs
// Represents how one category performed against its configured monthly target.
// Connects to: Models/CategoryBudgetTarget.cs, Models/MonthlyReport.cs, Services/MonthlyReportBuilder.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents a category's monthly spending compared to its target.
/// </summary>
/// <param name="Category">The category being evaluated.</param>
/// <param name="Spent">The amount spent during the month.</param>
/// <param name="Target">The configured monthly target.</param>
/// <param name="Variance">The amount over or under target. Positive means over budget.</param>
/// <param name="IsOverBudget">Indicates whether the category exceeded its target.</param>
public sealed record CategoryBudgetStatus(
    BudgetCategory Category,
    decimal Spent,
    decimal Target,
    decimal Variance,
    bool IsOverBudget);
