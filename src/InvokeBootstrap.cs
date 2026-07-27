using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using PSBootstrap.Service;
using PSBootstrap.Shared.Context;
using PSBootstrap.Shared.Entity;
using PSBootstrap.Shared.Enum;
using PSBootstrap.Shared.Exception;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Template;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap
{
    [Cmdlet(VerbsLifecycle.Invoke,"Bootstrap")]
    [OutputType(typeof(object))]
    public class InvokeBootstrapCmdletCommand : PSCmdlet
    {
        private ScriptConfig _scriptConfig;
        private IBootstrapService _bootstrapService;
        private IConfigService _configService;
        private Collection<PSObject> _config;

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            try
            {
                string moduleBase = MyInvocation.MyCommand.Module.ModuleBase;
                string xmlPath = new XmlPath(Path.Combine(moduleBase, "Bootstrap.xml")).ToString();

                // Make sure the script config exists
                if (!File.Exists(xmlPath))
                {
                    string file = Path.GetFileName(xmlPath);
                    string directory = Path.GetDirectoryName(xmlPath);
                    throw new BootstrapException($"The Bootstrap XML '{file}' doesn't exist at location '{directory}'. You can create a new Bootstrap.xml file by running the 'New-BootstrapStructure' cmdlet.");
                }  

                // Load the script configuration from the XML file
                IScriptConfigService scriptConfigService = new ScriptConfigService(xmlPath);
                _scriptConfig = scriptConfigService.Convert() ??
                    throw new BootstrapException("Script configuration is not initialized.");

                _bootstrapService = new BootstrapService(this);

                // Check if the BootstrapVersion in the script config matches the assembly version
                if (_scriptConfig.BootstrapVersion != null)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string informationalVersion = assembly
                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                        ?.InformationalVersion.Split('+')[0] ?? string.Empty;

                    if (informationalVersion != _scriptConfig.BootstrapVersion.ToString())
                    {
                        throw new BootstrapException($"Bootstrap version mismatch. Expected: {_scriptConfig.BootstrapVersion}, Actual: {informationalVersion}");
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowAsScriptError(ex);
            }
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            try
            {
                // Load and check the functions specified in the script configuration
                _bootstrapService.LoadFunctions(_scriptConfig.FunctionPath.ToString(), _scriptConfig.Functions);
                if (_scriptConfig.Functions.Length > 0)
                {
                    _bootstrapService.CheckFunctions(_scriptConfig.Functions);
                }

                // Load and check the modules specified in the script configuration
                string[] moduleNames = [.. _scriptConfig.Modules.Select(m => m.Name)];
                if (moduleNames.Length > 0)
                {
                    _bootstrapService.CheckModules(moduleNames);
                    _bootstrapService.LoadModules(_scriptConfig.ModulePath.ToString(), moduleNames);
                }

                // If logging is enabled, set the LogPath for other cmdlets and throw an exception if the LogPath is not specified in the script configuration
                if (_scriptConfig.LogEnabled && _scriptConfig.LogPath != null && BootstrapContext.LogPath == null)
                {
                    BootstrapContext.LogPath = _scriptConfig.LogPath;
                }
                else if (_scriptConfig.LogEnabled && _scriptConfig.LogPath == null && BootstrapContext.LogPath == null)
                {
                    throw new BootstrapException("Logging for the script has not been initialized because the 'LogPath' property is not specified in the 'Bootstrap.xml' config which can cause problems when using the Write-Log function.");
                }

                // Set the ShowShellOutput property in the BootstrapContext based on the script configuration to allow other cmdlets to determine whether to show output in the shell or not
                BootstrapContext.ShowShellOutput = _scriptConfig.ShowShellOutput;

                // Enable or disable verbose and debug output
                _bootstrapService.EnableVerbose(_scriptConfig.Verbose);
                _bootstrapService.EnableDebug(_scriptConfig.Debug);

                // If enabled in the script configuration, load and validate the Json (domain) configuration file with the Json schema
                if (_scriptConfig.ConfigFileName != null && _scriptConfig.ConfigPath != null)
                {
                    if (string.IsNullOrWhiteSpace(_scriptConfig.ConfigFileName))
                    {
                        throw new BootstrapException("The 'FileName' property in 'Bootstrap.xml' cannot be null or empty when 'Config' is enabled.");
                    }

                    JsonConfigPath jsonConfigPath = new(Path.Combine(_scriptConfig.ConfigPath.ToString(), $"{_scriptConfig.ConfigFileName}.json"));
                    _configService = new ConfigService(jsonConfigPath);

                    JsonSchemaPath schemaPath = _scriptConfig.SchemaPath ?? new(Path.Combine(_scriptConfig.ConfigPath.ToString(), $"{_scriptConfig.ConfigFileName}_schema.json"));
                    _config = _configService.Convert(schemaPath);

                    if (_config == null ||  _config.Count == 0  || _config[0].Properties.Count() == 0)
                    {
                        throw new BootstrapException($"The configuration was successfully loaded but returned null. Please make sure {_scriptConfig.ConfigPath}\\{_scriptConfig.ConfigFileName}.json contains valid JSON properties or turn off the 'Config' feature in the 'Bootstrap.xml' file.");
                    }
                }
            }
            catch (Exception ex)
            {
                // If logging is enabled and the LogPath is specified, log the error message to the log file
                if (_scriptConfig.LogEnabled && _scriptConfig.LogPath != null)
                {
                    using ILogService logService = new LogService(_scriptConfig.LogPath);
                    logService.Append($"An error occurred during the bootstrap process: {ex.Message}", LogType.Error, true);
                }

                ThrowAsScriptError(ex);
            }
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            BootstrapContext.RanBootstrap = true;

            // Return the (domain) configuration object to the pipeline if it was loaded and validated successfully
            WriteObject(_config);
        }

        // Re-throws this exception as a genuine PowerShell script-level "throw", which unconditionally stops the calling
        // script (matching PowerShell's own throw behavior) regardless of $ErrorActionPreference or begin/process/end block
        // boundaries - unlike ThrowTerminatingError, which doesn't reliably propagate as terminating across those boundaries.
        // Still fully catchable by an explicit try/catch in the caller, unlike Environment.Exit().
        private void ThrowAsScriptError(Exception ex)
        {
            InvokeCommand.InvokeScript("param($e) throw $e", [ex]);
        }
    }
}
