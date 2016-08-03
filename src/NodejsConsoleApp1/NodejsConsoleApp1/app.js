
var netpackRequireJs = require('./netpack-requirejs.js');

var moduleAContents =
    "define(\"ModuleA\", [\"require\", \"exports\"], function (require, exports) {\r\n    \"use strict\";\r\n});";

var moduleBContents =
    "define(\"ModuleB\", [\"require\", \"exports\", \"ModuleA\"], function (require, exports, moduleB) {\r\n    \"use strict\";\r\n});";

var moduleAFile = { "FilePath": "wwwroot/ModuleA.js", "FileContents": moduleAContents };
var moduleBFile = { "FilePath": "wwwroot/ModuleB.js", "FileContents": moduleBContents };

var files = [moduleAFile, moduleBFile];

var modulesArray = [{ name: 'ModuleA' }, { name: 'ModuleB' }];
var out = 'built.js';
var baseUrl = 'wwwroot';
var dir = './built';
var options = { Files: files, BaseUrl: baseUrl, Modules: modulesArray, Out: out, Dir: dir }

netpackRequireJs((state, args) => {
  //  var outputA = args["built/ModuleA.js"];
  //  var outputAA = args["built\ModuleA.js"];
  //  var outputB = args["built\ModuleB.js"];
  //  var outputBB = args["built\ModuleB.js"];
  //  var outputText = args["built\build.txt"];

    for (var property in args) {
        if (args.hasOwnProperty(property)) {
            // do stuff
            var propVal = args[property];
            console.log(propVal);
        }
    }

}, options);









