/// <summary>
///  Some of this code was taken, with modifications, from https://github.com/aspnet/AspNetCore/blob/d3400f7cb23d83ae93b536a5fc9f46fc2274ce68/src/Components/Blazor/Server/src/BlazorConfig.cs
///  Which is:
///  // Copyright (c) .NET Foundation. All rights reserved.
///  // Licensed under the Apache License, Version 2.0. See License.txt at this location for information: https://github.com/aspnet/AspNetCore/blob/master/LICENSE.txt
/// </summary>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using System;
using System.Net.Mime;

namespace NetPack.Blazor
{
    public static class AppBuilderExtensions
    {

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
