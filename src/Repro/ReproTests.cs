using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Repro
{


    public class NodeServicesTests
    {

       



        public NodeServicesTests()
        {

           

        }


        [Fact]
        public async Task Maintain_State_Between_Invokes()
        {

            ServiceCollection services = new ServiceCollection();
            services.AddNodeServices(options =>
            {
                // Set any properties that you want on 'options' here
            });
            var serviceProvider = services.BuildServiceProvider();

            INodeServices nodeServices = serviceProvider.GetRequiredService<INodeServices>();

            Assembly assy = GetType().Assembly;
            string namespaceForFileProvider = assy.GetName().Name;
            EmbeddedFileProvider fileProvider = new EmbeddedFileProvider(assy, namespaceForFileProvider);

            const string scriptName = "Embedded/repro.js";
            IFileInfo script = fileProvider.GetFileInfo(scriptName);
            string scriptContent = ReadAllContent(script);

            using (var tempFile = new StringAsTempFile(scriptContent, CancellationToken.None))
            {

                // The first invoke populates node state object with two items.
                var requestDto = new RequestDto();
                requestDto.Items.Add("one", "One");
                requestDto.Items.Add("two", "Two");

                var result = await nodeServices.InvokeExportAsync<RequestDto>(tempFile.FileName, "build", requestDto);

                Assert.NotNull(result.EchoState);
                Assert.Contains(result.EchoState, (a) => a.Key == "one");
                Assert.Contains(result.EchoState, (a) => a.Key == "two");

                await Task.Delay(new TimeSpan(0, 2, 0));

                // The second invoke populates node state with an additional item.
                requestDto = new RequestDto();
                requestDto.Items.Add("three", "Three");

                result = await nodeServices.InvokeExportAsync<RequestDto>(tempFile.FileName, "build", requestDto);

                // Verify node state contains all items sent so far..


                Assert.NotNull(result.EchoState);
                Assert.Contains(result.EchoState, (a) => a.Key == "one");
                Assert.Contains(result.EchoState, (a) => a.Key == "two");
                Assert.Contains(result.EchoState, (a) => a.Key == "three");

            }


            

           

        }

        public static string ReadAllContent( IFileInfo fileInfo)
        {
            using (var reader = new StreamReader(fileInfo.CreateReadStream()))
            {
                return reader.ReadToEnd();
            }
        }

    }
}
