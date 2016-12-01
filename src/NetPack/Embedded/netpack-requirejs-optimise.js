
//var mock = require('mock-fs');
//var requirejs = require('requirejs');
var requirejs = require("netpack-requirejs");

module.exports = function (callback, options) {

    // set up virtual file system to resolve files from memory when r.js requests them.
    var requireJsOptions = new netpackrequirejs.RequireJsOptions();
    requireJsOptions.files = new Array();
    var files = options.Files;

    files.forEach(function (file) {
        requireJsOptions.push({ "path": file.FilePath, "contents": file.FileContents })
    });

    var modules = options.Modules;
    modules.forEach(function (module) {

        var amdModule = new netpackrequirejs.Module();
        var exclude = amdModule.Exclude;
        var include = amdModule.Include;
        requireJsOptions.modules.push({ "name": amdModule.Name, "exclude": exclude, "include": include })
    });

    requireJsOptions.mainConfigFile = options.MainConfigFile;
    requireJsOptions.baseUrl = options.BaseUrl;
    requireJsOptions.dir = options.Dir;

    var optimiser = new netpackrequirejs.NetPackRequireJs();
    optimiser.optimise(requireJsOptions, function (results) {
        for (var property in results) {
            if (results.hasOwnProperty(property)) {
                // do stuff
                console.log("Property Name: " + property);
                console.log("=====================================");
                var propVal = results[property];
                console.log(propVal);
            }
        }
        callback(null, results);
    }, function (message) {
        console.log("Error: " + message);
        callback(null, message);
        //done(message);
    });

};






