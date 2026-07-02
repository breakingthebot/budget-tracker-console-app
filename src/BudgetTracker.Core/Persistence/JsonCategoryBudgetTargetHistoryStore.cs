// Persistence/JsonCategoryBudgetTargetHistoryStore.cs
// Loads and saves budget-target audit history in a JSON file.
// Connects to: Abstractions/ICategoryBudgetTargetHistoryStore.cs, Models/CategoryBudgetTargetHistoryEntry.cs, Logging/StructuredConsoleLogger.cs
// Created: 2026-07-01

using System.Text.Json;
using BudgetTracker.Core.Abstractions;
using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Persistence;

/// <summary>
/// Loads and saves budget-target history entries from JSON.
/// </summary>
public sealed class JsonCategoryBudgetTargetHistoryStore : ICategoryBudgetTargetHistoryStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string filePath;
    private readonly StructuredConsoleLogger logger;

    /// <summary>
    /// Initializes the JSON-backed history store.
    /// </summary>
    /// <param name="filePath">The full history file path.</param>
    /// <param name="logger">The logger used for persistence diagnostics.</param>
    public JsonCategoryBudgetTargetHistoryStore(string filePath, StructuredConsoleLogger logger)
    {
        this.filePath = filePath;
        this.logger = logger;
    }

    /// <summary>
    /// Loads previously recorded budget-target changes.
    /// </summary>
    /// <returns>The stored history entries.</returns>
    public IReadOnlyList<CategoryBudgetTargetHistoryEntry> LoadHistory()
    {
        if (!File.Exists(filePath))
        {
            logger.LogInfo("Budget target history file is missing. Starting with empty history.", new { filePath });
            return [];
        }

        try
        {
            var content = File.ReadAllText(filePath);
            var entries = JsonSerializer.Deserialize<List<CategoryBudgetTargetHistoryEntry>>(content, SerializerOptions) ?? [];

            ValidateHistory(entries);
            logger.LogInfo("Budget target history loaded from disk.", new { filePath, count = entries.Count });
            return entries;
        }
        catch (JsonException exception)
        {
            logger.LogError("Budget target history file is invalid JSON.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not parse budget target history at '{filePath}'.", exception);
        }
        catch (IOException exception)
        {
            logger.LogError("Budget target history file could not be read.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not read budget target history at '{filePath}'.", exception);
        }
    }

    /// <summary>
    /// Saves the budget-target history snapshot.
    /// </summary>
    /// <param name="entries">The entries to persist.</param>
    public void SaveHistory(IReadOnlyList<CategoryBudgetTargetHistoryEntry> entries)
    {
        ValidateHistory(entries);

        try
        {
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var content = JsonSerializer.Serialize(entries, new JsonSerializerOptions(SerializerOptions)
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, content);
            logger.LogInfo("Budget target history saved to disk.", new { filePath, count = entries.Count });
        }
        catch (IOException exception)
        {
            logger.LogError("Budget target history file could not be written.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not write budget target history at '{filePath}'.", exception);
        }
    }

    /// <summary>
    /// Validates the shape of persisted history entries.
    /// </summary>
    /// <param name="entries">The entries to validate.</param>
    private static void ValidateHistory(IReadOnlyList<CategoryBudgetTargetHistoryEntry> entries)
    {
        if (entries.Any(entry => string.IsNullOrWhiteSpace(entry.Category)))
        {
            throw new InvalidOperationException("Budget target history cannot contain blank category names.");
        }

        if (entries.Any(entry => entry.PreviousMonthlyTarget <= 0 || entry.UpdatedMonthlyTarget <= 0))
        {
            throw new InvalidOperationException("Budget target history must use positive monthly target values.");
        }

        if (entries.Any(
                entry => !string.Equals(entry.EvaluationMode, BudgetTargetEvaluationModes.MaxSpend, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(entry.EvaluationMode, BudgetTargetEvaluationModes.MinProgress, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Budget target history must use '{BudgetTargetEvaluationModes.MaxSpend}' or '{BudgetTargetEvaluationModes.MinProgress}' evaluation modes.");
        }
    }
}
