using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using PSBootstrap.Service;
using PSBootstrap.Shared.Context;
using PSBootstrap.Shared.Enum;
using PSBootstrap.Shared.Exception;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Template;
using PSBootstrap.Shared.Utility;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap
{
    [Cmdlet(VerbsCommunications.Write,"Log")]
    public class WriteLogCmdletCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "This message will be written to the logfile and optionally to the shell if OutHost is set to true"
        )]
        public string Message { get; set; }

        [Parameter(
            Position = 1,
            HelpMessage = "Choose what type of message is logged for detailed information. The default type is Information"
        )]
        [ValidateSet("Information", "Warning", "Attention", "Error", "Success", "Verbose", "Debug", "Fatal", IgnoreCase = true)]
        public LogType Type = LogType.Information;

        public LogPath LogPath
        {
            get => _logPath ?? BootstrapContext.LogPath;
            set => _logPath = value;
        }

        [Parameter(
            HelpMessage = "Together with writing the Message to the logfile, it also writes the Message to the shell"
        )]
        public SwitchParameter ShowShellOutput;

        [Parameter(
            HelpMessage = "Write only the message as logline and forgo the date/time and type"
        )]
        public SwitchParameter NoDetails;

        private LogPath _logPath;
        private ILogService _logService;

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            if (LogPath == null)
            {
                var ex = new BootstrapException("LogPath is not set. Please set the LogPath as a parameter or as an environment variable ($env:LogPath)");
                ThrowTerminatingError(new ErrorRecord(ex, "LogPathNotSet", ErrorCategory.InvalidArgument, null));
                return;
            }

            // Set OutHost to true if the environment variable is set and OutHost is not already true
            if (ShowShellOutput == false && BootstrapContext.ShowShellOutput == true)
            {
                ShowShellOutput = true;
            }

            _logService = new LogService(LogPath);
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            // Check if SilentlyContinue or Ignore is set for a preference variable
            bool IsSilent(string preferenceVariable) =>
                GetVariableValue(preferenceVariable)?.ToString() is "SilentlyContinue" or "Ignore";

            // Determine if the log message should be skipped based on the log type and preference variables, keeping it in line with PowerShell's behavior for preference variables
            bool skip = (Type == LogType.Debug && IsSilent("DebugPreference")) ||
                (Type == LogType.Verbose && IsSilent("VerbosePreference"));

            // Write the log message to the log file if the type is not Debug or Verbose and they are not set to SilentlyContinue or Ignore
            if (!skip)
            {
                _logService.Append(Message, Type, !NoDetails);
            }
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            // Make sure to properly close the log file for a next write operation to avoid file locking issues and dispose of the log service
            _logService?.Dispose();

            // Write the message to the PowerShell host
            ShellUtil shellUtil = new(this);
            if (ShowShellOutput == true) shellUtil.WriteType(Message, Type);
        }
    }
}