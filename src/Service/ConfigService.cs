#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using PSBootstrap.Shared.Exception;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Service;

public class ConfigService : IConfigService
{
    public Collection<PSObject>? Config { get; private set; }

    // Convert the JSON configuration file to a PowerShell object and optionally validate it against a JSON schema
    public Collection<PSObject>? ConvertFromPath(JsonConfigPath jsonConfigPath, JsonSchemaPath? schemaPath = null, string? relativePathReplacement = null)
    {
        string jsonContent = File.ReadAllText(jsonConfigPath.ToString());
        return Convert(jsonContent, schemaPath, relativePathReplacement);
    }

    // Recursively walks a PowerShell object's properties, replacing relative-path-looking string values (e.g. ".\..." or "./...") with full paths
    private static void ReplaceRelativePaths(PSObject psObject, string relativePathReplacement)
    {
        foreach (PSPropertyInfo property in psObject.Properties)
        {
            property.Value = ReplaceRelativePathValue(property.Value, relativePathReplacement);
        }
    }

    private static object? ReplaceRelativePathValue(object? value, string relativePathReplacement)
    {
        switch (value)
        {
            case string text when text.StartsWith(".\\", StringComparison.Ordinal) || text.StartsWith("./", StringComparison.Ordinal):
                return Path.GetFullPath(Path.Combine(relativePathReplacement, text));
            case PSObject nestedObject:
                ReplaceRelativePaths(nestedObject, relativePathReplacement);
                return nestedObject;
            case object[] array:
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = ReplaceRelativePathValue(array[i], relativePathReplacement) ?? array[i];
                }
                return array;
            default:
                return value;
        }
    }

    // Searches for a specific property in the configuration object and returns its value
    public object? SearchProperty(string propertyName)
    {
        // Implement the property search logic here based on the propertyName and scriptConfig
        throw new NotImplementedException();
    }

    public Collection<PSObject>? Convert(string inputObject, JsonSchemaPath? schemaPath = null, string? relativePathReplacement = null)
    {
        // Read the JSON content from the configuration file and validate it against the provided schema if specified
        if (schemaPath != null && !string.IsNullOrWhiteSpace(schemaPath.ToString()))
        {
            JToken jsonToken = JToken.Parse(inputObject);
            JSchema schemaToken = JSchema.Parse(File.ReadAllText(schemaPath.ToString()));
            if (!jsonToken.IsValid(schemaToken, out IList<string> validationErrors))
            {
                throw new BootstrapException($"JSON content does not conform to the schema. Errors: {string.Join(", ", validationErrors)}");
            }
        }

        // Convert the JSON content to a PowerShell object
        using var ps = PowerShell.Create();
        Collection<PSObject> jsonObjects = ps.AddCommand("ConvertFrom-Json")
            .AddParameter("InputObject", inputObject)
            .Invoke();

        // Replace relative paths in the JSON objects if a relative path replacement is provided
        if (!string.IsNullOrWhiteSpace(relativePathReplacement))
        {
            foreach (PSObject jsonObject in jsonObjects)
            {
                ReplaceRelativePaths(jsonObject, relativePathReplacement!);
            }
        }

        Config = jsonObjects;
        return Config;
    }
}