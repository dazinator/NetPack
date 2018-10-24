using Microsoft.Extensions.FileProviders;
using Scriban;
using System;
using System.IO;

namespace NetPack.Rollup
{
    public class RollupCodeSplittingScriptGenerator
    {
        private readonly Lazy<Template> _template;

        public RollupCodeSplittingScriptGenerator(IFileInfo templateFile)
        {
            _template = new Lazy<Template>(() =>
            {
                using (StreamReader reader = new StreamReader(templateFile.CreateReadStream()))
                {
                    return Template.Parse(reader.ReadToEnd());
                }
            });
        }

        public string GenerateScript(RollupCodeSplittingInputOptions rollupOptions)
        {
            return _template.Value.Render(rollupOptions);
        }
    }
}