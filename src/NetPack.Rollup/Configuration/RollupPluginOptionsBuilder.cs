using NetPack.Requirements;
using System;
using System.Text.Json.Nodes;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public enum OptionsKind
    {
        Object,
        Array,
    }

    public interface IRollupPluginStepConfigurationBuilder
    {
        IRollupPluginStepConfigurationBuilder ImportOnly(string defaultExportName = null);
        IRollupPluginStepConfigurationBuilder HasDefaultExportName(string name);
        IRollupPluginStepConfigurationBuilder HasOptionsOfKind(OptionsKind kind, Action<dynamic> configureOptions);
        IRollupPluginStepConfigurationBuilder RunsBeforeSystemPlugins();
        IRollupPluginStepConfigurationBuilder HasOptionsObject(Action<JsonObject> configureOptions);
        IRollupPluginStepConfigurationBuilder HasOptionsArray(Action<JsonArray> configureOptions);     
    }

    public class RollupPluginOptionsBuilder : IRollupPluginOptionsBuilder, IRollupPluginStepConfigurationBuilder
    {
        private JsonNode _options = null;
       // private JsonArray _optionsArray = null;
        // private RollupPipeOptionsBuilder _builder;
        private NpmDependency _npmDependency = null;
        private bool _importOnly = false;
        private string _defaultExportName = null;

        public NpmDependency NpmDependency { get => _npmDependency; set => _npmDependency = value; }
        public JsonNode Options { get => _options; set => _options = value; }
      //  public JsonArray OptionsArray { get => _optionsArray; set => _optionsArray = value; }

        public string DefaultExportName { get => _defaultExportName; set => _defaultExportName = value; }
        public bool IsImportOnly { get => _importOnly; set => _importOnly = value; }

        public RollupPluginOptionsBuilder()
        {
            IsImportOnly = false;
            Options = null;
        }

        public IRollupPluginStepConfigurationBuilder HasNpmDependency(Action<NpmDependencyBuilder> configureNpmModuleRequirement)
        {
            NpmDependencyBuilder builder = new NpmDependencyBuilder();
            configureNpmModuleRequirement?.Invoke(builder);
            NpmDependency = builder.BuildRequirement();
            return this;
        }

        public IRollupPluginStepConfigurationBuilder HasNpmDependency(string packageName, string version)
        {
            NpmDependency module = new NpmDependency(packageName, version);
            NpmDependency = module;
            return this;
        }       

        public IRollupPluginStepConfigurationBuilder ImportOnly(string defaultExportName = null)
        {
            IsImportOnly = true;
            DefaultExportName = defaultExportName;
            return this;
        }

        public OptionsKind OptionsKind { get; set; }

        public bool PluginRunsBeforeSystemPlugins { get; set; }

        public IRollupPluginStepConfigurationBuilder HasOptionsObject(Action<JsonObject> configureOptions)
        {
            OptionsKind =  OptionsKind.Object;
            if (configureOptions != null)
            {
              
                var options = new JsonObject();
                    configureOptions?.Invoke(options);
                    Options = options;   
            }
            return this;
        }

        public IRollupPluginStepConfigurationBuilder HasOptionsArray(Action<JsonArray> configureOptions)
        {
            OptionsKind = OptionsKind.Array;
            if (configureOptions != null)
            {
                JsonArray options = new JsonArray();
                configureOptions?.Invoke(options);
                Options = options;
            }
            return this;
        }

        public IRollupPluginStepConfigurationBuilder HasOptionsOfKind(OptionsKind kind, Action<dynamic> configureOptions)
        {
            OptionsKind = kind;
            if (configureOptions != null)
            {
                if (OptionsKind == OptionsKind.Object)
                {
                    JsonObject options = new JsonObject();
                    configureOptions?.Invoke(options);
                    Options = options;
                }
                else if (OptionsKind == OptionsKind.Array)
                {
                    JsonArray options = new JsonArray();
                    configureOptions?.Invoke(options);
                    Options = options;
                }
                else
                {
                    throw new NotSupportedException(nameof(OptionsKind) + " " + OptionsKind.ToString());
                }
            }

            return this;
        }

        public JsonNode GetJsonConfigurationObject()
        {
            return Options;
        }

        /// <summary>
        /// The name of the default export in the script used in the import statement. e.g import { defaultExportName } from 'module-name'".
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IRollupPluginStepConfigurationBuilder HasDefaultExportName(string name)
        {
            DefaultExportName = name;
            return this;
        }
       
        public IRollupPluginStepConfigurationBuilder RunsBeforeSystemPlugins()
        {
            PluginRunsBeforeSystemPlugins = true;
            return this;
        }

    }

    public interface IRollupImportConfigurationBuilder
    {
        /// <summary>
        /// The name of the default export in the script used in the import statement. e.g import { defaultExportName } from 'module-name'".
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IRollupImportConfigurationBuilder HasDefaultExportName(string name);
    }

    public class RollupImportOptionsBuilder : IRollupImportOptionsBuilder, IRollupImportConfigurationBuilder
    {
       
        private NpmDependency _npmDependency = null;       
        private string _defaultExportName = null;

        public NpmDependency NpmDependency { get => _npmDependency; set => _npmDependency = value; }       

        public string DefaultExportName { get => _defaultExportName; set => _defaultExportName = value; }       

        public RollupImportOptionsBuilder()
        {           
        }

        public IRollupImportConfigurationBuilder HasNpmDependency(Action<NpmDependencyBuilder> configureNpmModuleRequirement)
        {
            NpmDependencyBuilder builder = new NpmDependencyBuilder();
            configureNpmModuleRequirement?.Invoke(builder);
            NpmDependency = builder.BuildRequirement();
            return this;
        }

        public IRollupImportConfigurationBuilder HasNpmDependency(string packageName, string version)
        {
            NpmDependency module = new NpmDependency(packageName, version);
            NpmDependency = module;
            return this;
        }       

        /// <summary>
        /// The name of the default export in the script used in the import statement. e.g import { defaultExportName } from 'module-name'".
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IRollupImportConfigurationBuilder HasDefaultExportName(string name)
        {
            DefaultExportName = name;
            return this;
        }

    }
}