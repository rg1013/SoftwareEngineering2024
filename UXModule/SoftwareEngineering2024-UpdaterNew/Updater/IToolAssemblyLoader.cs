/******************************************************************************
* Filename    = IToolAssemblyLoader.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = 
*****************************************************************************/

namespace Updater;
public interface IToolAssemblyLoader
{
    public Dictionary<string, List<string>> LoadToolsFromFolder(string folderPath);
}
