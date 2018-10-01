"use strict";
var compilerhost = require("netpack-typescript-compiler");

var allFiles = {};

module.exports = {
   
    build: function (callback, requestDto) {

        var typescriptFiles = requestDto.files;
        for (var property in typescriptFiles) {
            if (typescriptFiles.hasOwnProperty(property)) {
                allFiles[property] = typescriptFiles[property];
            }
        }      

        var inputs = requestDto.inputs;
        var inputFiles = {};

        for (var i = 0; i < inputs.length; i++) {
            var input = inputs[i];
            var inputFile = allFiles[input];
            inputFiles[input] = inputFile;
        }

        var options = requestDto.options;
        for (var optionsProp in options) {
            if (options.hasOwnProperty(optionsProp)) {
                if (options[optionsProp] === null) {
                    delete options[optionsProp];
                }
            }
        }

        requestDto.options = options;

        var compileErrors = [];
        var errorHandler = function (err) {
            compileErrors.push(err);
        };
        var sut = new compilerhost["default"]();

        // Act
        var result = sut.compileStrings(inputFiles, options, errorHandler);
        result.Echo = allFiles;

        callback(null, result);

    }
};