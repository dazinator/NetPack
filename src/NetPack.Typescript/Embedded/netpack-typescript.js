"use strict";
var compilerhost = require("netpack-typescript-compiler");

var allFiles = {
    

}

module.exports = {

    build: function (callback, requestDto) {

        var typescriptFiles = requestDto.files;
        for (var property in typescriptFiles) {
            if (typescriptFiles.hasOwnProperty(property)) {
                allFiles[property] = typescriptFiles[property];
            }
        }
       
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
        
        var compileErrors = [];
        var errorHandler = function (err) {
            compileErrors.push(err);
        };
        var sut = new compilerhost["default"]();

        // Act
        var result = sut.compileStrings(typescriptFiles, args, null, errorHandler);

        callback(null, result);

    },

    //updateFiles: function (callback, requestDto) {
    //    var typescriptFiles = requestDto.files;
    //    for (var property in typescriptFiles) {
    //        if (typescriptFiles.hasOwnProperty(property)) {
    //            allFiles[property] = typescriptFiles[property];
    //        }
    //    }
    //}

};