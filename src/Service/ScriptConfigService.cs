#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using PSBootstrap.Shared.Entity;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Service;

internal class ScriptConfigService(string xmlPath) : IScriptConfigService
{
    private readonly string _xmlPath = xmlPath;

    // Converts the XML configuration file into a ScriptConfig object so its properties can be used by the cmdlets.
    public ScriptConfig Convert()
    {
        // Load the XML configuration file and get the root element.
        XDocument xml = XDocument.Load(_xmlPath);
        XElement root = xml.Root;

        // Helper function to get the value of an element's "Value" attribute, or return an empty string if the element or attribute is not found.
        static string GetValue(XElement parent, string elementName) =>
            parent.Element(elementName)?.Attribute("Value")?.Value ?? string.Empty;

        // Helper function to resolve a relative path to an absolute path based on the base directory of the XML file.
        static string ResolvePath(string baseDirectory, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (Path.IsPathRooted(value))
            {
                return value;
            }

            string normalized = value.Replace('\\', Path.DirectorySeparatorChar)
                                     .Replace('/', Path.DirectorySeparatorChar);

            return Path.IsPathRooted(normalized)
                ? normalized
                : Path.GetFullPath(Path.Combine(baseDirectory, normalized));
        }

        // Get the base directory of the XML file to resolve relative paths.
        string baseDirectory = Path.GetDirectoryName(_xmlPath)!;

        // Find the config sections that correspond to the different parts of the configuration
        XElement config = root.Element("Config");
        XElement function = root.Element("Function");
        XElement module = root.Element("Module");
        XElement log = root.Element("Log");

        // Get the expected version of this Bootstrap module if specified
        Version? bootstrapVersion = Version.TryParse(GetValue(root, "BootstrapVersion"), out var parsedVersion) ? parsedVersion : null;

        // Get the configuration file name, root path, and schema path if logging is enabled
        string? configFileName = null;
        ConfigPath? configPath = null;
        JsonSchemaPath? schemaPath = null;
        if (GetValue(config, "Enabled") == "true")
        {
            configFileName = GetValue(config, "FileName");
            string configPathValue = GetValue(config, "RootPath");
            configPath = new(!string.IsNullOrWhiteSpace(configPathValue) ? ResolvePath(baseDirectory, configPathValue) : string.Empty);
            string schemaPathValue = GetValue(config, "SchemaPath");
            schemaPath = !string.IsNullOrWhiteSpace(schemaPathValue) ? new(ResolvePath(baseDirectory, GetValue(config, "SchemaPath"))) : null;
        }

        // Get the function root path and the list of required functions
        FunctionPath functionPath = new(ResolvePath(baseDirectory, GetValue(function, "RootPath")));
        string[] functions = function.Elements("Required")
            .Elements("Item")
            .Select(item => item.Attribute("Value")?.Value ?? string.Empty)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToArray() ?? [];

        // Get the module root path and the list of required modules with their optional versions
        ModulePath modulePath = new(ResolvePath(baseDirectory, GetValue(module, "RootPath")));
        List<RequiredModule> modules = module.Element("Required")?
            .Elements("Item")
            .Where(item => !string.IsNullOrWhiteSpace(item.Attribute("Value")?.Value))
            .Select(item =>
            {
                string name = item.Attribute("Value")!.Value;
                string? versionString = item.Attribute("Version")?.Value;
                Version? version = !string.IsNullOrWhiteSpace(versionString) && Version.TryParse(versionString, out var parsed)
                    ? parsed : null;
                return new RequiredModule(name, version);
            })
            .ToList() ?? [];

        // Get the logging settings, including whether logging is enabled, the log file path, and whether to show shell output
        bool logEnabled = GetValue(log, "Enabled") == "true";
        LogPath? logPath = null;
        if (logEnabled)
        {
            string logPathValue = GetValue(log, "Path");
            if (!string.IsNullOrWhiteSpace(logPathValue))
            {
                logPath = new(ResolvePath(baseDirectory, logPathValue));
            }
        }
        bool showShellOutput = bool.Parse(GetValue(root.Element("Log"), "ShowShellOutput"));
        
        // Get the verbose and debug settings
        bool verbose = bool.Parse(GetValue(root.Element("Verbose"), "Enabled"));
        bool debug = bool.Parse(GetValue(root.Element("Debug"), "Enabled"));

        return new ScriptConfig(
            bootstrapVersion, configFileName, configPath, schemaPath, functionPath, functions,
            modulePath, modules, logEnabled, logPath, showShellOutput, verbose, debug
        );
    }
}