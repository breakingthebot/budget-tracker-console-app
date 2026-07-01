// Persistence/JsonCategoryBudgetTargetProvider.cs
// Loads monthly category targets from a JSON configuration file.
// Connects to: Abstractions/ICategoryBudgetTargetProvider.cs, Models/CategoryBudgetTarget.cs, Logging/StructuredConsoleLogger.cs
// Created: 2026-07-01

using System.Text.Json;
using BudgetTracker.Core.Abstractions;
using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Persistence;

/// <summary>
/// Loads category targets from a JSON file.
/// </summary>
public sealed class JsonCategoryBudgetTargetProvider : ICategoryBudgetTargetProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string filePath;
    private readonly StructuredConsoleLogger logger;

    /// <summary>
    /// Initializes the JSON target provider.
    /// </summary>
    /// <param name="filePath">The full path to the target configuration file.</param>
    /// <param name="logger">The logger used for configuration diagnostics.</param>
    public JsonCategoryBudgetTargetProvider(string filePath, StructuredConsoleLogger logger)
    {
        this.filePath = filePath;
        this.logger = logger;
    }

    /// <summary>
    /// Loads monthly targets from the JSON configuration file.
    /// </summary>
    /// <returns>The configured targets.</returns>
    public IReadOnlyList<CategoryBudgetTarget> LoadTargets()
    {
        if (!File.Exists(filePath))
        {
            logger.LogWarning("Budget target configuration file is missing. No targets will be applied.", new { filePath });
            return [];
        }

        try
        {
            var content = File.ReadAllText(filePath);
            var targets = JsonSerializer.Deserialize<List<CategoryBudgetTarget>>(content, SerializerOptions) ?? [];

            ValidateTargets(targets);
            logger.LogInfo("Budget targets loaded from configuration.", new { filePath, count = targets.Count });
            return targets;
        }
        catch (JsonException exception)
        {
            logger.LogError("Budget target configuration is invalid JSON.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not parse budget target configuration at '{filePath}'.", exception);
        }
        catch (IOException exception)
        {
            logger.LogError("Budget target configuration could not be read.", new { filePath, exception.Message });
            throw new InvalidOperationException($"Could not read budget target configuration at '{filePath}'.", exception);
        }
    }

    /// <summary>
    /// Validates target configuration values.
    /// </summary>
    /// <param name="targets">The targets to validate.</param>
    private static void ValidateTargets(IReadOnlyList<CategoryBudgetTarget> targets)
    {
        var duplicateCategories = targets
            .GroupBy(target => target.Category, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateCategories.Count > 0)
        {
            throw new InvalidOperationException(
                $"Budget target configuration contains duplicate categories: {string.Join(", ", duplicateCategories)}.");
        }

        if (targets.Any(target => target.MonthlyTarget <= 0))
        {
            throw new InvalidOperationException("Budget target configuration must use positive monthly target values.");
        }

        if (targets.Any(target => string.IsNullOrWhiteSpace(target.Category)))
        {
            throw new InvalidOperationException("Budget target configuration cannot contain blank category names.");
        }

        if (targets.Any(
                target => !string.Equals(target.EvaluationMode, BudgetTargetEvaluationModes.MaxSpend, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(target.EvaluationMode, BudgetTargetEvaluationModes.MinProgress, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Budget target configuration must use '{BudgetTargetEvaluationModes.MaxSpend}' or '{BudgetTargetEvaluationModes.MinProgress}' evaluation modes.");
        }
    }
}
