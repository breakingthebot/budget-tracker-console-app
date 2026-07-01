// Models/BudgetCategory.cs
// Defines the supported spending categories for budget entries and reports.
// Connects to: Models/BudgetEntry.cs, Services/BudgetTrackerService.cs, Services/MonthlyReportBuilder.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents the allowed budget categories in the application.
/// </summary>
public enum BudgetCategory
{
    Housing = 1,
    Food = 2,
    Transportation = 3,
    Utilities = 4,
    Healthcare = 5,
    Entertainment = 6,
    Savings = 7,
    Miscellaneous = 8
}
