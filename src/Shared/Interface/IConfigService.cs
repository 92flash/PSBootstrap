#nullable enable

using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Shared.Interface;

internal interface IConfigService
{
    public object? Convert(JsonSchemaPath? schemaPath = null);
    public object? SearchProperty(string propertyName);
}