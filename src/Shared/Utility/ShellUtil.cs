using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PSBootstrap.Shared.Enum;
using PSBootstrap.Shared.Template;

namespace PSBootstrap.Shared.Utility;

internal class ShellUtil
{
    private readonly PSCmdlet moduleContext;
    private static readonly object ConsoleLock = new();

    internal ShellUtil(PSCmdlet context)
    {
        moduleContext = context;
    }

    internal static void Write(string message, ConsoleColor? color = null, bool newLine = true)
    {
        lock (ConsoleLock)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            if (null != color)
            {
                Console.ForegroundColor = color.Value;
            }

            try
            {
                if (newLine)
                {
                    Console.WriteLine(message);
                }
                else
                {
                    Console.Write(message);
                }
            }
            finally
            {
                Console.ForegroundColor = defaultColor;
            }
        }
    }

    private void TypeToOutput(string message, LogType logType)
    {
        switch (logType)
        {
            case LogType.Warning:
                moduleContext?.WriteWarning(message);
                break;

            case LogType.Error:
                ErrorRecord errorRecord = new(new System.Exception(message), "Error", ErrorCategory.NotSpecified, null);
                moduleContext?.WriteError(errorRecord);
                break;

            case LogType.Fatal:
                // ThrowTerminatingError expects an ErrorRecord
                ErrorRecord fatalRecord = new(new System.Exception(message), "Fatal", ErrorCategory.NotSpecified, null);
                moduleContext?.ThrowTerminatingError(fatalRecord);
                break;

            case LogType.Verbose:
                moduleContext?.WriteVerbose(message);
                break;

            case LogType.Debug:
                moduleContext?.WriteDebug(message);
                break;

            case LogType.Information:
                HostInformationMessage hostMessage = new()
                {
                    Message = message,
                    ForegroundColor = ConsoleColor.Cyan
                };

                var record = new InformationRecord(hostMessage, moduleContext.MyInvocation.MyCommand.Name);
                record.Tags.Add("PSHOST");
                
                moduleContext?.WriteInformation(record);
                break;
        }
    }

    internal void WriteType(string message, LogType logType)
    {
        Dictionary<LogType, ConsoleColor> colorType = LogTemplate.TypeToColor();
        if (!colorType.Keys.Any(type => logType == type))
        {
            TypeToOutput(message, logType);
            return;
        }

        Write(message, colorType[logType]);
    }
}