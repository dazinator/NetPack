using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipeline;
using System;
using Dazinator.Extensions.FileProviders.InMemory.Directory;

namespace NetPack.RequireJs
{
    public class RequireJsOptimiseTagHelper : TagHelper
    {

        private static readonly char[] _splitChars = new char[] { ',' };

        private readonly IServiceProvider _serviceProvider;

        public RequireJsOptimiseTagHelper(IServiceProvider serviceProvider, PipelineManager pipelineManager)
        {
            _serviceProvider = serviceProvider;
            //Builder = builder;
            PipelineManager = pipelineManager;
            RequireJsSrc = "/embedded/netpack/js/require.js";
            PipelineName = Guid.NewGuid().ToString();
            _pipeLine = new Lazy<IPipeLine>(() =>
            {
                return EnsurePipeline();
            });
            //  ServiceProvider = sp;
            // var builder = new pipelinebu

            // pm.PipeLines.Where(p=>p.na)

        }

        protected PipelineManager PipelineManager { get; set; }

        protected IPipelineConfigurationBuilder Builder { get; set; }

        private Lazy<IPipeLine> _pipeLine;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            // todo           
            IPipeLine pipeLine = _pipeLine.Value;

            output.TagName = "script";    // Replaces <RequireJsOptimise> with <script> tag

            //  OutPath = "/netpack

            output.Attributes.SetAttribute("src", RequireJsSrc);

            string baseRequestpath = BaseRequestPath ?? new PathString("/");

            PathString outFile = new PathString(baseRequestpath);
            if (!string.IsNullOrWhiteSpace(outFile))
            {
                outFile = outFile.Add(OutPath);
            }

            if (!string.IsNullOrWhiteSpace(OutName))
            {
                outFile = $"{outFile}/{OutName}";
            }

            output.Attributes.SetAttribute("data-main", outFile);       
            //  pipeLine.
            //   / netpack / built.js
        }

        private IPipeLine EnsurePipeline()
        {
          
            IPipeLine pipeline;
            if (PipelineManager.PipeLines.ContainsKey(PipelineName))
            {
                pipeline = PipelineManager.PipeLines[PipelineName];
                return pipeline;
            }



            IDirectory sourcesDirectory = _serviceProvider.GetRequiredService<IDirectory>();
            PipelineConfigurationBuilder builder = new PipelineConfigurationBuilder(_serviceProvider, sourcesDirectory);

            IPipelineBuilder pipeBuilder = builder.WithHostingEnvironmentWebrootProvider()
             .AddRequireJsOptimisePipe(input =>
             {
                 if (!string.IsNullOrWhiteSpace(InInclude))
                 {
                     string[] patterns = InInclude.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries);
                     foreach (string item in patterns)
                     {
                         input.Include(item);
                     }
                 }

                 if (!string.IsNullOrWhiteSpace(InExclude))
                 {
                     string[] patterns = InExclude.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries);
                     foreach (string item in patterns)
                     {
                         input.Exclude(item);
                     }
                 }

             }, o =>
             {
                 o.GenerateSourceMaps = true;
                 o.Optimizer = Optimisers.none;
                 o.BaseUrl = BaseUrl;
                 // options.
                 //  options.AppDir = "amd";
                 o.Name = Main; // The name of the AMD module to optimise.
                 o.Out = OutName; // The name of the output file.

                 // Here we list the module names
                 //options.Modules.Add(new ModuleInfo() { Name = "ModuleA" });
                 //options.Modules.Add(new ModuleInfo() { Name = "ModuleB" });
                 //  options.Modules.Add(new ModuleInfo() { Name = "SomePage" });
             })
            .UseBaseRequestPath(BaseRequestPath);

            if (Watch)
            {
                pipeBuilder.Watch();
            }

            pipeline = pipeBuilder.BuildPipeLine();
            return pipeline;
        }

        public string PipelineName { get; set; }

        public string Main { get; set; }

        public string RequireJsSrc { get; set; }

        public string OutPath { get; set; }

        public string OutName { get; set; }

        public string BaseUrl { get; set; }

        public string BaseRequestPath { get; set; }

        public bool Watch { get; set; }

        /// <summary>
        /// A comma separated list of globbed file patterns of JavaScript files to include in the rjs optimise pipe input.
        /// </summary>
        [Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeName("include")]
        public string InInclude { get; set; }

        /// <summary>
        /// A comma separated list of globbed file patterns of JavaScript files to exclude from the input of the rjs optimise pipe input.
        /// </summary>
        [Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeName("exclude")]
        public string InExclude { get; set; }

        public bool Enabled { get; set; }


    }
}
