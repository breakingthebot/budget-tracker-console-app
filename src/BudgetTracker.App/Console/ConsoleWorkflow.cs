// Console/ConsoleWorkflow.cs
// Handles interactive console input and delegates budget operations to the core service.
// Connects to: Core/Services/BudgetTrackerService.cs, Core/Logging/StructuredConsoleLogger.cs, Core/Models/*
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Models;
using BudgetTracker.Core.Services;

namespace BudgetTracker.App.Console;

/// <summary>
/// Runs the interactive console workflow for the app.
/// </summary>
public sealed class ConsoleWorkflow
{
    private readonly BudgetTrackerService budgetTrackerService;
    private readonly StructuredConsoleLogger logger;
    private readonly string dataFilePath;

    /// <summary>
    /// Initializes the console workflow.
    /// </summary>
    /// <param name="budgetTrackerService">The budgeting application service.</param>
    /// <param name="logger">The shared application logger.</param>
    /// <param name="dataFilePath">The data file path shown to the user.</param>
    public ConsoleWorkflow(BudgetTrackerService budgetTrackerService, StructuredConsoleLogger logger, string dataFilePath)
    {
        this.budgetTrackerService = budgetTrackerService;
        this.logger = logger;
        this.dataFilePath = dataFilePath;
    }

    /// <summary>
    /// Starts the interactive menu loop.
    /// </summary>
    public void Run()
    {
        var shouldContinue = true;

        while (shouldContinue)
        {
            RenderSessionStatus();
            RenderMenu();
            var choice = ReadRequiredString("Select an option");

            try
            {
                shouldContinue = choice.Trim() switch
                {
                    "1" => RunAddEntry(),
                    "2" => RunMonthlyReport(),
                    "3" => RunCsvExport(),
                    "4" => RunListEntries(),
                    "5" => false,
                    _ => HandleUnknownOption()
                };
            }
            catch (Exception exception)
            {
                logger.LogError("Console workflow action failed.", new { exception.Message });
                WriteLine($"Error: {exception.Message}");
            }

            if (shouldContinue)
            {
                WriteLine(string.Empty);
                WriteLine("Press Enter to continue.");
                System.Console.ReadLine();
            }
        }
    }

    /// <summary>
    /// Adds a new entry from user input.
    /// </summary>
    /// <returns>True to continue the menu loop.</returns>
    private bool RunAddEntry()
    {
        var date = ReadDate("Enter transaction date (yyyy-mm-dd)");
        var category = ReadCategory();
        var description = ReadRequiredString("Enter a short description");
        var amount = ReadPositiveAmount("Enter the amount");

        budgetTrackerService.AddEntry(new BudgetEntry(date, category, description, amount));
        WriteLine("Entry added.");
        return true;
    }

    /// <summary>
    /// Shows a monthly report.
    /// </summary>
    /// <returns>True to continue the menu loop.</returns>
    private bool RunMonthlyReport()
    {
        var month = ReadMonth("Enter report month (yyyy-mm)");
        var report = budgetTrackerService.GetMonthlyReport(month);

        WriteLine($"Monthly report for {report.Month:yyyy-MM}");
        WriteLine($"Entries: {report.EntryCount}");
        WriteLine($"Total spent: ${report.TotalSpent:0.00}");

        if (report.CategoryBreakdown.Count == 0)
        {
            WriteLine("No entries found for that month.");
            return true;
        }

        foreach (var item in report.CategoryBreakdown)
        {
            WriteLine($"- {item.Category}: ${item.Total:0.00}");
        }

        return true;
    }

    /// <summary>
    /// Prints a CSV export for the requested month.
    /// </summary>
    /// <returns>True to continue the menu loop.</returns>
    private bool RunCsvExport()
    {
        var month = ReadMonth("Enter export month (yyyy-mm)");
        var csv = budgetTrackerService.ExportMonthToCsv(month);
        WriteLine("CSV export:");
        WriteLine(csv);
        return true;
    }

    /// <summary>
    /// Displays all tracked entries.
    /// </summary>
    /// <returns>True to continue the menu loop.</returns>
    private bool RunListEntries()
    {
        var entries = budgetTrackerService.GetEntries();

        if (entries.Count == 0)
        {
            WriteLine("No entries tracked yet.");
            return true;
        }

        foreach (var entry in entries)
        {
            WriteLine($"{entry.Date:yyyy-MM-dd} | {entry.Category} | {entry.Description} | ${entry.Amount:0.00}");
        }

        return true;
    }

    /// <summary>
    /// Handles invalid menu input.
    /// </summary>
    /// <returns>True to continue the menu loop.</returns>
    private static bool HandleUnknownOption()
    {
        WriteLine("Unknown option. Choose 1 through 5.");
        return true;
    }

    /// <summary>
    /// Renders the top-level menu.
    /// </summary>
    private static void RenderMenu()
    {
        WriteLine("Budget Tracker");
        WriteLine("1. Add expense");
        WriteLine("2. View monthly report");
        WriteLine("3. Export month to CSV");
        WriteLine("4. List all entries");
        WriteLine("5. Exit");
    }

    /// <summary>
    /// Writes the current storage path and loaded entry count.
    /// </summary>
    private void RenderSessionStatus()
    {
        WriteLine($"Data file: {dataFilePath}");
        WriteLine($"Tracked entries: {budgetTrackerService.GetEntries().Count}");
    }

    /// <summary>
    /// Reads a required string value from the console.
    /// </summary>
    /// <param name="prompt">The prompt shown to the user.</param>
    /// <returns>A non-empty value.</returns>
    private static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Write($"{prompt}: ");
            var input = System.Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            WriteLine("A value is required.");
        }
    }

    /// <summary>
    /// Reads a positive decimal amount.
    /// </summary>
    /// <param name="prompt">The prompt shown to the user.</param>
    /// <returns>A positive decimal amount.</returns>
    private static decimal ReadPositiveAmount(string prompt)
    {
        while (true)
        {
            Write($"{prompt}: ");
            var input = System.Console.ReadLine();

            if (decimal.TryParse(input, out var amount) && amount > 0)
            {
                return amount;
            }

            WriteLine("Enter a valid amount greater than zero.");
        }
    }

    /// <summary>
    /// Reads an ISO date from user input.
    /// </summary>
    /// <param name="prompt">The prompt shown to the user.</param>
    /// <returns>The parsed date value.</returns>
    private static DateOnly ReadDate(string prompt)
    {
        while (true)
        {
            Write($"{prompt}: ");
            var input = System.Console.ReadLine();

            if (DateOnly.TryParse(input, out var date))
            {
                return date;
            }

            WriteLine("Enter a valid date in yyyy-mm-dd format.");
        }
    }

    /// <summary>
    /// Reads a year-month value and normalizes it to the first day of the month.
    /// </summary>
    /// <param name="prompt">The prompt shown to the user.</param>
    /// <returns>The normalized month value.</returns>
    private static DateOnly ReadMonth(string prompt)
    {
        while (true)
        {
            Write($"{prompt}: ");
            var input = System.Console.ReadLine();
            var parts = input?.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (parts is [var yearText, var monthText]
                && int.TryParse(yearText, out var year)
                && int.TryParse(monthText, out var month)
                && year >= 2000
                && month is >= 1 and <= 12)
            {
                return new DateOnly(year, month, 1);
            }

            WriteLine("Enter a valid month in yyyy-mm format.");
        }
    }

    /// <summary>
    /// Reads a category selection from the user.
    /// </summary>
    /// <returns>The chosen category.</returns>
    private static BudgetCategory ReadCategory()
    {
        WriteLine("Available categories:");

        foreach (var category in Enum.GetValues<BudgetCategory>())
        {
            WriteLine($"{(int)category}. {category}");
        }

        while (true)
        {
            Write("Select a category number: ");
            var input = System.Console.ReadLine();

            if (int.TryParse(input, out var categoryNumber)
                && Enum.IsDefined(typeof(BudgetCategory), categoryNumber))
            {
                return (BudgetCategory)categoryNumber;
            }

            WriteLine("Select one of the listed category numbers.");
        }
    }

    /// <summary>
    /// Writes a line to the console.
    /// </summary>
    /// <param name="message">The message to write.</param>
    private static void WriteLine(string message) => System.Console.WriteLine(message);

    /// <summary>
    /// Writes text without a trailing newline.
    /// </summary>
    /// <param name="message">The message to write.</param>
    private static void Write(string message) => System.Console.Write(message);
}
