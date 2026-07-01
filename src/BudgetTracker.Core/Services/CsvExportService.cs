// Services/CsvExportService.cs
// Converts budget entries into CSV output for export workflows.
// Connects to: Models/BudgetEntry.cs, Services/BudgetTrackerService.cs
// Created: 2026-07-01

using System.Text;
using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Services;

/// <summary>
/// Exports budget entries as CSV text.
/// </summary>
public sealed class CsvExportService
{
    private const string HeaderRow = "Date,Category,Description,Amount";

    /// <summary>
    /// Creates a CSV document for the provided entries.
    /// </summary>
    /// <param name="entries">The entries to export.</param>
    /// <returns>The CSV document content.</returns>
    public string Export(IEnumerable<BudgetEntry> entries)
    {
        var builder = new StringBuilder();
        builder.AppendLine(HeaderRow);

        foreach (var entry in entries.OrderBy(item => item.Date).ThenBy(item => item.Category))
        {
            builder.AppendLine(
                string.Join(
                    ",",
                    entry.Date.ToString("yyyy-MM-dd"),
                    Escape(entry.Category.ToString()),
                    Escape(entry.Description),
                    entry.Amount.ToString("0.00")));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Escapes CSV values that contain separators or quotes.
    /// </summary>
    /// <param name="value">The raw value to encode.</param>
    /// <returns>A CSV-safe value.</returns>
    private static string Escape(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return escaped.Contains(',') || escaped.Contains('"')
            ? $"\"{escaped}\""
            : escaped;
    }
}
