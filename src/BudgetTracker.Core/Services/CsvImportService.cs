// Services/CsvImportService.cs
// Parses budget entries from a CSV file with explicit row validation.
// Connects to: Models/BudgetEntry.cs, Logging/StructuredConsoleLogger.cs, Services/BudgetTrackerService.cs
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Services;

/// <summary>
/// Reads budget entries from CSV files.
/// </summary>
public sealed class CsvImportService
{
    private const string ExpectedHeader = "Date,Category,Description,Amount";
    private readonly StructuredConsoleLogger logger;

    /// <summary>
    /// Initializes the CSV import service.
    /// </summary>
    /// <param name="logger">The logger used for import diagnostics.</param>
    public CsvImportService(StructuredConsoleLogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Loads budget entries from the provided CSV file path.
    /// </summary>
    /// <param name="filePath">The CSV file path to import.</param>
    /// <returns>The parsed budget entries.</returns>
    public IReadOnlyList<BudgetEntry> LoadEntries(string filePath)
    {
        ValidateFilePath(filePath);

        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException($"Import file was not found at '{filePath}'.");
        }

        try
        {
            var lines = File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                throw new InvalidOperationException("Import file is empty.");
            }

            if (!string.Equals(lines[0].Trim(), ExpectedHeader, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Import file must start with header '{ExpectedHeader}'.");
            }

            var entries = new List<BudgetEntry>();

            for (var index = 1; index < lines.Length; index++)
            {
                var line = lines[index].Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                entries.Add(ParseLine(line, index + 1));
            }

            logger.LogInfo("CSV import file parsed.", new { filePath, count = entries.Count });
            return entries;
        }
        catch (IOException exception)
        {
            logger.LogError("CSV import file could not be read.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not read import file at '{filePath}'.", exception);
        }
    }

    /// <summary>
    /// Parses one CSV data line into a budget entry.
    /// </summary>
    /// <param name="line">The CSV line to parse.</param>
    /// <param name="lineNumber">The 1-based file line number.</param>
    /// <returns>The parsed budget entry.</returns>
    private static BudgetEntry ParseLine(string line, int lineNumber)
    {
        var columns = line.Split(',');

        if (columns.Length != 4)
        {
            throw new InvalidOperationException($"Line {lineNumber} must contain exactly 4 comma-separated values.");
        }

        if (!DateOnly.TryParse(columns[0].Trim(), out var date))
        {
            throw new InvalidOperationException($"Line {lineNumber} contains an invalid date.");
        }

        if (!Enum.TryParse<BudgetCategory>(columns[1].Trim(), ignoreCase: true, out var category))
        {
            throw new InvalidOperationException($"Line {lineNumber} contains an unknown category.");
        }

        var description = columns[2].Trim();

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException($"Line {lineNumber} must include a description.");
        }

        if (!decimal.TryParse(columns[3].Trim(), out var amount) || amount <= 0)
        {
            throw new InvalidOperationException($"Line {lineNumber} contains an invalid amount.");
        }

        return new BudgetEntry(date, category, description, amount);
    }

    /// <summary>
    /// Ensures the import file path is valid.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Import file path is required.", nameof(filePath));
        }

        if (!string.Equals(Path.GetExtension(filePath), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Import file path must use the .csv extension.");
        }
    }
}
