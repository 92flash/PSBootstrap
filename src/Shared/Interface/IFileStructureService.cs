using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Shared.Interface;

internal interface IFileStructureService
{
    void Create(string baseDirectory, FileSystemEntry[] fileStructure);
    void Delete(string baseDirectory, FileSystemEntry[] fileStructure);
}