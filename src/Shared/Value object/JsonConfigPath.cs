using System;
using System.IO;
using System.Linq;

namespace PSBootstrap.Shared.Value_object;

public class JsonConfigPath(string path) : CheckFile(path, "json config", true, [".json"]);