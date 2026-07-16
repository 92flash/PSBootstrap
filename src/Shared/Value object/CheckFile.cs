#nullable enable
using System;
using System.IO;
using System.Linq;
using PSBootstrap.Shared.Exception;

namespace PSBootstrap.Shared.Value_object;

public class CheckFile
{
    public string Path {get; private set;}

    public CheckFile(string path, string intendedUse, bool checkExistence = false, string[]? allowedExtensions = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new BootstrapException($"The value for {intendedUse} file in '{path}' cannot be null or empty.");
        }

        if (!IsPathValid(path))
        {
            throw new BootstrapException($"The provided {intendedUse} file in '{path}' is not set to a valid path.");
        }

        if (checkExistence && !File.Exists(path))
        {
            throw new BootstrapException($"The provided {intendedUse} file in '{path}' does not exist.");
        }

        string extension = System.IO.Path.GetExtension(path);
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new BootstrapException($"The provided {intendedUse} file in '{path}' does not contain a file extension");
        }
        else if (allowedExtensions != null && !allowedExtensions.Any(ext => string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BootstrapException($"The provided {intendedUse} file in '{path}' has an invalid file extension. Allowed file extensions are {string.Join(", ", allowedExtensions)}");
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

    public bool Exists() => File.Exists(Path);

    public override string ToString()
    {
        return Path;
    }
}