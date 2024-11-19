/******************************************************************************
* Filename    = IToolAssemblyLoader.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Interface for ToolAssemblyLoader
*****************************************************************************/

namespace Updater;
/// <summary>
/// Interface for loading tool assemblies from a specified folder.
/// This interface defines the method for fetching tool assemblies present in a folder path.
/// </summary>
public interface IToolAssemblyLoader
{
    /// <summary>
    /// Loads tool assemblies from the specified folder path.
    /// This method retrieves tools and their respective names from a given directory.
    /// </summary>
    /// <param name="folderPath">
    /// The path of the folder where tool assemblies are stored.
    /// </param>
    /// <returns>
    /// A dictionary containing tool names as keys and a list of related assembly files as values.
    /// </returns>
    public Dictionary<string, List<string>> LoadToolsFromFolder(string folderPath);
}
