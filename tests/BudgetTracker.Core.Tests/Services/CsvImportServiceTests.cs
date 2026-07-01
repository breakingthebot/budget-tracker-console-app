// Services/CsvImportServiceTests.cs
// Verifies CSV import parsing, header validation, and invalid-row behavior.
// Connects to: Core/Services/CsvImportService.cs, Core/Models/BudgetEntry.cs
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Services;

namespace BudgetTracker.Core.Tests.Services;

/// <summary>
/// Tests the CSV import service.
/// </summary>
[TestClass]
public sealed class CsvImportServiceTests
{
    /// <summary>
    /// Loads valid budget entries from a CSV file.
    /// </summary>
    [TestMethod]
    public void LoadEntries_WithValidCsv_ReturnsEntries()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(
            filePath,
            """
            Date,Category,Description,Amount
            2026-07-10,Food,Groceries,45.50
            2026-07-11,Transportation,"Train pass, monthly",12.00
            """);

        var service = CreateService();

        var entries = service.LoadEntries(filePath);

        Assert.AreEqual(2, entries.Count);
        Assert.AreEqual("Train pass, monthly", entries[1].Description);
    }

    /// <summary>
    /// Rejects a CSV file with the wrong header.
    /// </summary>
    [TestMethod]
    public void LoadEntries_WithWrongHeader_ThrowsInvalidOperationException()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "When,Type,Notes,Value");
        var service = CreateService();

        Assert.Throws<InvalidOperationException>(() => service.LoadEntries(filePath));
    }

    /// <summary>
    /// Rejects invalid row values with a clear exception.
    /// </summary>
    [TestMethod]
    public void LoadEntries_WithInvalidAmount_ThrowsInvalidOperationException()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(
            filePath,
            """
            Date,Category,Description,Amount
            2026-07-10,Food,Groceries,not-a-number
            """);

        var service = CreateService();

        Assert.Throws<InvalidOperationException>(() => service.LoadEntries(filePath));
    }

    /// <summary>
    /// Creates the import service for tests.
    /// </summary>
    /// <returns>A configured CSV import service.</returns>
    private static CsvImportService CreateService()
    {
        return new CsvImportService(new StructuredConsoleLogger());
    }

    /// <summary>
    /// Creates an isolated CSV file path for a test run.
    /// </summary>
    /// <returns>A unique CSV file path inside the temp directory.</returns>
    private static string CreateTempFilePath()
    {
        return Path.Combine(Path.GetTempPath(), "BudgetTrackerImportTests", Guid.NewGuid().ToString("N"), "import.csv");
    }
}
