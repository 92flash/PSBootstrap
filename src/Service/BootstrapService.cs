using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using PSBootstrap.Shared.Exception;
using PSBootstrap.Shared.Interface;

namespace PSBootstrap.Service;

internal class BootstrapService : IBootstrapService
{
    private readonly PSCmdlet moduleContext;
    private static string _currentDebugPreference;
    private static string _currentVerbosePreference;

    public BootstrapService(PSCmdlet context)
    {
        moduleContext = context;
    }

    // Checks if the specified functions are available in the current PowerShell session.
    public void CheckFunctions(string[] functionNames)
    {
        foreach (string functionName in functionNames)
        {
            using var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            Collection<PSObject> result = ps.AddCommand("Get-Command")
                                            .AddParameter("Name", functionName)
                                            .Invoke();

            if (result.Count == 0)
            {
                throw new BootstrapException($"Required function '{functionName}' is not available in the current PowerShell session, while it is required by the script config.");
            }
        }
    }

    // Checks if the specified modules are available in the current PowerShell session.
    public void CheckModules(string[] moduleNames)
    {
        using var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
        Collection<PSModuleInfo> available = ps.AddCommand("Get-Module")
                                                .Invoke<PSModuleInfo>();

        foreach (string moduleName in moduleNames)
        {
            if (!available.Any(m => m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new BootstrapException($"Required module '{moduleName}' is not imported in the current PowerShell session, while it is required by the script config.");
            }
        }
    }

    // Enables or disables debug output in the current PowerShell session.
    public void EnableDebug(bool debug)
    {
        if (null == _currentDebugPreference)
        {
            using var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            Collection<PSObject> result = ps.AddCommand("Get-Variable")
                                            .AddParameter("Name", "DebugPreference")
                                            .Invoke();

            // If DebugPreference is changed from its default value of "SilentlyContinue", don't overwrite it to respect the user's preference. Only change it if it's still set to the default value.
            PSVariable variable = result[0].BaseObject as PSVariable;
            if (variable?.Value?.ToString() != "SilentlyContinue") return;


            if (result.Count > 0 && variable is not null)
            {
                _currentDebugPreference = variable.Value?.ToString();
            }
        }

        if (debug)
        {
            using var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.AddCommand("Set-Variable").AddParameter("Name", "DebugPreference").AddParameter("Value", "Inquire").Invoke();
        }
        else if (_currentDebugPreference != null)
        {
            using var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.AddCommand("Set-Variable").AddParameter("Name", "DebugPreference").AddParameter("Value", _currentDebugPreference).Invoke();
        }
    }

    // Enables or disables verbose output in the current PowerShell session.
    public void EnableVerbose(bool verbose)
    {
        if (null == _currentVerbosePreference)
        {
            using var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            Collection<PSObject> result = ps.AddCommand("Get-Variable")
                                            .AddParameter("Name", "VerbosePreference")
                                            .Invoke();

            // If VerbosePreference is changed from its default value of "SilentlyContinue", don't overwrite it to respect the user's preference. Only change it if it's still set to the default value.
            PSVariable variable = result[0].BaseObject as PSVariable;
            if (variable?.Value?.ToString() != "SilentlyContinue") return;
                                            
            if (result.Count > 0 && variable is not null)
            {
                _currentVerbosePreference = variable.Value?.ToString();
            }
        }

        if (verbose)
        {
            using var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.AddCommand("Set-Variable").AddParameter("Name", "VerbosePreference").AddParameter("Value", "Continue").Invoke();
        }
        else if (_currentVerbosePreference != null)
        {
            using var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.AddCommand("Set-Variable").AddParameter("Name", "VerbosePreference").AddParameter("Value", _currentVerbosePreference).Invoke();
        }
    }

    // Loads only the specified functions or all functions from the specified root path into the current PowerShell session.
    public void LoadFunctions(string rootFunctionPath, string[] functionNames)
    {
        string[] functionPaths = functionNames == null || functionNames.Length == 0 ?
            [.. Directory.EnumerateFiles(rootFunctionPath, "*.ps1", SearchOption.AllDirectories)] :
            [.. Directory.EnumerateFiles(rootFunctionPath, "*.ps1", SearchOption.AllDirectories)
                .Where(f => functionNames.Contains(Path.GetFileNameWithoutExtension(f), StringComparer.OrdinalIgnoreCase))];

        if (functionPaths == null || functionPaths.Length == 0)
        {
            return;
        }

        foreach (string functionPath in functionPaths)
        {
            string functionName = Path.GetFileNameWithoutExtension(functionPath);
            string fileText = File.ReadAllText(functionPath);

            ScriptBlockAst ast = Parser.ParseInput(fileText, out _, out ParseError[] errors);
            FunctionDefinitionAst functionAst = ast.FindAll(
                a => a is FunctionDefinitionAst f && f.Name == functionName, true)
                .Cast<FunctionDefinitionAst>()
                .FirstOrDefault() ?? throw new /* BootstrapException */("Expected function '" + functionName + "' was not found in '" + functionPath + "'.");
            ScriptBlock scriptBlock = functionAst.Body.GetScriptBlock();

            moduleContext.SessionState.InvokeProvider.Item.Set(
                [$"Function:\\Global:{functionName}"],
                scriptBlock,
                force: true,
                literalPath: false
            );
        }
    }

    // Loads the specified modules into the current PowerShell session.
    public void LoadModules(string rootModulePath, string[] moduleNames)
    {
        if (moduleNames == null || moduleNames.Length == 0)
        {
            return;
        }

        foreach (string moduleName in moduleNames)
        {
            using var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.AddCommand("Import-Module")
              .AddParameter("Name", $"{rootModulePath}\\{moduleName}")
              .AddParameter("Force")
              .Invoke();
        }
    }
}