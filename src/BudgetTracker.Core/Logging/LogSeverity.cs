// Logging/LogSeverity.cs
// Defines supported structured log severity levels.
// Connects to: Logging/StructuredConsoleLogger.cs
// Created: 2026-07-01

namespace BudgetTracker.Core.Logging;

/// <summary>
/// Represents the severity level for application logs.
/// </summary>
public enum LogSeverity
{
    Debug,
    Info,
    Warning,
    Error
}
