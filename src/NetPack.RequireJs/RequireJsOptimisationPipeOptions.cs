using System.Collections.Generic;

namespace NetPack.RequireJs
{
    public class RequireJsOptimisationPipeOptions
    {

        public RequireJsOptimisationPipeOptions()
        {
            Modules = new List<ModuleInfo>();
            Optimizer = Optimisers.uglify2;
        }

        /// <summary>
        /// The top level directory that contains your app. If this option is used
        /// then it assumed your scripts are in a subdirectory under this path.
        /// This option is not required. If it is not specified, then baseUrl
        /// below is the anchor point for finding things. If this option is specified,
        /// then all the files from the app directory will be copied to the dir:
        /// output area, and baseUrl will assume to be a relative path under
        /// this directory.
        /// </summary>
        public string AppDir { get; set; }

        /// <summary>
        /// By default, all modules are located relative to this path. If baseUrl
        /// is not explicitly set, then all modules are loaded relative to
        /// the directory that holds the build file. If appDir is set, then
        /// baseUrl should be specified as relative to the appDir.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// By default all the configuration for optimization happens from the command
        /// line or by properties in the config file, and configuration that was
        /// passed to requirejs as part of the app's runtime "main" JS file is *not*
        /// considered. However, if you prefer the "main" JS file configuration
        /// to be read for the build so that you do not have to duplicate the values
        /// in a separate configuration, set this property to the location of that
        /// main JS file. The first requirejs({}), require({}), requirejs.config({}),
        /// or require.config({}) call found in that file will be used.
        /// As of 2.1.10, mainConfigFile can be an array of values, with the last
        /// value's config take precedence over previous values in the array.
        /// </summary>
        public string MainConfigFile { get; set; }

        /// <summary>
        /// The directory path to save the output. If not specified, then
        /// the path will default to be a directory called "build" as a sibling
        /// to the build file. All relative paths are relative to the build file.
        /// </summary>
        public string Dir { get; set; }

        /// <summary>
        /// As of RequireJS 2.0.2, the dir above will be deleted before the
        /// build starts again. If you have a big build and are not doing
        /// source transforms with onBuildRead/onBuildWrite, then you can
        /// set keepBuildDir to true to keep the previous dir. This allows for
        /// faster rebuilds, but it could lead to unexpected errors if the
        /// built code is transformed in some way.
        /// </summary>
        public bool KeepBuildDir { get; set; }

        /// <summary>
        /// As of 2.1.11, shimmed dependencies can be wrapped in a define() wrapper
        /// to help when intermediate dependencies are AMD have dependencies of their
        /// own. The canonical example is a project using Backbone, which depends on
        /// jQuery and Underscore. Shimmed dependencies that want Backbone available
        /// immediately will not see it in a build, since AMD compatible versions of
        /// Backbone will not execute the define() function until dependencies are
        /// ready. By wrapping those shimmed dependencies, this can be avoided, but
        /// it could introduce other errors if those shimmed dependencies use the
        /// global scope in weird ways, so it is not the default behavior to wrap.
        /// To use shim wrapping skipModuleInsertion needs to be false.
        /// More notes in http://requirejs.org/docs/api.html#config-shim
        /// </summary>
        public bool WrapShim { get; set; }

        /// <summary>
        /// Used to inline i18n resources into the built file. If no locale
        /// is specified, i18n resources will not be inlined. Only one locale
        /// can be inlined for a build. Root bundles referenced by a build layer
        /// will be included in a build layer regardless of locale being set.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// How to optimize all the JS files in the build output directory. 
        /// Right now only the following values
        /// are supported:
        /// - "uglify": (default) uses UglifyJS to minify the code. Before version
        /// 2.2, the uglify version was a 1.3.x release. With r.js 2.2, it is now
        /// a 2.x uglify release.
        /// - "uglify2": in version 2.1.2+. Uses UglifyJS2. As of r.js 2.2, this
        /// is just an alias for "uglify" now that 2.2 just uses uglify 2.x.
        /// - "closure": uses Google's Closure Compiler in simple optimization
        /// mode to minify the code. Only available if running the optimizer using
        /// Java.
        /// - "closure.keepLines": Same as closure option, but keeps line returns
        /// in the minified files.
        /// - "none": no minification will be done.
        /// </summary>
        public Optimisers Optimizer
        {
            set
            {
                switch (value)
                {
                    case Optimisers.uglify:
                        Optimize = "uglify";
                        break;
                    case Optimisers.uglify2:
                        Optimize = "uglify2";
                        break;
                    case Optimisers.none:
                        Optimize = "none";
                        break;

                }

            }
        }

        public string Optimize { get; private set; }


        public string Out { get; set; }


        /// <summary>
        /// Introduced in 2.1.2: If using "dir" for an output directory, normally the
        /// optimize setting is used to optimize the build bundles (the "modules"
        /// section of the config) and any other JS file in the directory. However, if
        /// the non-build bundle JS files will not be loaded after a build, you can
        /// skip the optimization of those files, to speed up builds. Set this value
        /// to true if you want to skip optimizing those other non-build bundle JS
        /// files.
        /// </summary>
        public bool SkipDirOptimize { get; set; }

        /// <summary>
        /// Introduced in 2.1.2 and considered experimental.
        /// If the minifier specified in the "optimize" option supports generating
        /// source maps for the minified code, then generate them. The source maps
        /// generated only translate minified JS to non-minified JS, it does not do
        /// anything magical for translating minified JS to transpiled source code.
        /// Currently only optimize: "uglify2" is supported when running in node or
        /// rhino, and if running in rhino, "closure" with a closure compiler jar
        /// build after r1592 (20111114 release).
        /// The source files will show up in a browser developer tool that supports
        /// source maps as ".js.src" files.
        /// </summary>
        public bool GenerateSourceMaps { get; set; }

        public List<ModuleInfo> Modules { get; set; }

        /// <summary>
        /// By default, comments that have a license in them are preserved in the
        /// output when a minifier is used in the "optimize" option.
        /// However, for a larger built files there could be a lot of
        /// comment files that may be better served by having a smaller comment
        /// at the top of the file that points to the list of all the licenses.
        /// This option will turn off the auto-preservation, but you will need
        /// work out how best to surface the license information.
        /// NOTE: As of 2.1.7, if using xpcshell to run the optimizer, it cannot
        /// parse out comments since its native Reflect parser is used, and does
        /// not have the same comments option support as esprima.
        /// </summary>
        public bool PreserveLicenseComments { get; set; }

        /// <summary>
        /// Wrap any build bundle in a start and end text specified by wrap.
        /// Use this to encapsulate the module code so that define/require are
        /// not globals. The end text can expose some globals from your file,
        /// making it easy to create stand-alone libraries that do not mandate
        /// the end user use requirejs.
        /// Another way to use wrap, but uses default wrapping of:
        /// (function() { + content + }());
        /// </summary>
        public bool Wrap { get; set; }

        /// <summary>
        /// Introduced in 2.1.3: Seed raw text contents for the listed module IDs.
        /// These text contents will be used instead of doing a file IO call for
        /// those modules. Useful if some module ID contents are dynamically
        /// based on user input, which is common in web build tools.
        ///   {
        ///     'some/id': 'define(["another/id"], function () {});'
        /// },
        /// </summary>
        public string RawText { get; set; }

        public int WaitSeconds { get; set; }

        /// <summary>
        /// can be one of:
        /// "none" | "standard" | "standard.keepLines" | "standard.keepComments" | "standard.keepComments.keepLines" | "standard.keepWhitespace"
        /// </summary>
        public string CssOptimisers { get; set; }

        /// <summary>
        /// If optimizeCss is in use, a list of files to ignore for the @import
        /// inlining. The value of this option should be a string of comma separated
        /// CSS file names to ignore (like 'a.css,b.css'. The file names should match
        /// whatever strings are used in the @import calls.
        /// </summary>
        public string CssImportIgnore { get; set; }

        /// <summary>
        /// cssIn is typically used as a command line option. It can be used
        /// along with out to optimize a single CSS file.
        /// </summary>
        public string CssIn { get; set; }

        /// <summary>
        /// cssIn is typically used as a command line option. It can be used
        /// along with out to optimize a single CSS file.
        /// </summary>
        public string CssPrefix { get; set; }

        /// <summary>
        /// Inlines the text for any text! dependencies, to avoid the separate
        /// async XMLHttpRequest calls to load those dependencies.
        /// </summary>
        public bool InlineText { get; set; }

        /// <summary>
        /// Allow "use strict"; be included in the RequireJS files.
        /// Default is false because there are not many browsers that can properly
        /// process and give errors on code for ES5 strict mode,
        /// and there is a lot of legacy code that will not work in strict mode.
        /// </summary>
        public bool UseStrict { get; set; }

        /// <summary>
        /// Allows namespacing requirejs, require and define calls to a new name.
        /// This allows stronger assurances of getting a module space that will
        /// not interfere with others using a define/require AMD-based module
        /// system. The example below will rename define() calls to foo.define().
        /// See http://requirejs.org/docs/faq-advanced.html#rename for a more
        /// complete example.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Skip processing for pragmas.
        /// </summary>
        public bool SkipPragmas { get; set; }

        /// <summary>
        /// If skipModuleInsertion is false, then files that do not use define()
        /// to define modules will get a define() placeholder inserted for them.
        /// Also, require.pause/resume calls will be inserted.
        /// Set it to true to avoid this. This is useful if you are building code that
        /// does not use require() in the built project or in the JS files, but you
        /// still want to use the optimization tool from RequireJS to concatenate modules
        /// together.
        /// </summary>
        public bool SkipModuleInsertion { get; set; }

        /// <summary>
        /// If set to true, any files that were combined into a build bundle will be
        /// removed from the output folder.
        /// </summary>
        public bool RemoveCombined { get; set; }

        /// <summary>
        /// If the target module only calls define and does not call require() at the
        /// top level, and this build output is used with an AMD shim loader like
        /// almond, where the data-main script in the HTML page is replaced with just
        /// a script to the built file, if there is no top-level require, no modules
        /// will execute. specify insertRequire to have a require([]) call placed at
        /// the end of the file to trigger the execution of modules. More detail at
        /// https://github.com/requirejs/almond
        /// Note that insertRequire does not affect or add to the modules that are
        /// built into the build bundle. It just adds a require([]) call to the end
        /// of the built file for use during the runtime execution of the built code.
        /// </summary>
        public string InsertRequire { get; set; }

        /// <summary>
        /// When performing single file optimisation, this is the name of the AMD module to optimise.
        /// </summary>
        public string Name { get; set; }


        //        var RequireJsOptions = (function() {
        //    function RequireJsOptions()
        //        {
        //            this.cssImportIgnore = null;
        //            this.waitSeconds = 7;
        //            this.files = [];
        //            this.optimize = "uglify2";
        //            this.preserveLicenseComments = false;
        //            this.generateSourceMaps = true;
        //            this.appDir = null;
        //            this.baseUrl = "./";
        //            this.dir = null;
        //            this.modules = new Array();
        //            // css ptions
        //            this.optimizeCss = "standard";
        //            this.inlineText = true;
        //            this.useStrict = false;
        //        }
        //    return RequireJsOptions;
        //}());

    }
}