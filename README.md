# NetPack

### What problem does it solve?

When developing an ASP.NET Core application, you want to be able to edit files like typescript files, js files, and css files in your project, and then see the changes reflected in your browser, without having to rebuild or restart your application. 

There are many solutions to this but they often require you to learn third party tooling and define external build processes like Gulp, Grunt or WebPack etc. Each one of these comes with a fair learning curve, additional complexity (config files etc), and changes to your project's build and compilation processes to accomodate them (hooks in project.json, or additional Team City build steps for example). They are also not suitable if you want to process files that are not necessarily on physical disk, but accessible at runtime, through your applications `IFileProvider`, for example, perhaps your application has some `embedded` css files.

# NetPack is different

NetPack runs within your ASP.NET Core application, because you configure it in your `startup.cs.`. It's capable of seeing all the assets that your application can see via the `IFileProvider` concept. This abtsraction means files are not restricted to those on the physical disk, but could live in plugin assemblies (embedded), azure blob storage, or from any other source for which there is an IFileProvider implementation (feel free to write your own).

NetPack is also different, because it prefers in memory processing of files. It does not need to write output to physical disk in order for your asp.net core application to serve them up to the browser. This means it can avoid disk IO for a bit of a perfomance boost (in theory! not got any perf metrics to back this up yet!)


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

So firstly we must `services.AddNetPack()` to register the NetPack services with the DI system.

Next, we can configure a `Pipeline` for our assets. Configuring a pipeline consists of:

1. Specifying the files to be processed by the pipeline.  
2. Specifying the `Pipe`'s in the pipeline.

Each `Pipe` in the pipeline takes some inputs, and the "does something" to them, and produces particular outputs. The outputs of one pipe are provided as the inputs for the next pipe. 

So we start by defining the inputs:

```

                         .WithInput((inputBuilder) 
                                     => inputBuilder
                                        .Include("wwwroot/somefile.ts")
                                        .Include("wwwroot/someOtherfile.ts"))

```

Here we include the files that we want our pipeline to process. In this case it's some typescript files only.

Next we add some `Pipe`'s in sequence to define the pipeline. We can keep adding more pipes by using the `.AddPipe(IPipe)` method, or helpful extension methods such as `.AddTypeScriptPipe(options)`.

```csharp
// shortened for brevity.
           .DefinePipeline()
             .AddTypeScriptPipe()
             .AddPipe(new SomeOtherPipe())
             .AddPipe(new AndAnotherPipe())
             .BuildPipeLine();
```

NetPack comes with a number of diffrent `Pipe`s out of the box that you can use for common tasks. For example, the `TypeScriptCompilePipe` will compile any inputs that pass through it that have a `.ts` file extension. For each one it will compile it to javascript, and then output the corresponding javascript as an output. If a file is input that isn't a `.ts` file, then it will simply be ignored and allowed to flow through the pipe untouched.

Lastly, we call `.UsePipelineOutputAsStaticFiles();` which hooks up some `StaticFiles` middleware that can serve up the `outputs` of our pipeline. In this case it will be the compiled '.js` files. 

The outputs are not actually written to disk, they are held in memory for better performance.

Lastly, if we want the pipeline to automatically re-process our files (to produce updated outputs) when our inputs change (i.e whenever we edit one of the files), we can just add a `WatchInputForChanges()` call like this:

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



