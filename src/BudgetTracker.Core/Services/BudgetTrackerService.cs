// Services/BudgetTrackerService.cs
// Validates, stores, reports, previews imports, imports, exports, and audits budget workflows with persistence and config-backed categories.
// Connects to: Models/BudgetEntry.cs, Models/CategoryDefinition.cs, Models/CategoryBudgetTargetHistoryEntry.cs, Models/CsvExportResult.cs, Models/CsvImportPreview.cs, Models/CsvImportResult.cs, Abstractions/IBudgetEntryStore.cs, Abstractions/ICategoryDefinitionProvider.cs, Abstractions/ICategoryBudgetTargetProvider.cs, Abstractions/ICategoryBudgetTargetHistoryStore.cs, Logging/StructuredConsoleLogger.cs, Services/MonthlyReportBuilder.cs, Services/CsvExportService.cs, Services/CsvExportFileService.cs, Services/CsvImportService.cs
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
    private readonly ICategoryDefinitionProvider categoryDefinitionProvider;
    private readonly ICategoryBudgetTargetProvider categoryBudgetTargetProvider;
    private readonly ICategoryBudgetTargetHistoryStore categoryBudgetTargetHistoryStore;
    private readonly MonthlyReportBuilder reportBuilder;
    private readonly CsvExportService csvExportService;
    private readonly CsvExportFileService csvExportFileService;
    private readonly CsvImportService csvImportService;
    private readonly StructuredConsoleLogger logger;

    /// <summary>
    /// Initializes the budget tracker service.
    /// </summary>
    /// <param name="budgetEntryStore">Loads and saves tracked entries.</param>
    /// <param name="categoryDefinitionProvider">Loads configured categories.</param>
    /// <param name="categoryBudgetTargetProvider">Loads configured category targets.</param>
    /// <param name="categoryBudgetTargetHistoryStore">Loads and saves budget-target audit history.</param>
    /// <param name="reportBuilder">Builds monthly reports.</param>
    /// <param name="csvExportService">Builds CSV export content.</param>
    /// <param name="csvExportFileService">Writes CSV files to disk.</param>
    /// <param name="csvImportService">Reads CSV files from disk.</param>
    /// <param name="logger">Writes structured application logs.</param>
    public BudgetTrackerService(
        IBudgetEntryStore budgetEntryStore,
        ICategoryDefinitionProvider categoryDefinitionProvider,
        ICategoryBudgetTargetProvider categoryBudgetTargetProvider,
        ICategoryBudgetTargetHistoryStore categoryBudgetTargetHistoryStore,
        MonthlyReportBuilder reportBuilder,
        CsvExportService csvExportService,
        CsvExportFileService csvExportFileService,
        CsvImportService csvImportService,
        StructuredConsoleLogger logger)
    {
        this.budgetEntryStore = budgetEntryStore;
        this.categoryDefinitionProvider = categoryDefinitionProvider;
        this.categoryBudgetTargetProvider = categoryBudgetTargetProvider;
        this.categoryBudgetTargetHistoryStore = categoryBudgetTargetHistoryStore;
        this.reportBuilder = reportBuilder;
        this.csvExportService = csvExportService;
        this.csvExportFileService = csvExportFileService;
        this.csvImportService = csvImportService;
        this.logger = logger;

        var configuredCategories = categoryDefinitionProvider.LoadCategories();
        var storedEntries = budgetEntryStore.LoadEntries();

        foreach (var entry in storedEntries)
        {
            ValidateEntry(entry, configuredCategories);
        }

        entries.AddRange(storedEntries);
        this.logger.LogInfo("Budget tracker service initialized.", new { loadedEntryCount = entries.Count });
    }

    /// <summary>
    /// Adds a validated entry to the in-memory tracker.
    /// </summary>
    /// <param name="entry">The candidate entry.</param>
    public void AddEntry(BudgetEntry entry)
    {
        var configuredCategories = categoryDefinitionProvider.LoadCategories();
        ValidateEntry(entry, configuredCategories);
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
    /// Returns the configured categories in display order.
    /// </summary>
    /// <returns>The configured categories.</returns>
    public IReadOnlyList<CategoryDefinition> GetConfiguredCategories()
    {
        return categoryDefinitionProvider.LoadCategories();
    }

    /// <summary>
    /// Returns the configured budget targets in category order.
    /// </summary>
    /// <returns>The configured category targets.</returns>
    public IReadOnlyList<CategoryBudgetTarget> GetConfiguredBudgetTargets()
    {
        var configuredCategories = categoryDefinitionProvider.LoadCategories();
        var targets = categoryBudgetTargetProvider.LoadTargets();
        ValidateTargetsAgainstConfiguredCategories(configuredCategories, targets);

        return targets
            .OrderBy(target => configuredCategories.First(category =>
                string.Equals(category.Name, target.Category, StringComparison.OrdinalIgnoreCase)).DisplayOrder)
            .ThenBy(target => target.Category)
            .ToList();
    }

    /// <summary>
    /// Updates the monthly target amount for one configured category.
    /// </summary>
    /// <param name="category">The category whose target should change.</param>
    /// <param name="monthlyTarget">The replacement monthly target amount.</param>
    public CategoryBudgetTargetHistoryEntry? UpdateCategoryBudgetTarget(string category, decimal monthlyTarget)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category is required.", nameof(category));
        }

        if (monthlyTarget <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthlyTarget), "Monthly target must be greater than zero.");
        }

        var configuredCategories = categoryDefinitionProvider.LoadCategories();
        var matchingCategory = configuredCategories.FirstOrDefault(item =>
            string.Equals(item.Name, category, StringComparison.OrdinalIgnoreCase));

        if (matchingCategory is null)
        {
            throw new ArgumentException($"Category '{category}' is not configured.", nameof(category));
        }

        var existingTargets = categoryBudgetTargetProvider.LoadTargets().ToList();
        ValidateTargetsAgainstConfiguredCategories(configuredCategories, existingTargets);

        var existingTargetIndex = existingTargets.FindIndex(target =>
            string.Equals(target.Category, matchingCategory.Name, StringComparison.OrdinalIgnoreCase));

        if (existingTargetIndex < 0)
        {
            throw new InvalidOperationException($"Category '{matchingCategory.Name}' does not have a configured budget target to edit.");
        }

        var existingTarget = existingTargets[existingTargetIndex];

        if (existingTarget.MonthlyTarget == monthlyTarget)
        {
            logger.LogInfo("Category budget target unchanged.", new { Category = matchingCategory.Name, monthlyTarget });
            return null;
        }

        existingTargets[existingTargetIndex] = existingTarget with
        {
            MonthlyTarget = monthlyTarget,
            Category = matchingCategory.Name
        };

        categoryBudgetTargetProvider.SaveTargets(existingTargets);
        var historyEntries = categoryBudgetTargetHistoryStore.LoadHistory().ToList();
        var historyEntry = new CategoryBudgetTargetHistoryEntry(
            DateTimeOffset.UtcNow,
            matchingCategory.Name,
            existingTarget.MonthlyTarget,
            monthlyTarget,
            existingTarget.EvaluationMode);
        historyEntries.Add(historyEntry);
        categoryBudgetTargetHistoryStore.SaveHistory(historyEntries);

        logger.LogInfo(
            "Category budget target updated.",
            new
            {
                Category = matchingCategory.Name,
                previousMonthlyTarget = existingTarget.MonthlyTarget,
                updatedMonthlyTarget = monthlyTarget,
                existingTarget.EvaluationMode
            });

        return historyEntry;
    }

    /// <summary>
    /// Returns recent budget-target history entries, optionally filtered by category.
    /// </summary>
    /// <param name="category">The optional category filter.</param>
    /// <param name="limit">The maximum number of entries to return.</param>
    /// <returns>The matching target-change history entries in reverse chronological order.</returns>
    public IReadOnlyList<CategoryBudgetTargetHistoryEntry> GetBudgetTargetHistory(string? category = null, int limit = 20)
    {
        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "History limit must be greater than zero.");
        }

        var historyEntries = categoryBudgetTargetHistoryStore.LoadHistory();

        if (string.IsNullOrWhiteSpace(category))
        {
            return historyEntries
                .OrderByDescending(entry => entry.ChangedAtUtc)
                .Take(limit)
                .ToList();
        }

        var configuredCategories = categoryDefinitionProvider.LoadCategories();

        if (!configuredCategories.Any(item => string.Equals(item.Name, category, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Category '{category}' is not configured.", nameof(category));
        }

        return historyEntries
            .Where(entry => string.Equals(entry.Category, category, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(entry => entry.ChangedAtUtc)
            .Take(limit)
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
        var configuredCategories = categoryDefinitionProvider.LoadCategories();
        var targets = categoryBudgetTargetProvider.LoadTargets();
        ValidateTargetsAgainstConfiguredCategories(configuredCategories, targets);
        return reportBuilder.Build(entries, month, configuredCategories, targets);
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
    /// Previews entries from a CSV file before import.
    /// </summary>
    /// <param name="filePath">The CSV file path to preview.</param>
    /// <returns>The preview result showing new and duplicate rows.</returns>
    public CsvImportPreview PreviewImportFromCsvFile(string filePath)
    {
        var importedEntries = csvImportService.LoadEntries(filePath);
        var configuredCategories = categoryDefinitionProvider.LoadCategories();
        var existingEntries = new HashSet<BudgetEntry>(entries);
        var newEntries = new List<BudgetEntry>();
        var duplicateEntries = new List<BudgetEntry>();

        foreach (var entry in importedEntries)
        {
            ValidateEntry(entry, configuredCategories);

            if (existingEntries.Contains(entry))
            {
                duplicateEntries.Add(entry);
                continue;
            }

            newEntries.Add(entry);
        }

        logger.LogInfo(
            "CSV import preview created.",
            new { filePath, newCount = newEntries.Count, duplicateCount = duplicateEntries.Count });

        return new CsvImportPreview(filePath, newEntries, duplicateEntries);
    }

    /// <summary>
    /// Imports entries from a CSV preview and persists new rows.
    /// </summary>
    /// <param name="preview">The prepared import preview to apply.</param>
    /// <returns>The completed import result.</returns>
    public CsvImportResult ApplyImportPreview(CsvImportPreview preview)
    {
        entries.AddRange(preview.NewEntries);
        budgetEntryStore.SaveEntries(entries);

        logger.LogInfo(
            "CSV import completed.",
            new { filePath = preview.FilePath, importedCount = preview.NewEntries.Count, duplicateCount = preview.DuplicateEntries.Count });

        return new CsvImportResult(preview.FilePath, preview.NewEntries.Count, preview.DuplicateEntries.Count);
    }

    /// <summary>
    /// Imports entries from a CSV file and persists new rows.
    /// </summary>
    /// <param name="filePath">The CSV file path to import.</param>
    /// <returns>The completed import result.</returns>
    public CsvImportResult ImportEntriesFromCsvFile(string filePath)
    {
        var preview = PreviewImportFromCsvFile(filePath);
        return ApplyImportPreview(preview);
    }

    /// <summary>
    /// Validates a budget entry before it is stored.
    /// </summary>
    /// <param name="entry">The entry to validate.</param>
    /// <param name="configuredCategories">The configured categories used for validation.</param>
    private static void ValidateEntry(BudgetEntry entry, IReadOnlyList<CategoryDefinition> configuredCategories)
    {
        if (entry.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(entry), "Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(entry.Description))
        {
            throw new ArgumentException("Description is required.", nameof(entry));
        }

        if (string.IsNullOrWhiteSpace(entry.Category))
        {
            throw new ArgumentException("Category is required.", nameof(entry));
        }

        if (configuredCategories.Count > 0
            && !configuredCategories.Any(category => string.Equals(category.Name, entry.Category, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Category '{entry.Category}' is not configured.", nameof(entry));
        }
    }

    /// <summary>
    /// Validates configured targets against configured categories.
    /// </summary>
    /// <param name="configuredCategories">The configured categories.</param>
    /// <param name="targets">The configured targets.</param>
    private static void ValidateTargetsAgainstConfiguredCategories(
        IReadOnlyList<CategoryDefinition> configuredCategories,
        IReadOnlyList<CategoryBudgetTarget> targets)
    {
        var categoryNames = configuredCategories
            .Select(category => category.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unknownTargetCategories = targets
            .Where(target => !categoryNames.Contains(target.Category))
            .Select(target => target.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (unknownTargetCategories.Count > 0)
        {
            throw new InvalidOperationException(
                $"Budget target configuration contains unknown categories: {string.Join(", ", unknownTargetCategories)}.");
        }
    }
}
