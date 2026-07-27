#nullable enable

using System.Collections.ObjectModel;
using System.Management.Automation;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Shared.Interface;

internal interface IConfigService
{
    public Collection<PSObject>? Convert(JsonSchemaPath? schemaPath = null);
    public object? SearchProperty(string propertyName);
}