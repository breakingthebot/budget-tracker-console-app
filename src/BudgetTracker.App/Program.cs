// Program.cs
// Bootstraps the budget tracker console application and CLI entry points.
// Connects to: Configuration/ApplicationVersion.cs, Console/ConsoleWorkflow.cs, Core/Services/*, Core/Logging/*
// Created: 2026-07-01

using BudgetTracker.App.Configuration;
using BudgetTracker.App.Console;
using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Persistence;
using BudgetTracker.Core.Services;

var logger = new StructuredConsoleLogger();

if (args.Contains("--version", StringComparer.OrdinalIgnoreCase))
{
    System.Console.WriteLine(ApplicationVersion.Current);
    return;
}

logger.LogInfo("Starting budget tracker console application.", new { version = ApplicationVersion.Current });

var dataFilePath = DataFilePathProvider.GetBudgetDataFilePath();
logger.LogInfo("Resolved budget data file path.", new { dataFilePath });

var budgetTrackerService = new BudgetTrackerService(
    new JsonBudgetEntryStore(dataFilePath, logger),
    new MonthlyReportBuilder(),
    new CsvExportService(),
    logger);

var workflow = new ConsoleWorkflow(budgetTrackerService, logger, dataFilePath);
workflow.Run();

logger.LogInfo("Budget tracker console application stopped.");
