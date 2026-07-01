// Models/BudgetTargetEvaluationModes.cs
// Defines the supported evaluation modes for configured budget targets.
// Connects to: Models/CategoryBudgetTarget.cs, Services/MonthlyReportBuilder.cs, Persistence/JsonCategoryBudgetTargetProvider.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Defines supported evaluation modes for budget targets.
/// </summary>
public static class BudgetTargetEvaluationModes
{
    /// <summary>
    /// Treats the target as a maximum spend threshold.
    /// </summary>
    public const string MaxSpend = "max-spend";

    /// <summary>
    /// Treats the target as a minimum progress goal.
    /// </summary>
    public const string MinProgress = "min-progress";
}
