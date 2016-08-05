/// <reference path="compilerhost.ts" />
"use strict";
var ts = require("typescript");
var host = require("./compilerhost");
var StringSource = host.StringSource;
var SourceType = host.SourceType;
var CompositeCompilerHost = host.CompositeCompilerHost;
var NetpackTypescriptCli = (function () {
    function NetpackTypescriptCli() {
    }
    NetpackTypescriptCli.prototype.compileStrings = function (input, tscArgs, options, onError) {
        var host = new CompositeCompilerHost(options)
            .readFromStrings()
            .writeToString();
        var sources = [];
        if (Array.isArray(input) && input.length) {
            // string[]
            if (typeof input[0] == 'string') {
                sources.push(new StringSource(input[0])); // ts.map<string, StringSource>(input, );
            }
            else if (input[0] instanceof StringSource) {
                sources.concat(input);
            }
            else
                throw new Error('Invalid value for input argument');
        }
        else if (typeof input == 'object') {
            for (var k in input)
                if (input.hasOwnProperty(k))
                    sources.push(new StringSource(input[k], k));
        }
        else
            throw new Error('Invalid value for input argument');
        return this._compile(host, sources, tscArgs, options, onError);
    };
    NetpackTypescriptCli.prototype._compile = function (host, sources, tscArgs, options, onError) {
        if (typeof tscArgs == "string")
            tscArgs = tscArgs.split(' ');
        else
            tscArgs = tscArgs || [];
        var commandLine = ts.parseCommandLine(tscArgs);
        var files;
        if (host.readsFrom == SourceType.String) {
            sources.forEach(function (s) { return host.addSource(s.fileName, s.contents); });
            files = host.getSourcesFilenames();
        }
        else {
            files = sources.map(function (s) { return s.fileName; }).concat(commandLine.fileNames);
        }
        var program = ts.createProgram(files, commandLine.options, host);
        var emitResult = program.emit();
        var allDiagnostics = ts.getPreEmitDiagnostics(program).concat(emitResult.diagnostics);
        var errors = [];
        allDiagnostics.forEach(function (diagnostic) {
            var _a = diagnostic.file.getLineAndCharacterOfPosition(diagnostic.start), line = _a.line, character = _a.character;
            var message = ts.flattenDiagnosticMessageText(diagnostic.messageText, '\n');
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
                errors.forEach(function (e) {
                    e.formattedMessage = formatError(e);
                    onError(e);
                });
            }
        }
        function formatError(diagnostic) {
            var output = "";
            if (diagnostic.file) {
                var loc = diagnostic.file.getLineAndCharacterOfPosition(diagnostic.start);
                output += diagnostic.file.fileName + "(" + loc.line + "," + loc.character + "): ";
            }
            var category = ts.DiagnosticCategory[diagnostic.category].toLowerCase();
            output += category + " TS" + diagnostic.code + ": " + diagnostic.messageText + ts.sys.newLine;
            return output;
        }
    };
    return NetpackTypescriptCli;
}());
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = NetpackTypescriptCli;
//# sourceMappingURL=NetpackTypescriptCli.js.map