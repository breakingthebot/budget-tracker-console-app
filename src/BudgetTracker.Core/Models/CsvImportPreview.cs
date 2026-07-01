// Models/CsvImportPreview.cs
// Represents a preview of CSV rows before they are imported into storage.
// Connects to: Services/BudgetTrackerService.cs, App/ConsoleWorkflow.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents a preview of a pending CSV import.
/// </summary>
/// <param name="FilePath">The CSV file path being previewed.</param>
/// <param name="NewEntries">The rows that would be imported.</param>
/// <param name="DuplicateEntries">The rows that would be skipped as duplicates.</param>
public sealed record CsvImportPreview(
    string FilePath,
    IReadOnlyList<BudgetEntry> NewEntries,
    IReadOnlyList<BudgetEntry> DuplicateEntries);
