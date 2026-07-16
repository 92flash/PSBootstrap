namespace PSBootstrap.Shared.Interface;

internal interface IBootstrapService
{
    public void LoadFunctions(string rootFunctionPath, string[] functionNames);
    public void CheckFunctions(string[] functionNames);
    public void LoadModules(string rootModulePath, string[] moduleNames);
    public void CheckModules(string[] moduleNames);
    public void EnableVerbose(bool verbose);
    public void EnableDebug(bool debug);
}