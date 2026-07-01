// Logging/StructuredConsoleLogger.cs
// Writes simple structured JSON logs for console-hosted services.
// Connects to: Logging/LogSeverity.cs, App/Program.cs, App/ConsoleWorkflow.cs, Services/BudgetTrackerService.cs
// Created: 2026-07-01

using System.Text.Json;

namespace BudgetTracker.Core.Logging;

/// <summary>
/// Writes structured log entries to standard output.
/// </summary>
public sealed class StructuredConsoleLogger
{
    /// <summary>
    /// Writes a debug-level log entry.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Optional structured context.</param>
    public void LogDebug(string message, object? context = null) => WriteLog(LogSeverity.Debug, message, context);

    /// <summary>
    /// Writes an info-level log entry.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Optional structured context.</param>
    public void LogInfo(string message, object? context = null) => WriteLog(LogSeverity.Info, message, context);

    /// <summary>
    /// Writes a warning-level log entry.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Optional structured context.</param>
    public void LogWarning(string message, object? context = null) => WriteLog(LogSeverity.Warning, message, context);

    /// <summary>
    /// Writes an error-level log entry.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Optional structured context.</param>
    public void LogError(string message, object? context = null) => WriteLog(LogSeverity.Error, message, context);

    /// <summary>
    /// Serializes and writes a structured log object.
    /// </summary>
    /// <param name="severity">The severity level for the event.</param>
    /// <param name="message">The event message.</param>
    /// <param name="context">Optional structured context.</param>
    private static void WriteLog(LogSeverity severity, string message, object? context)
    {
        var payload = new
        {
            timestampUtc = DateTime.UtcNow,
            severity = severity.ToString().ToUpperInvariant(),
            message,
            context
        };

        Console.WriteLine(JsonSerializer.Serialize(payload));
    }
}
