using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NetPack.Pipeline;

namespace NetPack.RequireJs
{
    public class RequireJsOptimiseTagHelper : TagHelper
    {
        public RequireJsOptimiseTagHelper(IPipelineConfigurationBuilder builder, PipelineManager pipelineManager)
        {
            Builder = builder;
            PipelineManager = pipelineManager;
            RequireJsSrc = "/lib/require.js";
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

            var outFile = this.BaseRequestPath + this.OutPath + this.OutName;
            output.Attributes.SetAttribute("data-main", outFile);



            //  pipeLine.


            //   / netpack / built.js


        }

        private IPipeLine EnsurePipeline()
        {
            // var appServices = builder.ApplicationBuilder.ApplicationServices;
            //var nodeServices = (INodeServices)appServices.GetRequiredService(typeof(INodeServices));

            // add requirements to the pipeline to check nodejs is installed, and the npm packages we need.
            IPipeLine pipeline;
            if (this.PipelineManager.PipeLines.ContainsKey(this.PipelineName))
            {
                pipeline = this.PipelineManager.PipeLines[this.PipelineName];
                return pipeline;
            }



            var pipeBuilder = Builder.WithHostingEnvironmentWebrootProvider()
             .AddRequireJsOptimisePipe(input =>
             {
                 input.Include(this.In);
             }, o =>
             {
                 o.GenerateSourceMaps = true;
                 o.Optimizer = Optimisers.none;
                 o.BaseUrl = BaseUrl;
                 // options.
                 //  options.AppDir = "amd";
                 o.Name = this.Main; // The name of the AMD module to optimise.
                 o.Out = this.OutName; // The name of the output file.

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

        public string In { get; set; }

        public string PipelineName { get; set; }

        public string Main { get; set; }

        public string RequireJsSrc { get; set; }

        public string OutPath { get; set; }

        public string OutName { get; set; }

        public string BaseUrl { get; set; }

        public string BaseRequestPath { get; set; }

        public bool Watch { get; set; }


    }
}
