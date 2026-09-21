#nullable enable

using System.Collections.ObjectModel;
using System.Management.Automation;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Shared.Interface;

internal interface IConfigService
{
    public Collection<PSObject>? Convert(string inputObject, JsonSchemaPath? schemaPath = null, string? relativePathReplacement = null);
    public Collection<PSObject>? ConvertFromPath(JsonConfigPath jsonConfigPath, JsonSchemaPath? schemaPath = null, string? relativePathReplacement = null);
    public object? SearchProperty(string propertyName);
}