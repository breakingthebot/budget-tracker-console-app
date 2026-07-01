// Services/CsvExportFileService.cs
// Writes generated CSV content to disk with safe directory and overwrite handling.
// Connects to: Models/CsvExportResult.cs, Logging/StructuredConsoleLogger.cs
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Services;

/// <summary>
/// Writes CSV export content to files on disk.
/// </summary>
public sealed class CsvExportFileService
{
    private readonly StructuredConsoleLogger logger;

    /// <summary>
    /// Initializes the CSV file export service.
    /// </summary>
    /// <param name="logger">The logger used for export diagnostics.</param>
    public CsvExportFileService(StructuredConsoleLogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Writes CSV content to the requested file path.
    /// </summary>
    /// <param name="filePath">The full path for the CSV file.</param>
    /// <param name="csvContent">The CSV content to write.</param>
    /// <param name="entryCount">The number of exported entries.</param>
    /// <param name="overwriteExisting">Whether an existing file may be replaced.</param>
    /// <returns>The result describing the created export file.</returns>
    public CsvExportResult WriteToFile(string filePath, string csvContent, int entryCount, bool overwriteExisting)
    {
        ValidateFilePath(filePath);

        if (File.Exists(filePath) && !overwriteExisting)
        {
            throw new InvalidOperationException($"Export file already exists at '{filePath}'.");
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, csvContent);

            logger.LogInfo(
                "Monthly CSV written to file.",
                new { filePath, entryCount, overwriteExisting });

            return new CsvExportResult(filePath, entryCount);
        }
        catch (IOException exception)
        {
            logger.LogError("Monthly CSV file could not be written.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not write CSV export file at '{filePath}'.", exception);
        }
    }

    /// <summary>
    /// Ensures the provided file path is a CSV file path.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Export file path is required.", nameof(filePath));
        }

        if (!string.Equals(Path.GetExtension(filePath), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Export file path must use the .csv extension.");
        }
    }
}
