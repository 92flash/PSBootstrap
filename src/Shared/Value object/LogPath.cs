using System;
using System.IO;
using System.Linq;

namespace PSBootstrap.Shared.Value_object;

public class LogPath(string logPath) : CheckFile(logPath, "log", false, [".log", ".txt"]);