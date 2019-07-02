# NetPack

| Branch  | Build Status | NuGet |
| ------------- | ------------- | ----- |
| Master  |[![Build master](https://ci.appveyor.com/api/projects/status/2ri02762ca2dicfp/branch/master?svg=true)](https://ci.appveyor.com/project/dazinator/netpack/branch/master) | [![NuGet](https://img.shields.io/nuget/v/netpack.svg)](https://www.nuget.org/packages/netpack/) |
| Develop | [![Build develop](https://ci.appveyor.com/api/projects/status/2ri02762ca2dicfp?svg=true)](https://ci.appveyor.com/project/dazinator/netpack/branch/develop)  | [![NuGet](https://img.shields.io/nuget/vpre/netpack.svg)](https://www.nuget.org/packages/netpack/) |


### What problem does it solve?

NetPack runs within your own asp.net core application (configured in startup.cs in the usual mannor) and handles all of your file processing needs whilst your application is running. It also includes some optional supporting middlewares, to enable "automatic browser refresh" and "Hot Module Reload" etc.

NetPack uses the concept of "pipelines" which allow you to define which files from an `IFileProvider` you want to process (using Include, and Exclude patterns) and then the run actions on those files as necessary. A number of pipelines are provided out of the box for doing all sorts of things. Please run the `NetPack.Web` project and visit the different pages to see various examples.

Pipelines can generate outputs. When an output is generated, it may trigger another pipeline to run (chain effect) if another pipline is watching for that input. In this mannor you can set up workflows by chaining pipelines together.

Some pipelines that NetPack provides out of the box, actually send your files to an NPM process managed by the pipleline, so that processing can occur using NPM libraries, and the output files pipe backed into .NET Core. For example, the `Rollup` pipeline does this.

NetPack can watch the inputs to pipelines, and re-trigger them to process if any oinputs change.

You can use NetPack for:

- [x] Typescript Compilation
- [x] Combining multiple Javascript files into a single file, with optional source map.
- [x] rollupjs processing (Define your processing workflow with rollup, including use of rollup plugins). Using rollup there is probably nothing you can't achieve, thanks to it's wide array of plugins. You can configure rollupjs piepline purely through `c#` thanks to NetPack.
- [x] rjs optimisation. (Optimises your AMD javascript files, into bundles, using the rjs optimiser.)
- [x] Browser Reload - Automatically triggers your browser window to `reload()` when desired files are changed.
- [x] Hot Module Reload (for SystemJS) - Automatically reloads a SystemJS module as you make changes. Allows a quicker feedback loop than a full browser reload/
- [ ] Hot Module Reload (for RequireJS) - WIP. This might not be possible as RequireJS doesn't have as much support for HMR as SystemJS it seems, but still investigating.
- [x] Blazor app reload (Automatic recompilation of .razor files, and reloading of blazor client apps in the browser as you make changes at runtime).


Typically, generated output files, are held in memory. NetPack provides an `IFileProvider` that sits on top of this In Memory Directory and can see all outputs just like ordinary files that might live on disk - they have a name and directory etc. NetPack includes this IFileProvider in with StaticFilesMiddleware so that these generated outputs can be served up to the browser.
accessible by your applications IHotingEnvironment.WebRootFileProvider. 

# Stay in ASP.NET Core land.

During development, when I start my website, I want it to be watching source files for changes, and I want it to be responsive to those changes so that I get a quick feedback loop.
I don't want to have to switch to start up another dev server or external npm process, I just want stuff to start when I click the "play" button in visual studio, and stop when I click the stop button.

Gulp, Webpack, Grunt etc are all great tools, but they also shift you somewhat into a new paradigm of config files, and javascript build processes that not all asp.net core developers would be immediatley productive with, but perhaps would be fine for more seasoned `NPM` developers. I personally would much rather have an API for file pre-processing that is closer to the C# / ASP.NET paradigm of my ASP.NET Core application - and not have to switch hats into the land of gulp or webpack to get things done. 

# How does it work?

Let me show you a startup.cs file, and then i'll break it down..

```

public class Startup
{

    public void ConfigureServices(IServiceCollection services)
    {
            services.AddNetPack((setup) =>
            {
                setup.AddPipeline(pipelineBuilder =>
                {
                    pipelineBuilder.WithHostingEnvironmentWebrootProvider()                 
                   
                    // Let's use requirejs optimiser (rjs) to optimise our AMD javascript files for production to a `built.js` file.
                    .AddRequireJsOptimisePipe(input =>
                    {
                        input.Include("amd/*.js")
                        .Include("js/requireConfig.js"); // additional configuration for rjs.
                    }, options =>
                    {
                        options.GenerateSourceMaps = true;
                        options.Optimizer = Optimisers.none;
                        options.BaseUrl = "amd";                        
                        options.Name = "SomePage"; // The name of the entry point AMD module to optimise.
                        options.Out = "built.js"; // The name of the output file.                      
                    })                    
                    // could continue to add more pipes here, such as rollupjs etc etc.

                    .UseBaseRequestPath("/netpack") // prepends "/netpack" to all outputs from all pipes. So the output "/built.js" is now served up on `/netpack/built.js`
                    .Watch(); // Inputs for all pipes are monitored, and when changes occur, the pipe will be triggered to re-process its inputs.

                });
            });
            
            // optionally let's also use netpack's auto browser refresh! (Note there is a script tag to include in layout.cshtml or the page which acts as the browser refresh client to get browser refresh to work.)
            services.AddBrowserReload((options) =>
            {
                // trigger browser reload when our bundle file changes.
                options.WatchWebRoot("/netpack/built.js");
                options.WatchContentRoot("/Views/**/*.cshtml");
            });
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
            
             app.UseNetPack();
             app.UseStaticFiles();
             app.UseBrowserReload();             

             app.UseMvc();
        }


```

The call to `.UseBaseRequestPath("/netpack")` means that all of the generated outputs from the pipes that NetPack produces will have "/netpack" prepended to their request path (the path they will be served to the browser on). So the output file `built.js` will become `/netpack/built.js` 

So we can now do this on our page:

```
 <script type="text/javascript" src="/netpack/built.js")"></script>
```

The outputs of a pipeline are not actually written to disk. If you look for a `/netpack/built.js` on disk, you wont see it! They are held in memory (transient) for better performance (avoiding disk io).

Lastly, by calling `Watch()` we ensure that when any of the inputs to a pipeline change, it will automatically re-process.  


Now whenever we edit `amd/ModuleA.js` or `amd/ModuleB.js` with our application running, netpack will automatically process the files to produce an updated `build.js` file, which will be made accessbile to the browser on `/netpack/built.js`. Because we have configured browser reload to trigger a reload when `/netpack/built.js` changes, if we have a browser open and the page has our browser reload script loaded, it will just automatically refresh for us.

The following shows a page with the script tags for this scenario:
The top two scripts are required for automatic browser refresh (signalr script you have to source yourself, the `reload` script is embedded from netpack assembly, so you don't have to worry about sourcing that one.
The bottom two scripts are the bundle file, and `requirejs` itself which we happen to be using in our scenario as our bundle is in AMD format so it won't load without `requirejs` being on the page.

```

    <script src="~/lib/signalr/dist/browser/signalr.js"></script>
    <script src="~/js/reload.js"></script> @*This reload script file is embedded into a netpack assembly, you don't have to source it yourself*@

    <script src="~/lib/require.js/require.min.js"></script>
    <script src="~/netpack/built.js" asp-append-version="true"></script>


```

To see this in action, run the `NetPack.Web` project, and browse to the `BrowserReload` page using the nav links. The `NetPack.Web` project is a website with sample pages demoing various scenarios.


