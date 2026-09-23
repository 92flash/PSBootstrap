using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using PSBootstrap.Service;
using PSBootstrap.Shared.Template;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap
{
    [Cmdlet(VerbsCommon.New,"BootstrapStructure")]
    public class NewBootstrapStructureCmdletCommand : PSCmdlet
    {
        [Parameter(
            HelpMessage = "Enable configuration directly in the Bootstrap config"
        )]
        public SwitchParameter EnableConfig { get; set; }

        [Parameter(
            HelpMessage = "Enable automatic resolution of paths, when the Config section is also enabled, in the Bootstrap config"
        )]
        public SwitchParameter EnableConfigAutoResolvePaths { get; set; }

        [Parameter(
            HelpMessage = "Enable logging directly in the Bootstrap config"
        )]
        public SwitchParameter EnableLogging { get; set; }

        [Parameter(
            HelpMessage = "Enable showing the same log message in the shell output if logging is also enabled"
        )]
        public SwitchParameter EnableShowShellOutput { get; set; }

        private string moduleVersion;
        private string currentDirectoryName;
        private FileStructureService fileStructureService;

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            moduleVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? string.Empty;
            currentDirectoryName = Path.GetFileName(SessionState.Path.CurrentLocation.ProviderPath).Replace(" ", "_");
            fileStructureService = new FileStructureService();
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            fileStructureService.Create(SessionState.Path.CurrentLocation.ProviderPath, FileStructureTemplate.Get(
                "properties", currentDirectoryName, "Bootstrap", moduleVersion, $".\\Log\\{currentDirectoryName}.log",
                EnableConfig.IsPresent, EnableConfigAutoResolvePaths.IsPresent, EnableLogging.IsPresent, EnableShowShellOutput.IsPresent));
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            
        }
    }
}