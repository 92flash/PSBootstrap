using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PSBootstrap.Shared.Enum;

namespace PSBootstrap.Shared.Template;

public static class LogTemplate
{
    public static readonly Regex HeaderMatch = new(@"^(Date|Computername|Username):\s*(.+)$", RegexOptions.Compiled | RegexOptions.Multiline);
    public static readonly Regex LogEntryMatch = new(@"^(\d{2}-\d{2}-\d{4} \d{2}:\d{2}:\d{2}) - (\w+):\s*(.+)$", RegexOptions.Compiled | RegexOptions.Multiline);
    
    public static string LineDetails(string message, LogType logType) => $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - " + $"{logType}:".PadRight(14, ' ') + message;

    public static string[] LogHeader()
    {
        return [
            $"Date:".PadRight(16, ' ') + $"{DateTime.Now:dd-MM-yyyy}",
            $"Computername:".PadRight(16, ' ') + $"{Environment.MachineName}",
            $"Username:".PadRight(16, ' ') + $"{Environment.UserName}",
            Environment.NewLine
        ];
    }

    public static Dictionary<LogType, ConsoleColor> TypeToColor()
    {
        return new Dictionary<LogType, ConsoleColor>
        {
            [LogType.Attention] = ConsoleColor.DarkYellow,
            [LogType.Success] = ConsoleColor.Green
        };
    }
}