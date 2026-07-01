// Models/CategoryBudgetTarget.cs
// Stores a configured monthly budget target for one category.
// Connects to: Models/CategoryDefinition.cs, Models/CategoryBudgetStatus.cs, Persistence/JsonCategoryBudgetTargetProvider.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents a configured monthly target for a category.
/// </summary>
/// <param name="Category">The category with a target.</param>
/// <param name="MonthlyTarget">The allowed monthly spend for that category.</param>
/// <param name="EvaluationMode">How the target should be interpreted when building reports.</param>
public sealed record CategoryBudgetTarget(
    string Category,
    decimal MonthlyTarget,
    string EvaluationMode = BudgetTargetEvaluationModes.MaxSpend);
