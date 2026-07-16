using System.IO;

namespace PSBootstrap.Shared.Value_object;

public class ConfigPath(string path) : CheckFolder(path, "config", true)
{
}