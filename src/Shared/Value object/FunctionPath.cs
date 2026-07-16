using System.IO;

namespace PSBootstrap.Shared.Value_object;

public class FunctionPath(string path) : CheckFolder(path, "function", true);