using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetPack.Blazor
{
    public static class BlazorClientAppFileProviderHelper
    {

        public static string GetBlazorClientProjectDirectory<TClientApp>(out string outputAssemblyPath)
        {
            var assemblylocation = typeof(TClientApp).Assembly.Location;
            var configFilePath = Path.ChangeExtension(assemblylocation, ".blazor.config");
            var configLines = System.IO.File.ReadLines(configFilePath).ToList();

            var projFilePath = configLines[0];
            if (projFilePath == ".")
            {
                projFilePath = assemblylocation;
            }

            var projDirectory = Path.GetDirectoryName(projFilePath);
            outputAssemblyPath = Path.Combine(projDirectory, configLines[1]);
            return projDirectory;
        }

        public static IFileProvider GetStaticFileProvider<TClientApp>()
        {
            var projDirectory = GetBlazorClientProjectDirectory<TClientApp>(out string outputAssemblyPath);

            string webRootPath = Path.Combine(projDirectory, "wwwroot");
            bool webRootPathExists = Directory.Exists(webRootPath);

            var distPath = Path.Combine(Path.GetDirectoryName(outputAssemblyPath), "dist");
            bool distExists = Directory.Exists(distPath);

            var fileProviders = new List<IFileProvider>();
            if (webRootPathExists)
            {
                fileProviders.Add(new PhysicalFileProvider(webRootPath));
            }

            if (distExists)
            {
                fileProviders.Add(new PhysicalFileProvider(distPath));
            }

            if (fileProviders.Count > 1)
            {
                return new CompositeFileProvider(fileProviders);
            }
            else
            {
                return fileProviders.FirstOrDefault();
            }
        }

        public static IFileProvider GetProjectFileProvider<TClientApp>()
        {
            var projDirectory = GetBlazorClientProjectDirectory<TClientApp>(out string outputAssemblyPath);
            return new PhysicalFileProvider(projDirectory);

        }

    }
}
