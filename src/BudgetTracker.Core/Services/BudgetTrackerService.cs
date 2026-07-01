// Services/BudgetTrackerService.cs
// Validates, stores, reports, imports, and exports budget entries with persistence and target support.
// Connects to: Models/BudgetEntry.cs, Models/CsvExportResult.cs, Models/CsvImportResult.cs, Abstractions/IBudgetEntryStore.cs, Abstractions/ICategoryBudgetTargetProvider.cs, Logging/StructuredConsoleLogger.cs, Services/MonthlyReportBuilder.cs, Services/CsvExportService.cs, Services/CsvExportFileService.cs, Services/CsvImportService.cs
// Created: 2026-07-01

using BudgetTracker.Core.Abstractions;
using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Services;

/// <summary>
/// Coordinates budget entry management and reporting.
/// </summary>
public sealed class BudgetTrackerService
{
    private readonly List<BudgetEntry> entries = [];
    private readonly IBudgetEntryStore budgetEntryStore;
    private readonly ICategoryBudgetTargetProvider categoryBudgetTargetProvider;
    private readonly MonthlyReportBuilder reportBuilder;
    private readonly CsvExportService csvExportService;
    private readonly CsvExportFileService csvExportFileService;
    private readonly CsvImportService csvImportService;
    private readonly StructuredConsoleLogger logger;

    /// <summary>
    /// Initializes the budget tracker service.
    /// </summary>
    /// <param name="budgetEntryStore">Loads and saves tracked entries.</param>
    /// <param name="categoryBudgetTargetProvider">Loads configured category targets.</param>
    /// <param name="reportBuilder">Builds monthly reports.</param>
    /// <param name="csvExportService">Builds CSV export content.</param>
    /// <param name="csvExportFileService">Writes CSV files to disk.</param>
    /// <param name="csvImportService">Reads CSV files from disk.</param>
    /// <param name="logger">Writes structured application logs.</param>
    public BudgetTrackerService(
        IBudgetEntryStore budgetEntryStore,
        ICategoryBudgetTargetProvider categoryBudgetTargetProvider,
        MonthlyReportBuilder reportBuilder,
        CsvExportService csvExportService,
        CsvExportFileService csvExportFileService,
        CsvImportService csvImportService,
        StructuredConsoleLogger logger)
    {
        this.budgetEntryStore = budgetEntryStore;
        this.categoryBudgetTargetProvider = categoryBudgetTargetProvider;
        this.reportBuilder = reportBuilder;
        this.csvExportService = csvExportService;
        this.csvExportFileService = csvExportFileService;
        this.csvImportService = csvImportService;
        this.logger = logger;

        var storedEntries = budgetEntryStore.LoadEntries();
        entries.AddRange(storedEntries);
        this.logger.LogInfo("Budget tracker service initialized.", new { loadedEntryCount = entries.Count });
    }

    /// <summary>
    /// Adds a validated entry to the in-memory tracker.
    /// </summary>
    /// <param name="entry">The candidate entry.</param>
    public void AddEntry(BudgetEntry entry)
    {
        ValidateEntry(entry);
        entries.Add(entry);
        budgetEntryStore.SaveEntries(entries);

        logger.LogInfo(
            "Budget entry added.",
            new { entry.Date, Category = entry.Category.ToString(), entry.Description, entry.Amount });
    }

    /// <summary>
    /// Returns all entries in date order.
    /// </summary>
    /// <returns>A read-only snapshot of tracked entries.</returns>
    public IReadOnlyList<BudgetEntry> GetEntries()
    {
        return entries
            .OrderBy(entry => entry.Date)
            .ThenBy(entry => entry.Category)
            .ToList();
    }

    /// <summary>
    /// Creates a monthly spending report.
    /// </summary>
    /// <param name="month">Any date inside the month to report.</param>
    /// <returns>The calculated monthly report.</returns>
    public MonthlyReport GetMonthlyReport(DateOnly month)
    {
        logger.LogDebug("Building monthly report.", new { month = month.ToString("yyyy-MM") });
        var targets = categoryBudgetTargetProvider.LoadTargets();
        return reportBuilder.Build(entries, month, targets);
    }

    /// <summary>
    /// Exports entries for the requested month as CSV text.
    /// </summary>
    /// <param name="month">Any date inside the month to export.</param>
    /// <returns>The CSV document for that month.</returns>
    public string ExportMonthToCsv(DateOnly month)
    {
        var monthEntries = entries.Where(entry => entry.Date.Year == month.Year && entry.Date.Month == month.Month);
        logger.LogInfo("Exporting monthly CSV.", new { month = month.ToString("yyyy-MM"), count = monthEntries.Count() });
        return csvExportService.Export(monthEntries);
    }

    /// <summary>
    /// Exports entries for the requested month to a CSV file.
    /// </summary>
    /// <param name="month">Any date inside the month to export.</param>
    /// <param name="filePath">The CSV file path to create.</param>
    /// <param name="overwriteExisting">Whether an existing file may be replaced.</param>
    /// <returns>The created export file result.</returns>
    public CsvExportResult ExportMonthToCsvFile(DateOnly month, string filePath, bool overwriteExisting)
    {
        var monthEntries = entries
            .Where(entry => entry.Date.Year == month.Year && entry.Date.Month == month.Month)
            .ToList();

        var csvContent = csvExportService.Export(monthEntries);

        logger.LogInfo(
            "Exporting monthly CSV file.",
            new { month = month.ToString("yyyy-MM"), filePath, count = monthEntries.Count, overwriteExisting });

        return csvExportFileService.WriteToFile(filePath, csvContent, monthEntries.Count, overwriteExisting);
    }

    /// <summary>
    /// Imports entries from a CSV file and persists new rows.
    /// </summary>
    /// <param name="filePath">The CSV file path to import.</param>
    /// <returns>The completed import result.</returns>
    public CsvImportResult ImportEntriesFromCsvFile(string filePath)
    {
        var importedEntries = csvImportService.LoadEntries(filePath);
        var existingEntries = new HashSet<BudgetEntry>(entries);
        var newEntries = new List<BudgetEntry>();
        var duplicateCount = 0;

        foreach (var entry in importedEntries)
        {
            ValidateEntry(entry);

            if (!existingEntries.Add(entry))
            {
                duplicateCount++;
                continue;
            }

            newEntries.Add(entry);
        }

        entries.AddRange(newEntries);
        budgetEntryStore.SaveEntries(entries);

        logger.LogInfo(
            "CSV import completed.",
            new { filePath, importedCount = newEntries.Count, duplicateCount });

        return new CsvImportResult(filePath, newEntries.Count, duplicateCount);
    }

    /// <summary>
    /// Validates a budget entry before it is stored.
    /// </summary>
    /// <param name="entry">The entry to validate.</param>
    private static void ValidateEntry(BudgetEntry entry)
    {
        if (entry.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(entry), "Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(entry.Description))
        {
            throw new ArgumentException("Description is required.", nameof(entry));
        }
    }
}
