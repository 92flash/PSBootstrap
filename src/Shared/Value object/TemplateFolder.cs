#nullable enable
namespace PSBootstrap.Shared.Value_object;

public sealed record TemplateFolder(string Name, FileSystemEntry[]? SubFolders = null) : FileSystemEntry(Name)
{
    public FileSystemEntry[]? SubFolders { get; } = SubFolders;
}