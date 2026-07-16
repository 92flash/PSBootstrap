using PSBootstrap.Service;
using PSBootstrap.Shared.Entity;
using PSBootstrap.Shared.Enum;
using PSBootstrap.Shared.Exception;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Template;
using PSBootstrap.Shared.Value_object;

namespace Test;

[TestClass]
public class LogTest
{
    private ILogService? _logService;
    private string? _logFilePath;
    private readonly string _expectedLogEntry = "This is a test log entry.";

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        string currentPath = Directory.GetCurrentDirectory();
        // Each test gets its own file so parallel test execution can't race on shared state.
        _logFilePath = Path.Combine(currentPath, $"{TestContext.TestName}.log");
        _logService = new LogService(new LogPath(_logFilePath));
        _logService.Clear(); // Ensure the log file is empty before each test
    }

    [TestMethod]
    public void TestFileGetsCreatedAndHeaderAndLogEntryAreAppended()
    {
        // Create the log file, append the header if it doesn't exist, and append the log entry
        _logService?.Append(_expectedLogEntry, LogType.Information, true);

        // Create a usable object so it's easier to assert the values of the log entry
        Log? log = _logService?.Get(LogTemplate.HeaderMatch, LogTemplate.LogEntryMatch)
            .FirstOrDefault(l => l.Message == _expectedLogEntry);

        // Assert
        Assert.IsNotNull(log, "Log entry was not found in the log file.");
        Assert.AreEqual(_expectedLogEntry, log?.Message);
        Assert.AreEqual(LogType.Information, log?.LogType);
        Assert.AreEqual(Environment.MachineName, log?.ComputerName);
        Assert.AreEqual(Environment.UserName, log?.Username);
    }

    [TestMethod]
    public void TestLogFileIsCleared()
    {
        // Arrange
        _logService?.Append(_expectedLogEntry, LogType.Information, true);

        // Act
        _logService?.Clear();

        // Assert
        var logs = _logService?.Get(LogTemplate.HeaderMatch, LogTemplate.LogEntryMatch);
        Assert.IsFalse(logs?.Count != 0, "Log file was not cleared.");
    }

    [TestMethod]
    public void TestLogFileIsDeleted()
    {
        // Arrange
        _logService?.Append(_expectedLogEntry, LogType.Information, true);

        // Act
        _logService?.Delete();

        // Assert
        Assert.IsFalse(File.Exists(_logFilePath), "Log file was not deleted.");
        Assert.ThrowsException<BootstrapException>(() => _logService?.Get(LogTemplate.HeaderMatch, LogTemplate.LogEntryMatch), "Expected a BootstrapException when trying to read a deleted log file.");
    }

    [TestMethod]
    public void TestLogFileIsReopenedAfterClose()
    {
        // Create the log and close
        _logService?.Append(_expectedLogEntry, LogType.Information, true);
        _logService?.Close();

        // Expect an exception when trying to append to a closed log file
        Assert.ThrowsException<BootstrapException>(() => _logService?.Append(_expectedLogEntry, LogType.Information, true), "Cannot reopen the closed filestream. Please create a new object.");
    }

    [TestCleanup]
    public void TearDown()
    {
        _logService?.Dispose();
        if (_logFilePath != null && File.Exists(_logFilePath))
        {
            File.Delete(_logFilePath);
        }
    }
}