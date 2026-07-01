// Models/CategoryDefinition.cs
// Stores one configured category definition for app menus and validation.
// Connects to: Abstractions/ICategoryDefinitionProvider.cs, Services/BudgetTrackerService.cs, App/ConsoleWorkflow.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Models;

/// <summary>
/// Represents one configured budget category.
/// </summary>
/// <param name="Name">The stable category name used across data files and reports.</param>
/// <param name="DisplayOrder">The menu ordering value for the category.</param>
public sealed record CategoryDefinition(string Name, int DisplayOrder);
