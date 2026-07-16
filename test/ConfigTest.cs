using System.Runtime.InteropServices;
using PSBootstrap.Service;
using PSBootstrap.Shared.Entity;
using PSBootstrap.Shared.Exception;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Template;
using PSBootstrap.Shared.Value_object;

namespace Test;

[TestClass]
public class ConfigTest
{
    private IConfigService? _configService;
    private IFileStructureService? _fileStructureService;
    private string _rootPath = string.Empty;
    private JsonConfigPath? _jsonConfigPath;
    private JsonSchemaPath? _jsonSchemaPath;
    private FileSystemEntry[] _fileStructure = FileStructureTemplate.Get("properties", "TestScript", "Bootstrap");

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        // Each test gets its own directory so parallel test execution can't race on shared files.
        _rootPath = Path.Combine(Directory.GetCurrentDirectory(), TestContext.TestName!);
        _fileStructureService = new FileStructureService();
        _fileStructureService?.Create(_rootPath, _fileStructure);

        _jsonConfigPath = new(Path.Combine(_rootPath, "Config/properties.json"));
        _jsonSchemaPath = new(Path.Combine(_rootPath, "Config/properties_schema.json"));

        _configService = new ConfigService(_jsonConfigPath);
    }

    [TestMethod]
    public void TestConfig()
    {
        // Store the actual and false properties and required fields for the JSON schema
        string actualProperties = "\"properties\": {},";
        string falseProperties = "\"properties\": {\"Log\": {\"type\": \"string\"}, \"Module\": {\"type\": \"string\"}, \"Function\": {\"type\": \"string\"}},";
        string actualRequired = "\"required\": [],";
        string falseRequired = "\"required\": [\"Log\", \"Module\", \"Function\"],";

        // Test if an exception is thrown when the JSON content does not conform to the schema
        Assert.IsNotNull(_configService, "Config service should not be null.");
        File.WriteAllText(_jsonSchemaPath?.ToString() ?? string.Empty, File.ReadAllText(_jsonSchemaPath?.ToString() ?? string.Empty).Replace(actualProperties, falseProperties));
        File.WriteAllText(_jsonSchemaPath?.ToString() ?? string.Empty, File.ReadAllText(_jsonSchemaPath?.ToString() ?? string.Empty).Replace(actualRequired, falseRequired));
        Assert.ThrowsException<BootstrapException>(() => _configService?.Convert(_jsonSchemaPath), "Config service should throw an exception when the JSON content does not conform to the schema.");

        // Return the JSON schema to its original state and test if the Config object is created successfully
        File.WriteAllText(_jsonSchemaPath?.ToString() ?? string.Empty, File.ReadAllText(_jsonSchemaPath?.ToString() ?? string.Empty).Replace(falseProperties, actualProperties));
        File.WriteAllText(_jsonSchemaPath?.ToString() ?? string.Empty, File.ReadAllText(_jsonSchemaPath?.ToString() ?? string.Empty).Replace(falseRequired, actualRequired));
        object? config = _configService?.Convert(_jsonSchemaPath);
        Assert.IsNotNull(config, "Config should not be null.");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _fileStructureService?.Delete(_rootPath, _fileStructure);
    }
}