using System;
using System.IO;
using System.Linq;

namespace NetPack.Utils;

public static class NodeFnmHelper
{
    public static void SetPath()
    {
        // Try to locate fnm installation in the user's local app data
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var fnmMultishellsPath = Path.Combine(localAppData, "fnm_multishells");
        
        // If fnm_multishells directory exists, find the most recent shell and add it to PATH
        if (Directory.Exists(fnmMultishellsPath))
        {
            var shells = Directory.GetDirectories(fnmMultishellsPath);
            if (shells.Length > 0)
            {
                // Use the most recently created shell directory
                var mostRecentShell = shells.OrderByDescending(d => Directory.GetCreationTime(d)).First();
                var currentPath = Environment.GetEnvironmentVariable("PATH");
                Environment.SetEnvironmentVariable("PATH", $"{mostRecentShell};{currentPath}");
            }
        }
    }
}
