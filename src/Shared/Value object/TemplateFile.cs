#nullable enable
namespace PSBootstrap.Shared.Value_object;

public sealed record TemplateFile(string Name, string? Content = null) : FileSystemEntry(Name)
{
    public string? Content { get; private set; } = Content;
}