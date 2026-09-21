#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Shared.Template;

internal static class FileStructureTemplate
{
    public static FileSystemEntry[] Get(
        string configName, string scriptName, string scriptConfigName, string moduleVersion = "", string logPath = "",
        bool configEnabled = false, bool configAutoResolvePaths = false, bool logEnabled = false, bool logShowShellOutput = false) =>
    [
        new TemplateFolder("Config", [
            new TemplateFile($"{configName}.json", "{\n\n}"),
            new TemplateFile($"{configName}_schema.json", ReadEmbeddedResource("properties_schema.json")),
        ]),
        new TemplateFolder("Function", [
            new TemplateFolder("Private"),
            new TemplateFolder("Public")
        ]),
        new TemplateFolder("Module"),
        new TemplateFolder("Test"),
        new TemplateFile(".gitignore", ReadEmbeddedResource(".gitignore")),
        new TemplateFile($"{scriptConfigName}.xml", ReadEmbeddedResource("Bootstrap.xml")
            .Replace("{{ConfigName}}", configName)
            .Replace("{{ModuleVersion}}", moduleVersion)
            .Replace("{{LogPath}}", logPath)
            .Replace("{{ConfigEnabled}}", configEnabled ? "true" : "false")
            .Replace("{{ConfigAutoResolvePaths}}", configAutoResolvePaths ? "true" : "false")
            .Replace("{{LogEnabled}}", logEnabled ? "true" : "false")
            .Replace("{{LogShowShellOutput}}", logShowShellOutput ? "true" : "false")),
        new TemplateFile($"{scriptName}.ps1", ReadEmbeddedResource("Main.ps1")),
    ];

    private static string ReadEmbeddedResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = assembly.GetManifestResourceNames()
            .First(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}