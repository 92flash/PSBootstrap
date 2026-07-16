using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PSBootstrap.Service;
using PSBootstrap.Shared.Context;
using PSBootstrap.Shared.Entity;
using PSBootstrap.Shared.Exception;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Template;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap
{
    [Cmdlet(VerbsCommon.Get,"Log")]
    [OutputType(typeof(Log))]
    public class GetLogCmdletCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            HelpMessage = "Set the path for the logfile with the file and extention at the end of the path"
        )]
        public LogPath LogPath
        {
            get => _logPath ?? BootstrapContext.LogPath;
            set => _logPath = value;
        }


        private LogPath _logPath;
        private ILogService logService;

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            if (LogPath == null)
            {
                var ex = new BootstrapException("LogPath is not set. Please set the LogPath as a parameter or as an environment variable ($env:LogPath)");
                ThrowTerminatingError(new ErrorRecord(ex, "LogPathNotSet", ErrorCategory.InvalidArgument, null));
                return;
            }

            logService = new LogService(LogPath);
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            WriteObject(logService?.Get(LogTemplate.HeaderMatch, LogTemplate.LogEntryMatch), true);
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            logService?.Dispose();
        }
    }
}
