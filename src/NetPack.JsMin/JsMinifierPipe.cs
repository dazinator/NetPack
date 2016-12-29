using System;
using System.Threading;
using System.Threading.Tasks;
using NetPack.Pipeline;
using Dazinator.AspNet.Extensions.FileProviders;

namespace NetPack.JsMin
{

    /* Over the years i've fixed various bugs that have come along, I've written unit
     * tests to show that they are solved... hopefully not causing more bugs along the
     * way. I haven't seen any other C based implementations of this with these fixes,
     * though there is a python implementation which is still actively developed...
     * though looks a whole lot different.
     * Much of this has now been refactored, slightly more readable but still just as crazy.
     * - Shannon Deminick
     */

    /* Originally written in 'C', this code has been converted to the C# language.
     * The author's copyright message is reproduced below.
     * All modifications from the original to C# are placed in the public domain.
     */

    /* jsmin.c
       2007-05-22

    Copyright (c) 2002 Douglas Crockford  (www.crockford.com)

    Permission is hereby granted, free of charge, to any person obtaining a copy of
    this software and associated documentation files (the "Software"), to deal in
    the Software without restriction, including without limitation the rights to
    use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
    of the Software, and to permit persons to whom the Software is furnished to do
    so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    The Software shall be used for Good, not Evil.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
    */

    namespace NetPack.JsMin
    {

        public class JsMinifierPipe : IPipe
        {

            private JsMinOptions _options;

            public JsMinifierPipe(JsMinOptions options)
            {
                _options = options;
            }

            public async Task ProcessAsync(IPipelineContext context, CancellationToken cancelationToken)
            {
                var jsMin = new JsMin(_options);
                foreach (var item in context.InputFiles)
                {
                    var stream = item.FileInfo.CreateReadStream();
                    var minifiedContents = await jsMin.ProcessAsync(stream, cancelationToken);

                    var name = item.FileInfo.Name;
                    if (name.EndsWith(".js"))
                    {
                        name = item.FileInfo.Name.Substring(name.IndexOf(".js")) + ".min.js";
                    }
                    else
                    {
                        name = name + "min.js";
                    }

                    var outputFileName = $"{item.Directory}/{name}";
                    context.AddOutput(item.Directory, new StringFileInfo(minifiedContents, name));

                }

            }

        }
    }
}
