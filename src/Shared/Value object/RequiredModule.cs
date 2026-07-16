#nullable enable
using System;

namespace PSBootstrap.Shared.Value_object;

public record RequiredModule(string Name, Version? Version)
{
    public string Name { get; private set; } = Name;
    public Version? Version { get; private set; } = Version;
}