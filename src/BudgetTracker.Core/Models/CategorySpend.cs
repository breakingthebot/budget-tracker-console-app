// Models/CategorySpend.cs
// Holds category-level totals used inside monthly budget reports.
// Connects to: Models/CategoryDefinition.cs, Models/MonthlyReport.cs, Services/MonthlyReportBuilder.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents monthly spending for a single category.
/// </summary>
/// <param name="Category">The category being summarized.</param>
/// <param name="Total">The summed amount for that category.</param>
public sealed record CategorySpend(string Category, decimal Total);
