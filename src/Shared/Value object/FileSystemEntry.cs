namespace PSBootstrap.Shared.Value_object;

public abstract record FileSystemEntry(string Name)
{
    public string Name { get; } = Name;
}