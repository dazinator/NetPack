# NetPack
(STILL IN DEVELOPMENT)
### What problem does it solve?

When developing an ASP.NET Core application, you want to be able to edit files like typescript files, js files, and css files in your project, and then see the changes reflected in your browser, without having to rebuild or restart your application. 

There are many solutions to this but they often require you to learn third party tooling and define external build processes like Gulp, Grunt or WebPack etc. Each one of these comes with a fair learning curve, additional complexity (config files etc), and changes to your project's build and compilation processes to accomodate them (hooks in project.json, or additional Team City build steps for example). They are also not suitable if you want to include files that are not on physical dsik but accessible throuhg an IFileProvider at application runtime, for example, if your application as `embedded` files.

# NetPack is different

NetPack runs within your ASP.NET Core application, because you configure it in your `startup.cs.`. It's capable of seeing all the assets that your application can see via the `IFileProvider` concept. This abtsraction means files are not restricted to those on the physical disk, but could live in plugin assemblies (embedded), azure blob storage, or from any other source for which there is an IFileProvider implementation (feel free to write your own).

NetPack is also different, because it prefers in memory processing of files. It does not need to write output to physical disk in order for your asp.net core application to serve them up to the browser. This means it can avoid disk IO for a bit of a perfomance boost (in theory! not got any perf metrics to back this up yet!)


# How does it work?

With NetPack, once you have added the `NetPack` NuGet package to your project, in `startup.cs` you:

```
 services.AddNetPack();
 ```
 
Now, you need to construct a `Pipeline`. Think of this a number of `Pipe`s connected to together, where each `Pipe` does something you need it to when some contents pass through it. Later, we will pour some `IFileInfo`'s into the pipe for processing, and the processed `IFileInfo`'s will come out the other end.

Think sending typescript files in, and them coming out as a single minified js `IFileInfo`. 

So how is your application able to serve up the output?

NetPack has an `IFileProvider` that it wraps your existing applications `IHostingEnvironment.ContentRootFileProvider` with. NetPack's `IFileProvider` provides access to all of the `IFileInfo`'s that have been output from it's `PipeLine`s. This allows allows your asp.net core application to see these files, and it doesn't care that they are held in memory. 

Lastly NetPack has a `Watch` feature. This allows it to automatically re-process a pipeline when any of it's contetns changes. This results in updated `IFileInfo`s otuput from the pipeline whenever you make a change. This allows you to edit a .ts file, and refresh the browser to see the change.
