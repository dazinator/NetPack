using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;

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

        /// <summary>
        /// Adds a <see cref="StaticFileMiddleware"/> that will serve static files from the client-side Blazor application
        /// specified by <paramref name="clientAssemblyFilePath"/>.
        /// </summary>
        /// <param name="clientAssemblyFilePath">The file path of the client-side Blazor application assembly.</param>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseClientSideBlazorFiles(this IApplicationBuilder app, IFileProvider fileProvider, bool enableDebugging)
        {
            if (fileProvider == null)
            {
                throw new ArgumentNullException(nameof(fileProvider));
            }

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            AddMapping(contentTypeProvider, ".dll", MediaTypeNames.Application.Octet);
            if (enableDebugging)
            {
                AddMapping(contentTypeProvider, ".pdb", MediaTypeNames.Application.Octet);
            }

            var options = new StaticFileOptions()
            {
                ContentTypeProvider = contentTypeProvider,
                FileProvider = fileProvider,
                OnPrepareResponse = SetCacheHeaders,
            };

            app.UseStaticFiles(options);
            return app;


        }
        static void AddMapping(FileExtensionContentTypeProvider provider, string name, string mimeType)
        {
            if (!provider.Mappings.ContainsKey(name))
            {
                provider.Mappings.Add(name, mimeType);
            }
        }

        internal static void SetCacheHeaders(StaticFileResponseContext ctx)
        {
            // By setting "Cache-Control: no-cache", we're allowing the browser to store
            // a cached copy of the response, but telling it that it must check with the
            // server for modifications (based on Etag) before using that cached copy.
            // Longer term, we should generate URLs based on content hashes (at least
            // for published apps) so that the browser doesn't need to make any requests
            // for unchanged files.
            var headers = ctx.Context.Response.GetTypedHeaders();
            if (headers.CacheControl == null)
            {
                headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true
                };
            }
        }
    }
}
