//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:https://ci.appveyor.com/nuget/gitversion-8nigugxjftrw?package=GitVersion.CommandLine&version=5.0.1"
#tool "nuget:?package=GitReleaseNotes&version=0.7.1"
#addin "nuget:?package=NuGet.Core&version=2.14.0"
#addin nuget:?package=Cake.Git&version=2.0.0


//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var artifactsDir = "./artifacts";
var solutionPath = "./src/NetPack.sln";
var globalAssemblyFile = "./src/GlobalAssemblyInfo.cs";
var repoBranchName = "master";
var isContinuousIntegrationBuild = !BuildSystem.IsLocalBuild;
var excludeProjectFromPublish = "NetPack.Web";

var gitVersionInfo = GitVersion(new GitVersionSettings {
    OutputType = GitVersionOutput.Json
});

var nugetVersion = isContinuousIntegrationBuild ? gitVersionInfo.NuGetVersion : "0.0.0";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
    Information("Building DotNetCoreBuild v{0}", nugetVersion);    
});

Teardown(context =>
{
    Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("__Default")    
    .IsDependentOn("__SetAppVeyorBuildNumber")
   // .IsDependentOn("__Clean")
    .IsDependentOn("__Restore")
   // .IsDependentOn("__UpdateAssemblyVersionInformation")
   // .IsDependentOn("__UpdateProjectJsonVersion")
    .IsDependentOn("__Build")
    .IsDependentOn("__Test")    
    .IsDependentOn("__Pack")
    //.IsDependentOn("__GenerateReleaseNotes")
    .IsDependentOn("__PublishNuGetPackages");

Task("__Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    CleanDirectories("./src/**/bin");
    CleanDirectories("./src/**/obj");
});

Task("__SetAppVeyorBuildNumber")
    .Does(() =>
{
    if (BuildSystem.AppVeyor.IsRunningOnAppVeyor)
    {
        var appVeyorBuildNumber = EnvironmentVariable("APPVEYOR_BUILD_NUMBER");
        var appVeyorBuildVersion = $"{nugetVersion}+{appVeyorBuildNumber}";
        repoBranchName = EnvironmentVariable("APPVEYOR_REPO_BRANCH");
        Information("AppVeyor branch name is " + repoBranchName);
        Information("AppVeyor build version is " + appVeyorBuildVersion);
        BuildSystem.AppVeyor.UpdateBuildVersion(appVeyorBuildVersion);
    }
    else
    {
        Information("Not running on AppVeyor");
    }    
});

Task("__Restore")
    .Does(() => 
{
	 var settings = new DotNetCoreRestoreSettings
     {      	    
        // ArgumentCustomization = args => args.Append("/p:PackageVersion=" + nugetVersion),
		 DisableParallel = true
     };

	 DotNetCoreRestore(solutionPath, settings);

});	

Task("__Build")
    .Does(() =>
{
    DotNetCoreBuild(solutionPath, new DotNetCoreBuildSettings
    {        
        Configuration = configuration,
		ArgumentCustomization = args => args.Append("--disable-parallel"),
		// Verbosity = Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity.Detailed
    });      
});

Task("__Test")
    .IsDependentOn("__Build")
    .Does(() =>
{
    GetFiles("**/*Tests/*.csproj")
        .ToList()
        .ForEach(testProjectFile => 
        {           
            var projectDir = testProjectFile.GetDirectory();
			var settings = new DotNetCoreTestSettings()
			{
			    Configuration = configuration,
				WorkingDirectory = projectDir,
				Logger = $"trx;logfilename={testProjectFile.GetFilenameWithoutExtension()}.trx",
				// ResultsDirectory = artifactsDir
			};

			DotNetCoreTest(testProjectFile.ToString(), settings);
           
        });
});


Task("__Pack")
    .Does(() =>
{

    var versionarg = "/p:PackageVersion=" + nugetVersion;
    var settings = new DotNetCorePackSettings
    {
        Configuration = "Release",
        OutputDirectory = $"{artifactsDir}",
		ArgumentCustomization = args=>args.Append(versionarg)
    };

     GetFiles("**/*.csproj")
        .ToList()
        .ForEach(projectToPackagePackageJson => 
        {           
            var projectDir = projectToPackagePackageJson.GetDirectory();
            if(!projectDir.FullPath.Contains("Tests"))
            {
                Information("Packing {0}", projectToPackagePackageJson.FullPath);
                DotNetCorePack($"{projectToPackagePackageJson.FullPath}", settings);               
            }            
        });    
              
});

Task("__GenerateReleaseNotes")
    .Does(() =>
{             
    GitReleaseNotes($"{artifactsDir}/ReleaseNotes.md", new GitReleaseNotesSettings {
    WorkingDirectory         = ".",
    Verbose                  = true,       
    RepoBranch               = repoBranchName,    
    Version                  = nugetVersion,
    AllLabels                = true
    });
});


Task("__PublishNuGetPackages")
    .Does(() =>
{              

            if(isContinuousIntegrationBuild)
            {

                var feed = new
                {
                    Name = "NuGetOrg",
                    Source = EnvironmentVariable("PUBLIC_NUGET_FEED_SOURCE")
                };
            
                NuGetAddSource(
                    name:feed.Name,
                    source:feed.Source
                );

                var apiKey = EnvironmentVariable("NuGetOrgApiKey");

                 GetFiles($"{artifactsDir}/*.{nugetVersion}.nupkg")
                .ToList()
                .ForEach(nugetPackageToPublish => 
                     {           
                        if(!nugetPackageToPublish.FullPath.Contains(excludeProjectFromPublish))
                        {
                         // Push the package. NOTE: this also pushes the symbols package alongside.
                        NuGetPush(nugetPackageToPublish, new NuGetPushSettings {
                        Source = feed.Source,
                        ApiKey = apiKey
                        });                     
                     }});                     
                 }
            });


          


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Default")
    .IsDependentOn("__Default");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);