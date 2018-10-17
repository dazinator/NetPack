"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const rolluphost_1 = require("netpack-rollup");
const RollupFileOptions_1 = require("netpack-rollup/dist/RollupFileOptions");
const RollupOutputOptions_1 = require("netpack-rollup/dist/RollupOutputOptions");
const hypothetical = require("rollup-plugin-hypothetical");
const fs = require("fs");

var allFiles = {};

module.exports = {

    build: function (callback, requestDto) {

        var files = requestDto.files;
        for (var i = 0; i < requestDto.files.length; i++) {

            var file = requestDto.files[i];
            allFiles[file.path] = file.contents;
        }

        var inputOptionsDto = requestDto.inputOptions;
        var outputOptionsDto = requestDto.outputOptions;

        var inputOptions = new RollupFileOptions_1.default();
        inputOptions.input = inputOptionsDto.input;
        // inputOptions.entry = inputOptionsDto.input;

        var hypotheticalPlugin = hypothetical({
            files: allFiles
        });
        hypotheticalPlugin.cwd = false;
        inputOptions.plugins = [hypotheticalPlugin];

        var outputOptions = new RollupOutputOptions_1.default();
        outputOptions.format = outputOptionsDto.format;

        let sut = new rolluphost_1.default();

        //var f = [];
        //for (var p in allFiles) {
        //    if (allFiles.hasOwnProperty(p)) {
        //        f.push(p);
        //    }
        //}

        // var rollupResult = { Code: null, SourceMap: null, Echo: inputOptions, Files: f };
        //  callback(null, rollupResult);

        sut.build(inputOptions, outputOptions).then(result => {
            if (result === undefined) {
               // callback("result was undefined", null);
            }
            else {
                var code = result.Code;
                var sourceMap = result.SourceMap;
                // response.files.push({ path: property, contents: results[property] })
                var rollupResult = { Code: code, SourceMap: sourceMap };
                callback(null, rollupResult);
            }
           

        }).catch((reason) => {
            callback(reason, null);
        });
    }
};