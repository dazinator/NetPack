using Microsoft.Extensions.FileProviders;
using Scriban;
using System;
using System.IO;

namespace NetPack.Rollup
{
    public class RollupScriptGenerator
    {
        private readonly Lazy<Template> _template;

        public RollupScriptGenerator(IFileInfo templateFile)
        {
            _template = new Lazy<Template>(() =>
            {
                using (StreamReader reader = new StreamReader(templateFile.CreateReadStream()))
                {
                    return Template.Parse(reader.ReadToEnd());
                }
            });
        }      

        public string GenerateScript(RollupInputOptions rollupOptions)
        {
            return _template.Value.Render(rollupOptions);
        }
    }
}