using System.IO;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Service;

internal class FileStructureService : IFileStructureService
{
    // Creates a file system structure based on the provided base directory and file structure template
    public void Create(string baseDirectory, FileSystemEntry[] fileStructure)
    {
        foreach (var item in fileStructure)
        {
            switch (item)
            {
                case TemplateFolder folder:
                    Directory.CreateDirectory(Path.Combine(baseDirectory, folder.Name));
                    if (folder.SubFolders != null)
                    {
                        Create(Path.Combine(baseDirectory, folder.Name), [.. folder.SubFolders]);
                    }
                    break;
                case TemplateFile file:
                    File.WriteAllText(Path.Combine(baseDirectory, file.Name), file.Content ?? string.Empty);
                    break;
            }
        }
    }

    public void Delete(string baseDirectory, FileSystemEntry[] fileStructure)
    {
        foreach (var item in fileStructure)
        {
            switch (item)
            {
                case TemplateFolder folder:
                    string folderPath = Path.Combine(baseDirectory, folder.Name);
                    if (Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                    }
                    break;
                case TemplateFile file:
                    string filePath = Path.Combine(baseDirectory, file.Name);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    break;
            }
        }
    }
}