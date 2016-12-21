# NetPack

| Branch  | Build Status | NuGet |
| ------------- | ------------- | ----- |
| Master  |[![Build master](https://ci.appveyor.com/api/projects/status/2ri02762ca2dicfp/branch/master?svg=true)](https://ci.appveyor.com/project/dazinator/netpack/branch/master) | [![NuGet](https://img.shields.io/nuget/v/netpack.svg)](https://www.nuget.org/packages/netpack/) |
| Develop | [![Build develop](https://ci.appveyor.com/api/projects/status/2ri02762ca2dicfp?svg=true)](https://ci.appveyor.com/project/dazinator/netpack/branch/develop)  | [![NuGet](https://img.shields.io/nuget/vpre/netpack.svg)](https://www.nuget.org/packages/netpack/) |


### What problem does it solve?

NetPack is a very easy to use library, that performs file processing for your asp.net core application at runtime. 
NetPack will watch your files, and re-process them when they change.
NetPack currently has processors for:

- [x] Typescript Compilation
- [x] Combining multiple Javascript files into a single file, with optional source map.
- [ ] Css minification
- [ ] Css bundling (based on @import)
- [ ] Javascript minficatiion
- [x] rjs optimisation. (Optimises your AMD javascript files, into bundles, using the rjs optimiser.)
- [ ] [SystemJS Production workflows](https://github.com/systemjs/systemjs/blob/master/docs/production-workflows.md)

`NetPack` is easy. NetPack will:

1. Use C# fluent API to setup your file pre-processing needs in `startup.cs`
2. Pre-process any files that are accessbile to your ASP.NET Core application at runtime.
3 `Watch` for changes to any of those files, and re-processes when they change. This enables you to edit a source file (e.g Typescript) whilst your application is running, and then refresh your browser and see the updated javascript file from the pipeline.
4. Works with the `IFileProvider` abtsraction - so files to be processed by a pipeline do not have to live on the physical disk
   (can be embedded files etc)

`NetPack` performs all pre-processing in memory. It allows generated files to be served up to the browser by integrating it's own InMemory FileProvider with your applications IHostingEnvironment.WebRootFileProvider.

All generated files, are held in memory, and accessible by your applications IHotingEnvironment.WebRootFileProvider. This means all of the standard asp.net mvc TagHelpers will resolve any generated scripts that NetPack produces..

# NetPack is different from Gulp / WebPack / Grunt etc etc

Each of those tools are great. But they also shift you somewhat into a new paradigm of config files, and javascript build processes that not all .NET AspNet developers would be immediatley productive with, but perhaps would be fine for more seasoned `NPM` developers, but less so for developers who may not have used these external tool chains before. I personally would much rather have an API for file pre-processing that is closer to the C# / ASP.NET paradigm of my ASP.NET Core application - and not have to switch hats into the land of gulp or webpack to get things done. 

# How does it work?

Let me show you a startup.cs file, and then i'll break it down..

```

        public class Startup
        {

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddNetPack();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
            
             app.UseStaticFiles();
             
             app.UseFileProcessing(a =>
             {
                a.WithHostingEnvironmentWebrootProvider()
                    // Simple processor, that compiles typescript files.
                    .AddTypeScriptPipe(input =>
                    {
                        input.Include("ts/*.ts");
                   }, options =>
                    {
                        // options.InlineSourceMap = true;
                        options.InlineSources = true;
                        // configure various typescript compilation options here..
                        // options.InlineSourceMap = true;
                        //  options.Module = ModuleKind.Amd;
                    })
                    // Another processor that combines multiple js files into a bundle file.
                    .AddJsCombinePipe(input =>
                    {
                        input.Include("ts/*.js");
                    }, () => "bundle.js")
                    .UseBaseRequestPath("netpack") // serves all outputs using the specified base request path.
                    .Watch(); // Inputs are monitored, and when changes occur, pipes will automatically re-process.
             });

             app.UseMvc();
        }


```


NetPack comes with a number of diffrent `Pipe`s out of the box that you can use for common tasks. For example, the `TypeScriptCompilePipe` will compile any typescript files (.ts) that pass through it, and will do so using the options you specify. There are all the standard options such as to remove comments etc etc. For each `.ts` file it will the output a `.js` file, as well as the sourcemap if sourcemaps are enabled. 

Call `.UseBaseRequestPath("netpack")` means that all of the generated outputs that NetPack produces will be served with a base request path of "netpack". So the request path to `bundle.js` in the browser becomes `/netpack/bundle.js` 

We can now do this on our page:

```
 <script type="text/javascript" src="/netpack/bundle.js")"></script>
```

The outputs of a pipeline are not actually written to disk. If you look for a `/netpack/bundle.js` on disk, you wont see it! They are held in memory for better performance.

Lastly, by calling `Watch()` we ensure that when any of the inputs to a processor changes, it will automaitcally re-process.  


Now whenever we edit `ts/somefile.ts` or `ts/someOtherfile.ts` with our application running, netpack will automatically process the files to produce js files. This will effectively spit out a new `ts/somefile.js` and `ts/someOtherfile.js` file in memory. This will then trigger the `Combine` processor to run, which will spit out a new `bundle.js` file, which is served on `netpack/bundle.js`. So when we refresh the browser, we will see the changes.



