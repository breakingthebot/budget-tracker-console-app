// Services/CsvExportFileServiceTests.cs
// Verifies CSV file writing, overwrite protection, and path validation behavior.
// Connects to: Core/Services/CsvExportFileService.cs, Core/Models/CsvExportResult.cs
// Created: 2026-07-01

using BudgetTracker.Core.Logging;
using BudgetTracker.Core.Services;

namespace BudgetTracker.Core.Tests.Services;

/// <summary>
/// Tests the CSV export file service.
/// </summary>
[TestClass]
public sealed class CsvExportFileServiceTests
{
    /// <summary>
    /// Writes CSV content to a new file.
    /// </summary>
    [TestMethod]
    public void WriteToFile_CreatesCsvFile()
    {
        var filePath = CreateTempFilePath();
        var service = CreateService();

        var result = service.WriteToFile(filePath, "Date,Category,Description,Amount\n", 0, overwriteExisting: false);

        Assert.AreEqual(filePath, result.FilePath);
        Assert.IsTrue(File.Exists(filePath));
    }

    /// <summary>
    /// Rejects non-CSV file paths.
    /// </summary>
    [TestMethod]
    public void WriteToFile_WithNonCsvExtension_ThrowsInvalidOperationException()
    {
        var filePath = Path.ChangeExtension(CreateTempFilePath(), ".txt");
        var service = CreateService();

        Assert.Throws<InvalidOperationException>(
            () => service.WriteToFile(filePath, "content", 1, overwriteExisting: false));
    }

    /// <summary>
    /// Rejects existing files when overwrite is disabled.
    /// </summary>
    [TestMethod]
    public void WriteToFile_WithExistingFileAndNoOverwrite_ThrowsInvalidOperationException()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "existing");
        var service = CreateService();

        Assert.Throws<InvalidOperationException>(
            () => service.WriteToFile(filePath, "replacement", 1, overwriteExisting: false));
    }

    /// <summary>
    /// Creates the file export service for tests.
    /// </summary>
    /// <returns>A configured CSV file export service.</returns>
    private static CsvExportFileService CreateService()
    {
        return new CsvExportFileService(new StructuredConsoleLogger());
    }

    /// <summary>
    /// Creates an isolated CSV file path for a test run.
    /// </summary>
    /// <returns>A unique CSV file path inside the temp directory.</returns>
    private static string CreateTempFilePath()
    {
        return Path.Combine(Path.GetTempPath(), "BudgetTrackerCsvTests", Guid.NewGuid().ToString("N"), "export.csv");
    }
}
