/// <reference path="compilerhost.ts" />

import * as ts from "typescript";
import * as host from "./compilerhost";
import StringSource = host.StringSource;
import Source = host.ISource;
import CompilationResult = host.ICompilationResult;
import SourceType = host.SourceType;
import CompositeCompilerHost = host.CompositeCompilerHost;

export default class NetpackTypescriptCli {
    
    compileStrings(input, tscArgs?, options?: ts.CompilerOptions, onError?: (message) => void): host.ICompilationResult {

    var host = new CompositeCompilerHost(options)
        .readFromStrings()
        .writeToString();

    var sources = [];

    if (Array.isArray(input) && input.length) {
        // string[]
        if (typeof input[0] == 'string') {
            sources.push(new StringSource(input[0])); // ts.map<string, StringSource>(input, );
        }
        // Source[]
        else if (input[0] instanceof StringSource) {
            sources.concat(input);
        } else
            throw new Error('Invalid value for input argument');
    }
    // dictionary
    else if (typeof input == 'object') {
        for (var k in input) if (input.hasOwnProperty(k))
            sources.push(new StringSource(input[k], k));
    }
    else
        throw new Error('Invalid value for input argument')

    return this._compile(host, sources, tscArgs, options, onError);
}
    
    _compile(host: host.CompositeCompilerHost, sources: Source[], tscArgs: string, options?: ts.CompilerOptions, onError?: (message) => void);
    _compile(host: host.CompositeCompilerHost, sources: Source[], tscArgs: string[], options?: ts.CompilerOptions, onError?: (message) => void);
    _compile(host: host.CompositeCompilerHost, sources: Source[], tscArgs?, options?: ts.CompilerOptions, onError?: (message) => void): CompilationResult {

    if (typeof tscArgs == "string")
        tscArgs = tscArgs.split(' ');
    else
        tscArgs = tscArgs || [];

    var commandLine = ts.parseCommandLine(tscArgs);
    var files;

    if (host.readsFrom == SourceType.String) {
        sources.forEach(s => host.addSource(s.fileName, s.contents));
        files = host.getSourcesFilenames();
    }
    else {
        files = sources.map(s => s.fileName).concat(commandLine.fileNames);
    }

    var program = ts.createProgram(files, commandLine.options, host);

    let emitResult = program.emit();
    let allDiagnostics = ts.getPreEmitDiagnostics(program).concat(emitResult.diagnostics);

    let errors = [];
    allDiagnostics.forEach(diagnostic => {
        let { line, character } = diagnostic.file.getLineAndCharacterOfPosition(diagnostic.start);
        let message = ts.flattenDiagnosticMessageText(diagnostic.messageText, '\n');
        errors.push({
            "File": diagnostic.file.fileName,
            "Line": line + 1,
            "Char": character + 1,
            "Message": message
        });

        // console.log(`${diagnostic.file.fileName} (${line + 1},${character + 1}): ${message}`);
    });

    if (errors.length > 0) {
        forwardErrors(errors, onError);
    }

    return {
        sources: host.outputs,
        errors: errors
    };

    function forwardErrors(errors, onError) {
        if (typeof onError == 'function') {
            errors.forEach(e => {
                e.formattedMessage = formatError(e);
                onError(e);
            });
        }
    }

    function formatError(diagnostic: ts.Diagnostic) {
        var output = "";
        if (diagnostic.file) {
            var loc = diagnostic.file.getLineAndCharacterOfPosition(diagnostic.start);
            output += diagnostic.file.fileName + "(" + loc.line + "," + loc.character + "): ";
        }
        var category = ts.DiagnosticCategory[diagnostic.category].toLowerCase();
        output += category + " TS" + diagnostic.code + ": " + diagnostic.messageText + ts.sys.newLine;
        return output;
    }
}



}

