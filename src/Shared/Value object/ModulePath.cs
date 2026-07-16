using System.IO;

namespace PSBootstrap.Shared.Value_object;

public class ModulePath(string path) : CheckFolder(path, "module", true);