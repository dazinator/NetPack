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
        var result = sut.compileStrings(typescriptFiles, options, errorHandler);
        result.Echo = requestDto;

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