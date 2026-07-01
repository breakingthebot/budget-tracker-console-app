// Models/CsvImportResult.cs
// Represents the outcome of importing budget entries from a CSV file.
// Connects to: Services/CsvImportService.cs, Services/BudgetTrackerService.cs, App/ConsoleWorkflow.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents the completed result of a CSV import.
/// </summary>
/// <param name="FilePath">The imported CSV file path.</param>
/// <param name="ImportedCount">The number of new entries added.</param>
/// <param name="DuplicateCount">The number of duplicate rows skipped.</param>
public sealed record CsvImportResult(string FilePath, int ImportedCount, int DuplicateCount);
