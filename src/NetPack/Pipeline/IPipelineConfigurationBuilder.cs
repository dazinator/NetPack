using Microsoft.Extensions.FileProviders;

namespace NetPack.Pipeline
{
    public interface IPipelineConfigurationBuilder
    {
        // IApplicationBuilder ApplicationBuilder { get; set; }

        // IPipelineInputOptionsBuilder Take(PipelineInput sources);
        IPipelineBuilder WithHostingEnvironmentWebrootProvider();
        IPipelineBuilder WithHostingEnvironmentContentProvider();
        IPipelineBuilder WithFileProvider(IFileProvider fileProvider);


    }
}