using Microsoft.Extensions.DependencyInjection;
using NetPack.Blazor.JsHost.HostCapabilities;

namespace NetPack.Blazor.JsHost
{
    /// <summary>
    /// Extension methods for configuring NetPack Blazor JS Host services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds NetPack Blazor JS Host services to the service collection.
        /// </summary>
        public static IServiceCollection AddNetPackJsHost(this IServiceCollection services)
        {
            // Register default host capabilities if not already registered
            services.AddScoped<IHostCapabilities>(sp =>
            {
                var httpClient = sp.GetService<System.Net.Http.HttpClient>();
                var authStateProvider = sp.GetService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
                var configuration = sp.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
                var permissionChecker = sp.GetService<IPermissionChecker>();

                return new DefaultHostCapabilities(
                    httpClient ?? new System.Net.Http.HttpClient(),
                    authStateProvider,
                    configuration,
                    permissionChecker
                );
            });

            return services;
        }

        /// <summary>
        /// Adds NetPack Blazor JS Host services with a custom host capabilities implementation.
        /// </summary>
        public static IServiceCollection AddNetPackJsHost<THostCapabilities>(this IServiceCollection services)
            where THostCapabilities : class, IHostCapabilities
        {
            services.AddScoped<IHostCapabilities, THostCapabilities>();
            return services;
        }
    }
}
