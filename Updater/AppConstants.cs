using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater;

public class AppConstants
{
    public static readonly string ToolsDirectory = Path.Combine(GetSystemDirectory(), "Updater");
    public const string ServerIP = "10.32.5.145";
    public const string Port = "60091";

    private static string GetSystemDirectory()
    {
        // Use the system's application data folder based on the OS
        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    }
}
