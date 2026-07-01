// Configuration/ApplicationVersion.cs
// Centralizes the application version string for CLI output.
// Connects to: Program.cs
// Created: 2026-07-01

namespace BudgetTracker.App.Configuration;

/// <summary>
/// Holds the current application version string.
/// </summary>
public static class ApplicationVersion
{
    /// <summary>
    /// Gets the current application version.
    /// </summary>
    public const string Current = "0.2.0";
}
