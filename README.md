# NetPack

### What problem does it solve?

When developing an ASP.NET Core application, you often have content files in your project like:-

- Typescript
- Javascript
- Css
- Sass
- Less

`NetPack` allows you to:

1. Define your pre-processing requirements using a C# fluent API.
2. Pre-process any files that are visible to your ASP.NET Core application (via an IFileProvider), at runtime.
2. Can `Watch` for changes to any of those files, and re-processes the pipeline when files change. This enables you to edit a source file (e.g Typescript) whilst your application is running, and then refresh your browser and see the updated javascript file from the pipeline.
3. Works with the `IFileProvider` abtsraction - so files to be processed by a pipeline do not have to live on the physical disk
   (can be embedded files etc)

`NetPack` performs all pre-processing in memory, and it then it allows the outputs (processed files) to be served up by your ASP.NET Core application by integrating with the `StaticFiles` middleware.

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
                app.UseContentPipeLine(pipelineBuilder =>
                {
                    return pipelineBuilder
                        .WithInput((inputBuilder) 
                                     => inputBuilder
                                        .Include("wwwroot/somefile.ts")
                                        .Include("wwwroot/someOtherfile.ts"))
                        .DefinePipeline()
                            .AddTypeScriptPipe(tsConfig =>
                                     {
                                         tsConfig.Target = TypeScriptPipeOptions.ScriptTarget.Es5;
                                         tsConfig.Module = TypeScriptPipeOptions.ModuleKind.CommonJs;
                                         tsConfig.NoImplicitAny = true;
                                         tsConfig.RemoveComments = true;
                                         tsConfig.SourceMap = true;
                                     })
                           //.AddPipe(someOtherPipe) can add more pipes like minification, bundling etc.
                        .BuildPipeLine();
                })
                .UsePipelineOutputAsStaticFiles(); // makes the files output from the pipepline visible to static files middleware
            }
        }


```

So, straight off, you can tell this is a proper ASP.NET Core library because we call `services.AddNetPack()` to register the NetPack services with the DI system :-)

Next, we (fluently?) configure a `Pipeline` for our assets. Intellisense every step of the way baby!
Configuring a pipeline consists of:

1. Specifying the files you want to be processed by this pipeline.  
2. Defining the `Pipe`'s in the pipeline, these are the things that actually process the files and produce new outputs.

Each `Pipe` in the pipeline takes some inputs, and "does something" to them, and produces some outputs. 
The outputs of one pipe are provided as the inputs for the next pipe.
The outputs from the final pipe in the pipeline, are visible to the `StaticFiles` middleware, and therefore can be served up to the browser. 

Note: You can create multiple independent pipelines if you wish. Just make multiple calls to  `app.UseContentPipeLine()` for each pipeline you want to define.
                
So we start by defining the inputs for the pipeline:

```

                       app.UseContentPipeLine(pipelineBuilder =>
                {
                    return pipelineBuilder
                        .WithInput((inputBuilder) 
                                     => inputBuilder
                                        .Include("wwwroot/somefile.ts")
                                        .Include("wwwroot/someOtherfile.ts"))

```

By default, these paths are resolved to actual file content, using the `IHostingEnvironment.ContentFileProvider`, however you can use an override to specify another `IFileProvider` to use if you wish.

Here we have now specified the files that we want to be input into our pipeline for processing. Simples so far. 
In this case we have specified some typescript files only.

Next we define the `Pipe`'s in the pipeline, in the order we want them to process in. We can keep adding more pipes by using the `.AddPipe(IPipe)` method, or helpful extension methods such as `.AddTypeScriptPipe(options)`.

```csharp
// shortened for brevity.
           .DefinePipeline()
             .AddTypeScriptPipe()
             .AddPipe(new SomeOtherPipe())
             .AddPipe(new AndAnotherPipe())
             .BuildPipeLine();
```

NetPack comes with a number of diffrent `Pipe`s out of the box that you can use for common tasks. For example, the `TypeScriptCompilePipe` will compile any typescript files (.ts) that pass through it, and will do so using the options you specify. There are all the standard options such as to remove comments etc etc. For each `.ts` file it will the output a `.js` file. It does not output the original `.ts` file meaning that all .ts files will stop progressing through the pipeline at this pipe. If an input file is not a `.ts` file, then it is ignored and allowed it to flow through the pipe untouched.

Lastly, we call `.UsePipelineOutputAsStaticFiles();` which hooks up the outputs of the Pipeline to the aspnetcore `StaticFiles` middleware, allowing your application to serve up the `outputs` of our pipeline to the browser. 

We can now do this on our page:

```
 <script type="text/javascript" src="/wwwroot/somefile.js")"></script>
```

The outputs of a pipeline are not actually written to disk. If you look for `/wwwroot/somefile.js` on disk, you wont see it! They are held in memory for better performance.

Lastly, if we want our pipeline to automatically re-process the input files, when an input file is changed (to produce updated outputs), we can just add a `WatchInputForChanges()` call like this:

```

 app.UseContentPipeLine(pipelineBuilder =>
                {
                    return pipelineBuilder
                        //.AddPipe(someOtherPipe)
                        .WithInput((inputBuilder) 
                                     => inputBuilder
                                        .Include("wwwroot/somefile.ts")
                                        .Include("wwwroot/someOtherfile.ts"))
                                        .WatchInputForChanges()
                        .DefinePipeline()
                            .AddTypeScriptPipe(tsConfig =>
                                     {
                                         tsConfig.Target = TypeScriptPipeOptions.ScriptTarget.Es5;
                                         tsConfig.Module = TypeScriptPipeOptions.ModuleKind.CommonJs;
                                         tsConfig.NoImplicitAny = true;
                                         tsConfig.RemoveComments = true;
                                         tsConfig.SourceMap = true;
                                     })
                        .BuildPipeLine();
                })
                .UsePipelineOutputAsStaticFiles();

```

Now whenever we edit `wwwroot/somefile.ts` or `wwwroot/someOtherfile.ts` with our application running, the pipeline will automatically re-run agains the up to date inputs. This will effectively spit out a new `wwwroot/somefile.js` and `wwwroot/someOtherfile.js` file in memory, and when we refresh the browser thats referencing those files, we see the change.



