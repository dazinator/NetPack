
import NetpackTypescriptCli from "./NetpackTypescriptCli.ts";
import * as fs from "fs";
import TypescriptCli = require("./NetpackTypescriptCli");

//import TypescriptCli = require("./NetpackTypescriptCli");

var classAFileContents = fs.readFileSync('ts/ClassA.ts', "utf-8");
var classBFileContents = fs.readFileSync('ts/ClassB.ts', "utf-8");
var args = '--module Amd -t es6 --outFile test.js --inlineSourceMap';
var files = {
    "ClassA.ts": classAFileContents,
    "ClassB.ts": classBFileContents
};
var compileErrors = [];
var errorHandler = function (err) {
    compileErrors.push(err);
};
var cli = new TypescriptCli.NetpackTypescriptCli();
var result = cli.compileStrings(files, args, null, errorHandler);
throw new TypeError("Error message");










