// Models/CategoryBudgetTargetHistoryEntry.cs
// Stores one audited budget-target change for later review.
// Connects to: Services/BudgetTrackerService.cs, Persistence/JsonCategoryBudgetTargetHistoryStore.cs, App/Console/ConsoleWorkflow.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents one persisted change to a category budget target.
/// </summary>
/// <param name="ChangedAtUtc">The UTC timestamp when the target changed.</param>
/// <param name="Category">The category whose target changed.</param>
/// <param name="PreviousMonthlyTarget">The target value before the change.</param>
/// <param name="UpdatedMonthlyTarget">The target value after the change.</param>
/// <param name="EvaluationMode">The evaluation mode attached to the target at change time.</param>
public sealed record CategoryBudgetTargetHistoryEntry(
    DateTimeOffset ChangedAtUtc,
    string Category,
    decimal PreviousMonthlyTarget,
    decimal UpdatedMonthlyTarget,
    string EvaluationMode);
