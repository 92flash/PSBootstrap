using System.Collections.Generic;
using System.Management.Automation;
using PSBootstrap.Service;
using PSBootstrap.Shared.Entity;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap
{
    [Cmdlet(VerbsData.ConvertFrom,"JsonWithSchema")]
    [OutputType(typeof(object))]
    public class ConvertFromJsonWithSchemaCmdletCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            HelpMessage = "Set the path for the JSON configuration file with the file and extension at the end of the path",
            ParameterSetName = "InputObject",
            ValueFromPipeline = true
        )]
        public string[] InputObject { get; set; }

        [Parameter(
            Position = 0,
            HelpMessage = "Set the path for the JSON configuration file with the file and extension at the end of the path",
            ParameterSetName = "Path"
        )]
        public JsonConfigPath Path { get; set; }

        [Parameter(
            Position = 1,
            HelpMessage = "Set the path for the JSON schema file with the file and extension at the end of the path"
        )]
        public JsonSchemaPath SchemaPath { get; set; }

        [Parameter(
            HelpMessage = "Set whether to automatically resolve relative paths for the JSON configuration file"
        )]
        public bool AutoResolveRelativeConfigPaths { get; set; }

        private IConfigService _configService;
        private List<string> _inputLines;
        private string _relativePathReplacement;

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            _configService = new ConfigService();
            _inputLines = [];
            _relativePathReplacement = AutoResolveRelativeConfigPaths ? MyInvocation.MyCommand.Module.ModuleBase : string.Empty;
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                // Buffer InputObject across pipeline calls since PowerShell invokes ProcessRecord once per pipeline object (e.g. once per line when piping Get-Content),
                // so the JSON can only be parsed once all lines have been collected
                case "InputObject":
                    _inputLines.AddRange(InputObject);
                    break;
                case "Path":
                    WriteObject(_configService.ConvertFromPath(Path, SchemaPath, _relativePathReplacement));
                    break;
                default:
                    throw new PSArgumentException("Either InputObject or Path must be provided.");
            }
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            if (ParameterSetName == "InputObject")
            {
                WriteObject(_configService.Convert(string.Join(System.Environment.NewLine, _inputLines), SchemaPath, _relativePathReplacement));
            }
        }
    }
}