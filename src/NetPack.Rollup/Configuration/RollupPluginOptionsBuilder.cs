using NetPack.Requirements;
using Newtonsoft.Json.Linq;
using System;

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
        IRollupPluginStepConfigurationBuilder HasOptionsObject(Action<JObject> configureOptions);
        IRollupPluginStepConfigurationBuilder HasOptionsArray(Action<JArray> configureOptions);     
    }

    public class RollupPluginOptionsBuilder : IRollupPluginOptionsBuilder, IRollupPluginStepConfigurationBuilder
    {
        private JObject _options = null;
        private JArray _optionsArray = null;
        // private RollupPipeOptionsBuilder _builder;
        private NpmModuleRequirement _moduleRequirement = null;
        private bool _importOnly = false;
        private string _defaultExportName = null;

        public NpmModuleRequirement ModuleRequirement { get => _moduleRequirement; set => _moduleRequirement = value; }
        public JObject Options { get => _options; set => _options = value; }
        public JArray OptionsArray { get => _optionsArray; set => _optionsArray = value; }

        public string DefaultExportName { get => _defaultExportName; set => _defaultExportName = value; }
        public bool IsImportOnly { get => _importOnly; set => _importOnly = value; }

        public RollupPluginOptionsBuilder()
        {
            IsImportOnly = false;
            Options = null;
            OptionsArray = null;
        }

        public IRollupPluginStepConfigurationBuilder RequiresNpmModule(Action<NpmModuleRequirementBuilder> configureNpmModuleRequirement)
        {
            NpmModuleRequirementBuilder builder = new NpmModuleRequirementBuilder();
            configureNpmModuleRequirement?.Invoke(builder);
            ModuleRequirement = builder.BuildRequirement();
            return this;
        }

        public IRollupPluginStepConfigurationBuilder RequiresNpmModule(string packageName, string version, bool installAutomatically = true)
        {
            NpmModuleRequirement module = new NpmModuleRequirement(packageName, installAutomatically, version);
            ModuleRequirement = module;
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

        public IRollupPluginStepConfigurationBuilder HasOptionsObject(Action<JObject> configureOptions)
        {
            OptionsKind =  OptionsKind.Object;
            if (configureOptions != null)
            {
              
                    JObject options = new JObject();
                    configureOptions?.Invoke(options);
                    Options = options;   
            }
            return this;
        }

        public IRollupPluginStepConfigurationBuilder HasOptionsArray(Action<JArray> configureOptions)
        {
            OptionsKind = OptionsKind.Array;
            if (configureOptions != null)
            {
                JArray options = new JArray();
                configureOptions?.Invoke(options);
                OptionsArray = options;
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
                    JObject options = new JObject();
                    configureOptions?.Invoke(options);
                    Options = options;
                }
                else if (OptionsKind == OptionsKind.Array)
                {
                    JArray options = new JArray();
                    configureOptions?.Invoke(options);
                    OptionsArray = options;
                }
                else
                {
                    throw new NotSupportedException(nameof(OptionsKind) + " " + OptionsKind.ToString());
                }
            }

            return this;
        }

        public object GetJsonConfigurationObject()
        {
            return (object)Options ?? OptionsArray;
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
       
        private NpmModuleRequirement _moduleRequirement = null;       
        private string _defaultExportName = null;

        public NpmModuleRequirement ModuleRequirement { get => _moduleRequirement; set => _moduleRequirement = value; }       

        public string DefaultExportName { get => _defaultExportName; set => _defaultExportName = value; }       

        public RollupImportOptionsBuilder()
        {           
        }

        public IRollupImportConfigurationBuilder RequiresNpmModule(Action<NpmModuleRequirementBuilder> configureNpmModuleRequirement)
        {
            NpmModuleRequirementBuilder builder = new NpmModuleRequirementBuilder();
            configureNpmModuleRequirement?.Invoke(builder);
            ModuleRequirement = builder.BuildRequirement();
            return this;
        }

        public IRollupImportConfigurationBuilder RequiresNpmModule(string packageName, string version, bool installAutomatically = true)
        {
            NpmModuleRequirement module = new NpmModuleRequirement(packageName, installAutomatically, version);
            ModuleRequirement = module;
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