using System.Runtime.InteropServices;
using PSBootstrap.Service;
using PSBootstrap.Shared.Entity;
using PSBootstrap.Shared.Exception;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Template;
using PSBootstrap.Shared.Value_object;

namespace Test;

[TestClass]
public class ScriptConfigTest
{
    private IScriptConfigService? _scriptConfigService;
    private IFileStructureService? _fileStructureService;
    private string _rootPath = string.Empty;
    private string _xmlFilePath = string.Empty;
    private FileSystemEntry[] _fileStructure = FileStructureTemplate.Get("properties", "TestScript", "Bootstrap");

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        // Each test gets its own directory so parallel test execution can't race on shared files.
        _rootPath = Path.Combine(Directory.GetCurrentDirectory(), TestContext.TestName!);
        _xmlFilePath = Path.Combine(_rootPath, "Bootstrap.xml");
        _fileStructureService = new FileStructureService();
        _fileStructureService.Create(_rootPath, _fileStructure);
        _scriptConfigService = new ScriptConfigService(_xmlFilePath);
    }

    [TestMethod]
    public void TestScriptConfig()
    {
        // Set the correct and incorrect log paths based on the operating system
        string wrongLogPath = "/home/user/logs/log"; // No file extension provided
        string correctLogPath = "/home/user/logs/log.txt"; // File extension provided
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            wrongLogPath = "C:\\Logs\\log"; // No file extension provided
            correctLogPath = "C:\\Logs\\log.txt"; // File extension provided
        }

        // Test if an exception is thrown when the log path does not have a file extension
        File.WriteAllText(_xmlFilePath, File.ReadAllText(_xmlFilePath).Replace("<Path Value=\"\"/>", $"<Path Value=\"{wrongLogPath}\"/>"));
        Assert.ThrowsException<BootstrapException>(() => _scriptConfigService?.Convert(), "Log.Path should throw an exception when no file extension is provided.");

        // Set the correct log path and test if the ScriptConfig object is created successfully
        File.WriteAllText(_xmlFilePath, File.ReadAllText(_xmlFilePath).Replace($"<Path Value=\"{wrongLogPath}\"/>", $"<Path Value=\"{correctLogPath}\"/>"));
        ScriptConfig? scriptConfig = _scriptConfigService?.Convert();
        Assert.IsNotNull(scriptConfig, "ScriptConfig should not be null.");

        // Check if the boolean properties are set correctly
        Assert.IsTrue(scriptConfig?.LogEnabled, "Log should be enabled at first run.");
        Assert.IsFalse(scriptConfig?.Verbose, "Verbose should be disabled at first run.");
        Assert.IsFalse(scriptConfig?.Debug, "Debug should be disabled at first run.");

        // Check if the paths are resolved correctly
        Assert.AreEqual(correctLogPath, scriptConfig?.LogPath?.Path, $"Log path should be '{correctLogPath}'.");
        Assert.AreEqual(Path.Combine(_rootPath, "Module"), scriptConfig?.ModulePath?.Path, $"Module path should be '{Path.Combine(_rootPath, "Module")}'.");
        Assert.AreEqual(Path.Combine(_rootPath, "Function"), scriptConfig?.FunctionPath?.Path, $"Function path should be '{Path.Combine(_rootPath, "Function")}'.");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _fileStructureService?.Delete(_rootPath, _fileStructure);
    }
}