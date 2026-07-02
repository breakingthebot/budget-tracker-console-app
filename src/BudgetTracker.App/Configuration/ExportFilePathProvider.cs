// Configuration/ExportFilePathProvider.cs
// Resolves predictable CSV export file paths inside the local exports folder.
// Connects to: Console/ConsoleWorkflow.cs, Core/Services/CsvExportFileService.cs
// Created: 2026-07-01

namespace BudgetTracker.App.Configuration;

/// <summary>
/// Resolves file paths for monthly CSV exports.
/// </summary>
public static class ExportFilePathProvider
{
    private const string ExportDirectoryName = "exports";
    private const string TransactionExportFilePrefix = "budget-export";
    private const string ReportExportFilePrefix = "budget-report";

    /// <summary>
    /// Builds the export path for a given reporting month.
    /// </summary>
    /// <param name="month">The reporting month to export.</param>
    /// <returns>The full CSV file path inside the exports directory.</returns>
    public static string GetMonthlyExportFilePath(DateOnly month)
    {
        var fileName = $"{TransactionExportFilePrefix}-{month:yyyy-MM}.csv";
        return Path.Combine(Environment.CurrentDirectory, ExportDirectoryName, fileName);
    }

    /// <summary>
    /// Builds the report export path for a given reporting month.
    /// </summary>
    /// <param name="month">The reporting month to export.</param>
    /// <returns>The full CSV file path inside the exports directory.</returns>
    public static string GetMonthlyReportExportFilePath(DateOnly month)
    {
        var fileName = $"{ReportExportFilePrefix}-{month:yyyy-MM}.csv";
        return Path.Combine(Environment.CurrentDirectory, ExportDirectoryName, fileName);
    }
}
