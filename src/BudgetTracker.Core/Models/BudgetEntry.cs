// Models/BudgetEntry.cs
// Stores a single budget transaction entered by the user.
// Connects to: Models/CategoryDefinition.cs, Services/BudgetTrackerService.cs, Services/CsvExportService.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents one tracked budget transaction.
/// </summary>
/// <param name="Date">The date the transaction occurred.</param>
/// <param name="Category">The configured category assigned to the transaction.</param>
/// <param name="Description">A short user-facing description.</param>
/// <param name="Amount">The positive currency amount for the transaction.</param>
public sealed record BudgetEntry(DateOnly Date, string Category, string Description, decimal Amount);
