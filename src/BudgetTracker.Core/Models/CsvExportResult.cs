// Models/CsvExportResult.cs
// Represents the output of exporting monthly entries to a CSV file.
// Connects to: Services/CsvExportFileService.cs, Services/BudgetTrackerService.cs, App/ConsoleWorkflow.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents a completed CSV file export.
/// </summary>
/// <param name="FilePath">The full path to the created CSV file.</param>
/// <param name="EntryCount">The number of exported entries.</param>
public sealed record CsvExportResult(string FilePath, int EntryCount);
