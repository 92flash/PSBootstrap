using System;
using System.IO;
using PSBootstrap.Shared.Exception;

namespace PSBootstrap.Shared.Value_object;

public class CheckFolder
{
    public string Path {get; private set;}

    public CheckFolder(string path, string intendedUse, bool checkExistence = false)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new BootstrapException($"The value for {intendedUse} directory in '{path}' cannot be null or empty.");
        }

        if (!IsPathValid(path))
        {
            throw new BootstrapException($"The provided {intendedUse} directory in '{path}' is not set to a valid path.");
        }
        
        if (checkExistence && !Directory.Exists(path))
        {
            throw new BootstrapException($"The provided {intendedUse} directory in '{path}' does not exist.");
        }

        Path = path;
    }

    internal bool IsPathValid(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        try
        {
            _ = System.IO.Path.GetFullPath(path);
            return true;
        }
        catch (System.Exception ex) when (ex is ArgumentException or PathTooLongException or NotSupportedException)
        {
            return false;
        }
    }

    public bool Exists() => Directory.Exists(Path);

    public override string ToString()
    {
        return Path;
    }
}