using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using PSBootstrap.Service;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Template;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap
{
    [Cmdlet(VerbsCommon.New,"BootstrapStructure")]
    public class NewBootstrapStructureCmdletCommand : PSCmdlet
    {
        private string moduleVersion;
        private string currentDirectoryName;
        private IFileStructureService fileStructureService;

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
            fileStructureService.Create(SessionState.Path.CurrentLocation.ProviderPath, FileStructureTemplate.Get("properties", currentDirectoryName, "Bootstrap", moduleVersion, $".\\Log\\{currentDirectoryName}.log"));
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            
        }
    }
}
