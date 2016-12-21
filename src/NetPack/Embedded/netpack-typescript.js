"use strict";
var compilerhost = require("netpack-typescript-compiler");

module.exports = function (callback, requestDto) {

    var typescriptFiles = requestDto.files;
    var options = requestDto.options;

    var args = '--module ' + options.module + ' -t ' + options.target;
    if (options.sourceMap) {
        args = args + ' --sourceMap';
    } else {
        if (options.inlineSourceMap) {
            args = args + ' --inlineSourceMap';
        }
    }
    if (options.outFile) {
        args = args + ' --outFile' + options.outFile;
    }
    if (options.baseUrl) {
        args = args + ' --baseUrl' + options.baseUrl;
    }
    if (options.noImplicitAny) {
        args = args + ' --noImplicitAny';
    }
    if (options.removeComments) {
        args = args + ' --removeComments';
    }
    if (options.inlineSources) {
        args = args + ' --inlineSources';
    }
   

    //  '--traceResolution --baseUrl testFiles ';
    //var webRoot = "testFiles";
    //var filePathA = webRoot + "/ModuleA/ClassA.ts";
    //var filePathB = webRoot + "/ModuleB/ClassB.ts";

    //var files = {};
    //typescriptFiles.forEach(function (f) {
    //    files[filePathA] = classAFileContents;
    //    e.formattedMessage = formatError(e);
    //    onError(e);
    //});
   
   // files[filePathA] = classAFileContents;
   // files[filePathB] = classBFileContents;
    var compileErrors = [];
    var errorHandler = function (err) {
        compileErrors.push(err);
    };
    var sut = new compilerhost["default"]();

    // Act
    var result = sut.compileStrings(typescriptFiles, args, null, errorHandler);

    callback(null, result);
    

    // Invoke some external transpiler (e.g., an NPM module) then:
   
};