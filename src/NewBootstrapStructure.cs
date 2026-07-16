using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PSBootstrap.Service;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Template;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap
{
    [Cmdlet(VerbsCommon.New,"BootstrapStructure")]
    public class NewBootstrapStructureCmdletCommand : PSCmdlet
    {

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            string currentDirectoryName = Path.GetFileName(SessionState.Path.CurrentLocation.ProviderPath);
            IFileStructureService fileStructureService = new FileStructureService();
            fileStructureService.Create(SessionState.Path.CurrentLocation.ProviderPath, FileStructureTemplate.Get("properties", currentDirectoryName, "Bootstrap"));
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            
        }
    }
}
