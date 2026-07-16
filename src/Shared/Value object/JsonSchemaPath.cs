using System;
using System.IO;
using System.Linq;

namespace PSBootstrap.Shared.Value_object;

public class JsonSchemaPath(string path) : CheckFile(path, "json schema", true, [".json"]);