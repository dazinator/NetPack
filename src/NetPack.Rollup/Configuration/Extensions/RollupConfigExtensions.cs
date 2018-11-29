using System;

namespace NetPack
{
    public static partial class RollupConfigExtensions
    {
        /// <summary>
        /// Imports the module-lookup-amd module into the rollup build script, so that other scripts and plugin functions can lookup AMD module information from a requireJS style config file.
        /// Basically makes a function called `lookup` available which can be used as described: https://github.com/dependents/node-module-lookup-amd
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="version"></param>
        /// <param name="defaultExportName"></param>
        /// <returns></returns>
        public static RollupPipeOptionsBuilder ImportModuleLookupAmd(this RollupPipeOptionsBuilder builder, string version = "5.0.1", string defaultExportName = "lookup")
        {
            builder.AddImport((a) =>
             {
                 a.RequiresNpmModule("module-lookup-amd", version)
                  .HasDefaultExportName("lookup");   // only imported into the script as default export name, won't be included in rollupjs plugins list, but other plugin config could reference it.                                       
             });
            return builder;
        }

        /// <summary>
        /// Adds a rollup plugin that can transpile AMD module scripts to ES6 style modules.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static RollupPipeOptionsBuilder AddPluginAmd(this RollupPipeOptionsBuilder builder, Action<RollupPluginAmdOptionsBuilder> configureOptions = null, string version = "3.0.0")
        {
            builder.AddPlugin((a) =>
             {
                 IRollupPluginStepConfigurationBuilder stepBuilder = a.RequiresNpmModule("rollup-plugin-amd", version);
                 if (configureOptions != null)
                 {
                     stepBuilder.HasOptionsOfKind(OptionsKind.Object, (amdPluginOptions) =>
                       {
                           RollupPluginAmdOptionsBuilder adaptor = new RollupPluginAmdOptionsBuilder(amdPluginOptions);
                           configureOptions?.Invoke(adaptor);
                       });
                 }
             });
            return builder;
        }
    }
}
