# NetPack

### What problem does it solve?

When developing an ASP.NET Core application, you often have content files in your project like:-

- Typescript
- Javascript
- Css
- Sass
- Less

`NetPack` allows you to:

1. Perform runtime pre-processing of these files, by defining a `Pipeline` using a C# fluent API.
2. Watches for changes, and re-processes the pipeline when you edit files. This enables you to edit a file, and refresh your browser and immediately see the change.
3. Works with the `IFileProvider` abtsraction - so files to be processed by a pipeline do not have to live on the physical disk
   (can be embedded etc)

`NetPack` doesn't write to the physical disk. Instead it performs all pre-processing in memory, and integrates with `StaticFiles` middleware so that asp.net core can serve up the outputs of the pipeline (i.e your modified / preprocessed files) directly from memory. This is purely for performance gain of not having to use disk IO.


# NetPack is different from Gulp / WebPack / Grunt etc etc

NetPack runs within your ASP.NET Core application to perform pre-processing at runtime. This gives it several big advantages, and one minor disadvantage over pefroming pre-processing at build time.

Disadvantages:

1. It has to preprocess your files at runtime when your application starts up, which means there is a small performance cost on startup.


Advantages:

1. `NetPack` can access all files from your applications virtual directory, provided by the `IFileProvider` - which means it can access files from anywhere, not just the physical directory. For example, you can use `NetPack` to pre-process assets (css files etc) that might be embedded in plugin assemblies, loaded dynamically at runtime.
2. `NetPack` integrates your applications `StaticFiles` middleware to allow pre-processed files to be served up from memory. This means it does not need to write anything out to physical disk.
3. You don't have to worry about running a seprate filewatcher or dev server process, just start your app running as normal.

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

So firstly we must call `services.AddNetPack()` to register the NetPack services with the DI system.

Next, we configure a `Pipeline` for our assets. Configuring a pipeline consists of:

1. Specifying the files you want to be processed by this pipeline.  
2. Defining the `Pipe`'s in the pipeline, these are the things that actually process the files and produce new outputs.

Each `Pipe` in the pipeline takes some inputs, and "does something" to them, and produces particular outputs. The outputs of one pipe are provided as the inputs for the next pipe. The outputs from the final pipe in the pipeline, are `IFileInfo`s that represent the final processed outputs, and are made visible to your applications `StaticFiles` middleware so that they can be served up to the browser.

Note: You can define multiple seperate / isolated pipelines if you wish. Just make multiple calls to  `app.UseContentPipeLine()` for each pipeline you want to define.
                
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

Under the hood this is resolving those files using the `IHostingEnvironment.ContentFileProvider`, however you can use an override to specify another `IFileProvider` to use if you wish.

Here we have now specified the files that we want our pipeline to process. 
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

NetPack comes with a number of diffrent `Pipe`s out of the box that you can use for common tasks. For example, the `TypeScriptCompilePipe` will compile any inputs that pass through it (You can think of `inputs` as a bunch of `IFileInfo`s) and it will detect the ones that have a `.ts` file extension. For each `.ts` file it will compile it to a javascript file, and then add an `IFileInfo` representing the javascript file to it's outputs. It does not output the original `.ts` file in this case. If an input file is not a `.ts` file, then it outputs it directly allowing it to flow through the pipe untouched.

Lastly, we call `.UsePipelineOutputAsStaticFiles();` which hooks up the outputs of the Pipeline to the aspnetcore `StaticFiles` middleware, allowing your application to serve up the `outputs` of our pipeline to the browser. 

We can now do this on our page:

```
 <script type="text/javascript" src="/wwwroot/somefile.js")"></script>
```

The outputs of a pipeline are not actually written to disk. If you look for `/wwwroot/somefile.js` on disk, you wont see it! They are held in memory for better performance.

Lastly, if we want our pipeline to automatically re-process all of the input files, when an input file is changed (to produce updated outputs), we can just add a `WatchInputForChanges()` call like this:

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



