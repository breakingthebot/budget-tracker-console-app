// Persistence/JsonBudgetEntryStore.cs
// Loads and saves budget entries from a JSON file on disk.
// Connects to: Abstractions/IBudgetEntryStore.cs, Models/BudgetEntry.cs, Logging/StructuredConsoleLogger.cs
// Created: 2026-07-01

using System.Text.Json;
using BudgetTracker.Core.Abstractions;
using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Persistence;

/// <summary>
/// Stores budget entries in a JSON file.
/// </summary>
public sealed class JsonBudgetEntryStore : IBudgetEntryStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string filePath;
    private readonly StructuredConsoleLogger logger;

    /// <summary>
    /// Initializes the JSON-backed entry store.
    /// </summary>
    /// <param name="filePath">The full path to the JSON data file.</param>
    /// <param name="logger">The logger used for storage diagnostics.</param>
    public JsonBudgetEntryStore(string filePath, StructuredConsoleLogger logger)
    {
        this.filePath = filePath;
        this.logger = logger;
    }

    /// <summary>
    /// Loads entries from the JSON file.
    /// </summary>
    /// <returns>The stored entries, or an empty list when the file does not exist.</returns>
    public IReadOnlyList<BudgetEntry> LoadEntries()
    {
        if (!File.Exists(filePath))
        {
            logger.LogInfo("Budget data file does not exist yet.", new { filePath });
            return [];
        }

        try
        {
            var content = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(content))
            {
                logger.LogWarning("Budget data file is empty. Treating it as no entries.", new { filePath });
                return [];
            }

            var entries = JsonSerializer.Deserialize<List<BudgetEntry>>(content, SerializerOptions) ?? [];
            logger.LogInfo("Budget entries loaded from disk.", new { filePath, count = entries.Count });
            return entries;
        }
        catch (JsonException exception)
        {
            logger.LogError("Budget data file is invalid JSON.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not parse budget data file at '{filePath}'.", exception);
        }
        catch (IOException exception)
        {
            logger.LogError("Budget data file could not be read.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not read budget data file at '{filePath}'.", exception);
        }
    }

    /// <summary>
    /// Saves entries to the JSON file.
    /// </summary>
    /// <param name="entries">The entries to persist.</param>
    public void SaveEntries(IReadOnlyList<BudgetEntry> entries)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var content = JsonSerializer.Serialize(entries, SerializerOptions);
            File.WriteAllText(filePath, content);
            logger.LogInfo("Budget entries saved to disk.", new { filePath, count = entries.Count });
        }
        catch (IOException exception)
        {
            logger.LogError("Budget data file could not be written.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not write budget data file at '{filePath}'.", exception);
        }
    }
}
