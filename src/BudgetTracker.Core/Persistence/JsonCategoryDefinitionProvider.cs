// Persistence/JsonCategoryDefinitionProvider.cs
// Loads budget categories from a JSON configuration file.
// Connects to: Abstractions/ICategoryDefinitionProvider.cs, Models/CategoryDefinition.cs, Logging/StructuredConsoleLogger.cs
// Created: 2026-07-01

using System.Text.Json;
using BudgetTracker.Core.Abstractions;
using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Persistence;

/// <summary>
/// Loads configured categories from a JSON file.
/// </summary>
public sealed class JsonCategoryDefinitionProvider : ICategoryDefinitionProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string filePath;
    private readonly StructuredConsoleLogger logger;

    /// <summary>
    /// Initializes the JSON category provider.
    /// </summary>
    /// <param name="filePath">The full path to the category configuration file.</param>
    /// <param name="logger">The logger used for configuration diagnostics.</param>
    public JsonCategoryDefinitionProvider(string filePath, StructuredConsoleLogger logger)
    {
        this.filePath = filePath;
        this.logger = logger;
    }

    /// <summary>
    /// Loads configured categories from disk.
    /// </summary>
    /// <returns>The configured categories.</returns>
    public IReadOnlyList<CategoryDefinition> LoadCategories()
    {
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException($"Category configuration file was not found at '{filePath}'.");
        }

        try
        {
            var content = File.ReadAllText(filePath);
            var categories = JsonSerializer.Deserialize<List<CategoryDefinition>>(content, SerializerOptions) ?? [];

            ValidateCategories(categories);
            logger.LogInfo("Budget categories loaded from configuration.", new { filePath, count = categories.Count });
            return categories.OrderBy(category => category.DisplayOrder).ThenBy(category => category.Name).ToList();
        }
        catch (JsonException exception)
        {
            logger.LogError("Category configuration is invalid JSON.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not parse category configuration at '{filePath}'.", exception);
        }
        catch (IOException exception)
        {
            logger.LogError("Category configuration could not be read.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not read category configuration at '{filePath}'.", exception);
        }
    }

    /// <summary>
    /// Validates configured categories before use.
    /// </summary>
    /// <param name="categories">The categories to validate.</param>
    private static void ValidateCategories(IReadOnlyList<CategoryDefinition> categories)
    {
        if (categories.Count == 0)
        {
            throw new InvalidOperationException("Category configuration must define at least one category.");
        }

        if (categories.Any(category => string.IsNullOrWhiteSpace(category.Name)))
        {
            throw new InvalidOperationException("Category configuration cannot contain blank category names.");
        }

        if (categories.Any(category => category.DisplayOrder <= 0))
        {
            throw new InvalidOperationException("Category configuration must use positive display order values.");
        }

        var duplicateNames = categories
            .GroupBy(category => category.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateNames.Count > 0)
        {
            throw new InvalidOperationException(
                $"Category configuration contains duplicate names: {string.Join(", ", duplicateNames)}.");
        }
    }
}
