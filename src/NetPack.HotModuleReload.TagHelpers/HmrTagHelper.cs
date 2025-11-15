using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetPack.HotModuleReload
{

    public class HmrScripts
    {
        public HmrScripts()
        {
            ScriptSrcIncludes = new List<string>();
        }
        public List<string> ScriptSrcIncludes { get; set; }
    }

    [HtmlTargetElement("body", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class BodyTagHelperComponent : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {

            if (string.Equals(context.TagName, "body",
                             StringComparison.OrdinalIgnoreCase))
            {
             //   StringBuilder builder = new StringBuilder();

                var scripts = new HmrScripts();
                ViewContext.ViewData[nameof(HmrScripts)] = scripts;
               // context.Items[nameof(HmrScripts)] = scripts;

                // Execute children, they can read the BodyContext                
                await output.GetChildContentAsync();

                if (scripts?.ScriptSrcIncludes?.Count > 0)
                {
                    // Add script tags after the body content but before end tag.
                    var keys = scripts.ScriptSrcIncludes; ;
                    var post = output.PostElement;
                    foreach (var item in keys)
                    {
                        // var script = scripts.ScriptSrcIncludes[item];
                        var scriptInclude = $"<script src='{item}'></script>";
                        post.AppendHtml(scriptInclude);
                    }
                }
            }
        }
    }

    public class HmrTagHelper : TagHelper
    {

        private readonly IServiceProvider _serviceProvider;

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public HmrTagHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            //Builder = builder;
            FileChangePublisherSrc = "/embedded/netpack/js/publisher-filechange.js";
            PubSubSrc = "/embedded/netpack/js/pubsub.min.js";
            ModuleReloaderSrc = "/embedded/netpack/js/reloader-system.js";
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            var viewData = ViewContext.ViewData;

            if (!viewData.ContainsKey(nameof(HmrScripts)))
            {
                return;
            }

            var options = viewData[nameof(HmrScripts)] as HmrScripts;
           
            output.TagName = "";    // Replaces <Hmr> with <script> tag

            string modReloaderSrc = ModuleReloaderSrc;
            
            if(options == null)
            {
                return;
            }

            options.ScriptSrcIncludes.Add(PubSubSrc);
            options.ScriptSrcIncludes.Add(FileChangePublisherSrc);
            options.ScriptSrcIncludes.Add(modReloaderSrc);

            // output.PreElement.AppendHtml($"<script src='{modReloaderSrc}' />");
            //output.Attributes.SetAttribute("src", modReloaderSrc);
            //  OutPath = "/netpack
            // var pre = new 
            // string pubsubsrc = PubSubSrc;
            // output.PreElement.AppendHtml($"<script src='{pubsubsrc}' />");

            // string src = FileChangePublisherSrc;
            // output.PreElement.AppendHtml($"<script src='{src}' />");

        }

        public string FileChangePublisherSrc { get; set; }
        public string PubSubSrc { get; set; }
        public string ModuleReloaderSrc { get; set; }

        //public string BaseUrl { get; set; }

        //public string BaseRequestPath { get; set; }       

        ///// <summary>
        ///// A comma separated list of globbed file patterns of JavaScript files to include in the rjs optimise pipe input.
        ///// </summary>
        //[Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeName("include")]
        //public string InInclude { get; set; }

        ///// <summary>
        ///// A comma separated list of globbed file patterns of JavaScript files to exclude from the input of the rjs optimise pipe input.
        ///// </summary>
        //[Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeName("exclude")]
        //public string InExclude { get; set; }

        //public bool Enabled { get; set; }


    }
}
