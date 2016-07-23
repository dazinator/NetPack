# NetPack
(STILL IN DEVELOPMENT)
### What problem does it solve?

When developing an ASP.NET Core application, you want to be able to edit files like typescript files, js files, and css files in your project, and then see the changes reflected in your browser, without having to rebuild or restart your application. 

There are many solutions to this but they often require you to learn third party tooling and define external build processes like Gulp, Grunt or WebPack etc. Each one of these comes with a fair learning curve, additional complexity (config files etc), and changes to your project's build and compilation processes to accomodate them (hooks in project.json, or additional Team City build steps for example). They are also not suitable if you want to process files that are not necessarily on physical disk, but accessible at runtime, through your applications `IFileProvider`, for example, perhaps your application has some `embedded` css files.

# NetPack is different

NetPack runs within your ASP.NET Core application, because you configure it in your `startup.cs.`. It's capable of seeing all the assets that your application can see via the `IFileProvider` concept. This abtsraction means files are not restricted to those on the physical disk, but could live in plugin assemblies (embedded), azure blob storage, or from any other source for which there is an IFileProvider implementation (feel free to write your own).

NetPack is also different, because it prefers in memory processing of files. It does not need to write output to physical disk in order for your asp.net core application to serve them up to the browser. This means it can avoid disk IO for a bit of a perfomance boost (in theory! not got any perf metrics to back this up yet!)


# How does it work?

With NetPack, once you have added the `NetPack` NuGet package to your project, in `startup.cs` you:

```
 services.AddNetPack();
 ```
 
Next, you need to construct a `Pipeline` for your assets. Think of this as just a number of `Pipe`s connected to together, where each `Pipe` "does something" that you need it to when some contents pass through it. 

```csharp

 app.UseNetPackPipeLine(builder =>
                    builder.AddTypeScriptPipe(tsConfig =>
                    {
                        tsConfig.Target = TypeScriptPipeOptions.ScriptTarget.Es5;
                        tsConfig.Module = TypeScriptPipeOptions.ModuleKind.CommonJs;
                        tsConfig.NoImplicitAny = true;
                        tsConfig.RemoveComments = true;
                        tsConfig.SourceMap = true;
                    })
                    .AddPipe(new BundlePipe("/somedir/bundle1.js"))
                );

```

NetPack comes with a number of diffrent `Pipe`s out of the box that you can use for common tasks. For example, the `TypeScriptCompilationPipe` will compile any  `.ts` `IFileInfo`'s that pass through it, and output the corresponding `.js` `IFileInfo`s for the compiled js files.


Once we have a `Pipeline` defined, we need to give it some source files to process.

```

           var fileProvider = HostingEnvironment.ContentRootFileProvider;

            var sources = new SourcesBuilder(fileProvider);
            sources
                .Include("wwwroot/somefile.ts")
                .Include("wwwroot/someOtherfile.ts")
                .Sources;
                
                app.UseNetPackPipeLine(builder =>
                    builder.AddTypeScriptPipe(tsConfig =>
                    {
                        tsConfig.Target = TypeScriptPipeOptions.ScriptTarget.Es5;
                        tsConfig.Module = TypeScriptPipeOptions.ModuleKind.CommonJs;
                        tsConfig.NoImplicitAny = true;
                        tsConfig.RemoveComments = true;
                        tsConfig.SourceMap = true;
                    })
                    .AddPipe(new BundlePipe("/somedir/bundle1.js"))
                    .Pipeline()
                    .SetSourceFiles(sources)
                );


```

Those files will now be put through the pipeline, and the resulting `IFileInfo` (in this case /somedir/bundle1.js) will be visible to asp.net via the `HostingEnvironment.ContentRootFileProvider`.

Lastly, if we want to automatically rebuild the bundle file whenever we change a source file, we can add a `Watch` call like this:

```
                app.UseNetPackPipeLine(builder =>
                    builder.AddTypeScriptPipe(tsConfig =>
                    {
                        tsConfig.Target = TypeScriptPipeOptions.ScriptTarget.Es5;
                        tsConfig.Module = TypeScriptPipeOptions.ModuleKind.CommonJs;
                        tsConfig.NoImplicitAny = true;
                        tsConfig.RemoveComments = true;
                        tsConfig.SourceMap = true;
                    })
                    .AddPipe(new BundlePipe("/somedir/bundle1.js"))
                    .Pipeline()
                    .SetSourceFiles(sources).Watch()
                );


```


