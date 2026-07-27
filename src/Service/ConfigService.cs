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

public class ConfigService(JsonConfigPath jsonConfigPath) : IConfigService
{
    public Collection<PSObject>? Config { get; private set; }
    private readonly JsonConfigPath _jsonConfigPath = jsonConfigPath;

    // Convert the JSON configuration file to a PowerShell object and optionally validate it against a JSON schema
    public Collection<PSObject>? Convert(JsonSchemaPath? schemaPath = null)
    {
        // Read the JSON content from the configuration file and validate it against the provided schema if specified
        string jsonContent = File.ReadAllText(_jsonConfigPath.ToString());
        if (schemaPath != null && !string.IsNullOrWhiteSpace(schemaPath.ToString()))
        {
            JToken jsonToken = JToken.Parse(jsonContent);
            JSchema schemaToken = JSchema.Parse(File.ReadAllText(schemaPath.ToString()));
            if (!jsonToken.IsValid(schemaToken, out IList<string> validationErrors))
            {
                throw new BootstrapException($"JSON content does not conform to the schema. Errors: {string.Join(", ", validationErrors)}");
            }
        }

        // Convert the JSON content to a PowerShell object
        using var ps = PowerShell.Create();
        Collection<PSObject> jsonObjects = ps.AddCommand("ConvertFrom-Json")
            .AddParameter("InputObject", jsonContent)
            .Invoke();

        Config = jsonObjects;
        return Config;
    }

    // Searches for a specific property in the configuration object and returns its value
    public object? SearchProperty(string propertyName)
    {
        // Implement the property search logic here based on the propertyName and scriptConfig
        throw new NotImplementedException();
    }
}