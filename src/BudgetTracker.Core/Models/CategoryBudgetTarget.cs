// Models/CategoryBudgetTarget.cs
// Stores a configured monthly budget target for one category.
// Connects to: Models/BudgetCategory.cs, Models/CategoryBudgetStatus.cs, Persistence/JsonCategoryBudgetTargetProvider.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents a configured monthly target for a category.
/// </summary>
/// <param name="Category">The category with a target.</param>
/// <param name="MonthlyTarget">The allowed monthly spend for that category.</param>
public sealed record CategoryBudgetTarget(BudgetCategory Category, decimal MonthlyTarget);
