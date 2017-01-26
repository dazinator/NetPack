using System;
using System.Collections.Generic;

namespace NetPack.Typescript
{
    public class TypeScriptPipeOptions
    {

        public TypeScriptPipeOptions()
        {
            SourceMap = true;
            NoImplicitAny = true;
            Module = ModuleKind.Amd;
            Target = ScriptTarget.Es6;
            InlineSourceMap = false;
        }

        /// <summary>
        /// Allow JavaScript files to be compiled.
        /// </summary>
        public bool? AllowJs { get; set; }

        /// <summary>
        /// Allow default imports from modules with no default export. This does not affect code emit, just typechecking.
        /// </summary>
        public bool? AllowSyntheticDefaultImports { get; set; }

        /// <summary>
        /// Do not report errors on unreachable code.
        /// </summary>
        public bool? AllowUnreachableCode { get; set; }


        /// <summary>
        /// Do not report errors on unused lab
        /// </summary>
        public bool? AllowUnusedLabels { get; set; }

        /// <summary>
        /// Parse in strict mode and emit "use strict" for each source file
        /// </summary>
        public bool? AlwaysStrict { get; set; }

        /// <summary>
        /// Base directory to resolve non-relative module names. See Module Resolution documentation for more details.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// The character set of the input files.
        /// </summary>
        public string Charset { get; set; }

        /// <summary>
        /// Generates corresponding .d.ts file
        /// </summary>
        public bool? Declaration { get; set; }

        /// <summary>
        /// Output directory for generated declaration files.
        /// </summary>
        public string DeclarationDir { get; set; }

        /// <summary>
        /// Show diagnostic information.
        /// </summary>
        public bool? Diagnostics { get; set; }

        /// <summary>
        /// Disable size limitation on JavaScript project.
        /// </summary>
        public bool? DisableSizeLimit { get; set; }

        /// <summary>
        /// Emit a UTF-8 Byte Order Mark (BOM) in the beginning of output files.
        /// </summary>
        public bool? EmitBOM { get; set; }

        /// <summary>
        /// Emit design-type metadata for decorated declarations in source. See issue #2577 for details.
        /// </summary>
        public bool? EmitDecoratorMetadata { get; set; }

        /// <summary>
        /// Enables experimental support for ES decorators.
        /// </summary>
        public bool? ExperimentalDecorators { get; set; }

        /// <summary>
        /// Disallow inconsistently-cased references to the same file.
        /// </summary>
        public bool? ForceConsistentCasingInFileNames { get; set; }

        /// <summary>
        /// Print help message.
        /// </summary>
        public bool? Help { get; set; }

        /// <summary>
        /// Import emit helpers (e.g. __extends, __rest, etc..) from tslib
        /// </summary>
        public bool? ImportHelpers { get; set; }

        /// <summary>
        /// Emit a single file with source maps instead of having a separate file.
        /// </summary>
        public bool InlineSourceMap { get; set; }

        /// <summary>
        /// Emit the source alongside the sourcemaps within a single file; requires --inlineSourceMap or --sourceMap to be set.
        /// </summary>
        public bool InlineSources { get; set; }


        /// <summary>
        /// Initializes a TypeScript project and creates a tsconfig.json file.
        /// </summary>
        public bool? Init { get; set; }

        /// <summary>
        /// Unconditionally emit imports for unresolved files.
        /// </summary>
        public bool? IsolatedModules { get; set; }


        //   public bool? ExperimentalAsyncFunctions { get; set; }

        /// <summary>
        /// Support JSX in .tsx files: "React" or "Preserve". See JSX.
        /// </summary>
        public bool? Jsx { get; set; }

        /// <summary>
        /// Specify the JSX factory function to use when targeting react JSX emit, e.g. React.createElement or h.
        /// </summary>
        public string JsxFactory { get; set; }

        /// <summary>
        /// List of library files to be included in the compilation.
        /// Possible values are: 
        /// - ES5 
        /// - ES6 
        /// - ES2015 
        /// - ES7 
        /// - ES2016 
        /// - ES2017 
        /// - DOM 
        /// - DOM.Iterable 
        /// - WebWorker 
        /// - ScriptHost 
        /// - ES2015.Core 
        /// - ES2015.Collection 
        /// - ES2015.Generator 
        /// - ES2015.Iterable 
        /// - ES2015.Promise 
        /// - ES2015.Proxy 
        /// - ES2015.Reflect 
        /// - ES2015.Symbol 
        /// - ES2015.Symbol.WellKnown 
        /// - ES2016.Array.Include 
        /// - ES2017.object 
        /// - ES2017.SharedMemory
        /// Note: If --lib is not specified a default library is injected.The default library injected is: 
        /// For --target ES5: DOM,ES5,ScriptHost
        /// For --target ES6: DOM,ES6,DOM.Iterable,ScriptHost
        /// </summary>
        public List<string> Lib { get; set; }

        /// <summary>
        /// Print names of generated files part of the compilation.
        /// </summary>
        public bool? ListEmittedFiles { get; set; }

        /// <summary>
        /// Print names of files part of the compilation.
        /// </summary>
        public bool? ListFiles { get; set; }

        /// <summary>
        /// The locale to use to show error messages, e.g. en-us.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// Specifies the location where debugger should locate map files instead of generated locations. Use this flag if the .map files will be located at run-time in a different location than the .js files. The location specified will be embedded in the sourceMap to direct the debugger where the map files will be located.
        /// </summary>
        public string MapRoot { get; set; }

        /// <summary>
        /// The maximum dependency depth to search under node_modules and load JavaScript files. Only applicable with --allowJs.
        /// </summary>
        public int? MaxNodeModulesJsDepth { get; set; }

        /// <summary>
        /// Specify module code generation: "None", "CommonJS", "AMD", "System", "UMD", "ES6", or "ES2015".
        /// Only "AMD" and "System" can be used in conjunction with --outFile.
        /// "ES6" and "ES2015" values may not be used when targeting "ES5" or lower.
        /// </summary>
        public ModuleKind? Module { get; set; }

        /// <summary>
        /// Determine how modules get resolved. Either "Node" for Node.js/io.js style resolution, or "Classic". See Module Resolution documentation for more details.
        /// </summary>
        public ModuleResolutionKind? ModuleResolution { get; set; }

        /// <summary>
        /// Use the specified end of line sequence to be used when emitting files: "crlf" (windows) or "lf" (unix).”
        /// </summary>
        public ModuleKind? NewLine { get; set; }

        /// <summary>
        /// Do not emit outputs.
        /// </summary>
        public bool? NoEmit { get; set; }

        /// <summary>
        /// Do not generate custom helper functions like __extends in compiled output.
        /// </summary>
        public bool? NoEmitHelpers { get; set; }

        /// <summary>
        /// Do not emit outputs if any errors were reported.
        /// </summary>
        public bool? NoEmitOnError { get; set; }

        /// <summary>
        /// Report errors for fallthrough cases in switch statement.
        /// </summary>
        public bool? NoFallthroughCasesInSwitch { get; set; }

        /// <summary>
        /// Raise error on expressions and declarations with an implied any type.
        /// </summary>
        public bool? NoImplicitAny { get; set; }


        /// <summary>
        /// Report error when not all code paths in function return a value.
        /// </summary>
        public bool? NoImplicitReturns { get; set; }

        /// <summary>
        /// Raise error on this expressions with an implied any type.
        /// </summary>
        public bool? NoImplicitThis { get; set; }

        /// <summary>
        /// Do not emit "use strict" directives in module output.
        /// </summary>
        public bool? NoImplicitUseStrict { get; set; }

        /// <summary>
        /// Do not include the default library file (lib.d.ts).
        /// </summary>
        public bool? NoLib { get; set; }
        // public bool? NoErrorTruncation { get; set; }

        /// <summary>
        /// Do not add triple-slash references or module import targets to the list of compiled files.
        /// </summary>
        public bool? NoResolve { get; set; }

        /// <summary>
        /// Report errors on unused locals.
        /// </summary>
        public bool? NoUnusedLocals { get; set; }

        /// <summary>
        ///Report errors on unused parameters.
        /// </summary>
        public bool? NoUnusedParameters { get; set; }

        /// <summary>
        /// DEPRECATED. Use --outFile instead.
        /// </summary>
        [Obsolete("Use OutFile instead.")]
        public string Out { get; set; }

        /// <summary>
        /// Redirect output structure to the directory.
        /// </summary>
        public string OutDir { get; set; }

        /// <summary>
        /// Concatenate and emit output to single file. The order of concatenation is determined by the list of files passed to the compiler on the command line along with triple-slash references and imports. See output file order documentation for more details.
        /// </summary>
        public string OutFile { get; set; }

        // ONLY ALLOWED IN tsconfig.json
        /// <summary>
        /// List of path mapping entries for module names to locations relative to the baseUrl. See Module Resolution documentation for more details.
        /// </summary>
        public List<string> Paths { get; set; }


        /// <summary>
        /// Do not erase const enum declarations in generated code. See const enums documentation for more details.
        /// </summary>
        public bool? PreserveConstEnums { get; set; }

        /// <summary>
        /// Stylize errors and messages using color and context.
        /// </summary>
        public bool? Pretty { get; set; }

        /// <summary>
        /// Compile a project given a valid configuration file.
        /// The argument can be an file path to a valid JSON configuration file, or a directory path to a directory containing a tsconfig.json file.
        /// See tsconfig.json documentation for more details.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Specifies the object invoked for createElement and __spread when targeting "react" JSX emit.
        /// </summary>
        public string ReactNamespace { get; set; }

        /// <summary>
        /// Remove all comments except copy-right header comments beginning with /*!
        /// </summary>
        public bool? RemoveComments { get; set; }

        /// <summary>
        /// Specifies the root directory of input files. Only use to control the output directory structure with --outDir.
        /// </summary>
        public string RootDir { get; set; }

        // ONLY ALLOWED IN tsconfig.json
        /// <summary>
        /// List of root folders whose combined content represent the structure of the project at runtime. See Module Resolution documentation for more details.
        /// </summary>
        public List<string> RootDirs { get; set; }

        /// <summary>
        /// Skip type checking of default library declaration files.
        /// </summary>
        public bool? SkipDefaultLibCheck { get; set; }

        /// <summary>
        /// Skip type checking of all declaration files (*.d.ts).
        /// </summary>
        public bool? SkipLibCheck { get; set; }

        /// <summary>
        /// Generates corresponding .map file.
        /// </summary>
        public bool? SourceMap { get; set; }

        /// <summary>
        /// Specifies the location where debugger should locate TypeScript files instead of source locations. Use this flag if the sources will be located at run-time in a different location than that at design-time. The location specified will be embedded in the sourceMap to direct the debugger where the source files will be located.
        /// </summary>
        public string SourceRoot { get; set; }

        /// <summary>
        /// In strict null checking mode, the null and undefined values are not in the domain of every type and are only assignable to themselves and any (the one exception being that undefined is also assignable to void).
        /// </summary>
        public bool? StrictNullChecks { get; set; }

        /// <summary>
        /// Do not emit declarations for code that has an /** @internal */ JSDoc annotation.
        /// </summary>
        public bool? StripInternal { get; set; }

        /// <summary>
        /// Suppress excess property checks for object literals.
        /// </summary>
        public bool? SupressExcessPropertyErrors { get; set; }

        /// <summary>
        /// Suppress --noImplicitAny errors for indexing objects lacking index signatures. See issue #1232 for more details.
        /// </summary>
        public bool? SupressImplicitAnyIndexErrors { get; set; }

        /// <summary>
        /// Specify ECMAScript target version: "ES3" (default), "ES5", "ES6"/"ES2015", "ES2016", "ES2017" or "ESNext". 
        /// Note: "ESNext" targets latest supported ES proposed features.
        /// </summary>
        public ScriptTarget Target { get; set; }

        /// <summary>
        /// Report module resolution log messages.
        /// </summary>
        public bool? TraceResolution { get; set; }


        /// <summary>
        /// List of names of type definitions to include. See @types, –typeRoots and –types for more details.
        /// </summary>
        public List<string> Types { get; set; }

        /// <summary>
        /// List of folders to include type definitions from. See @types, –typeRoots and –types for more details.
        /// </summary>
        public List<string> TypeRoots { get; set; }

        /// <summary>
        /// Print the compiler’s version.
        /// </summary>
        public bool? Version { get; set; }

        /// <summary>
        /// Run the compiler in watch mode. Watch input files and trigger recompilation on changes.
        /// </summary>
        public bool? Watch { get; set; }

    }

    public enum ScriptTarget
    {
        Es5,
        Es6
    }
    public enum ModuleKind
    {
        CommonJs,
        Amd
    }

    public enum NewLineKind
    {
        CarriageReturnLineFeed = 0,
        LineFeed = 1,
    }

    public enum ModuleResolutionKind
    {
        Classic = 1,
        NodeJs = 2,
    }
}