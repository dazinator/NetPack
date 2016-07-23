# NetPack
(STILL IN DEVELOPMENT)
### What problem does it solve?

When developing an ASP.NET Core application, you want to be able to edit files like typescript files, js files, and css files in your project, and then see the changes reflected in your browser, without having to rebuild or restart your application. 

There are many solutions to this but they often use third party tooling, like Gulp, Grunt, WebPack etc. In addition to this, they also often require you to start some external web server process to serve the files up to the browser.

# NetPack is different

NetPack runs within your ASP.NET Core application, because you configure it in your `startup.cs.`
NetPack can see, and process, any file that it can access via your applications `IFileProvider`. This means not only can it see files on the physical disk (like the .ts files you are editing), it can also see files that are embedded in assemblies (thanks to EmbeddedFileProvider) or from any other source for which there is an IFileProvider implementation (feel free to write your own).

NetPack is also different, because it does not need to write processed files to the physical disk in order for your application to see them and serve them up to the browser. This means it can compile your assets in memory, which is faster.

# How does it work?

With NetPack you first define a `PipeLine` which basically consists of the processing steps you want to be applied to your assets. You then define the assets that you want to be sent through the `PipeLine` and they will come out the other end of the pipeline - as a processed set of `IFileInfo` objects. Think sending typescript files in, and them coming out as a single mnified js file. NetPack has an `IFileProvider` that you hook up with your applications `IHostingEnvironment.ContentRootFileProvider` and this allows asp.net core to see the files output from the pipeline, as if they were physical files on disk, but really they live in memory. 

Lastly NetPack has a `Watch` feature, so it can detect any change to any input files, and re-process the pipleine automatically.
