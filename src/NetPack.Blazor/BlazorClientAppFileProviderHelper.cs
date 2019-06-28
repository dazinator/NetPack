using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.IO;
using System.Linq;
/// <summary>
///  Taken from https://github.com/aspnet/AspNetCore/blob/d3400f7cb23d83ae93b536a5fc9f46fc2274ce68/src/Components/Blazor/Server/src/BlazorConfig.cs
///  Which is:
///  // Copyright (c) .NET Foundation. All rights reserved.
///  //  Licensed under the Apache License, Version 2.0. See License.txt at this location for information: https://github.com/aspnet/AspNetCore/blob/master/LICENSE.txt
/// </summary>
namespace NetPack.Blazor
{
    public static class BlazorClientAppFileProviderHelper
    {
       
        public static IFileProvider GetFileProvider<TClientApp>()
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
            var outputAssemblyPath = Path.Combine(projDirectory, configLines[1]);
            string webRootPath = Path.Combine(projDirectory, "wwwroot");
            bool webRootPathExists = Directory.Exists(webRootPath);

            var distPath = Path.Combine(Path.GetDirectoryName(outputAssemblyPath), "dist");

            bool distExists = Directory.Exists(distPath);

            var fileProviders = new List<IFileProvider>();
            if(webRootPathExists)
            {
                fileProviders.Add(new PhysicalFileProvider(webRootPath));
            }

            if (distExists)
            {
                fileProviders.Add(new PhysicalFileProvider(distPath));
            }

            if(fileProviders.Count > 1)
            {
                return new CompositeFileProvider(fileProviders);
            }
            else
            {
                return fileProviders.FirstOrDefault();
            }
        }
    }
}
