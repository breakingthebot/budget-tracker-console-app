// Abstractions/ICategoryDefinitionProvider.cs
// Defines the contract for retrieving configured budget categories.
// Connects to: Services/BudgetTrackerService.cs, Persistence/JsonCategoryDefinitionProvider.cs
// Created: 2026-07-01

using BudgetTracker.Core.Models;

namespace BudgetTracker.Core.Abstractions;

/// <summary>
/// Retrieves configured budget categories.
/// </summary>
public interface ICategoryDefinitionProvider
{
    /// <summary>
    /// Loads the configured categories.
    /// </summary>
    /// <returns>The configured categories.</returns>
    IReadOnlyList<CategoryDefinition> LoadCategories();
}
