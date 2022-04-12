namespace NetPack.Pipeline
{
    public class PipelineInputBuilder
    {

        //   private readonly IFileProvider _sourceFileProvider;
        //private readonly IHostingEnvironment _hostingEnv;
        public PipelineInputBuilder()
        {
            //  _sourceFileProvider = sourceFileProvider;
            // _sources = new Sources();
            Input = new PipeInput();
        }

        public PipelineInputBuilder Include(string pattern)
        {
            Input.AddInclude(pattern);
            return this;
        }

        public PipelineInputBuilder Exclude(string pattern)
        {
            Input.AddExclude(pattern);
            return this;
        }

        public PipeInput Input { get; set; }

    }
}