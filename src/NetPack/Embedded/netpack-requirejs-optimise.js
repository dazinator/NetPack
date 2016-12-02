
//var mock = require('mock-fs');
//var requirejs = require('requirejs');
var netpackrequirejs = require("netpack-requirejs");

module.exports = function (callback, request) {

    var options = request.options;
    options.files = request.files;
    // set up virtual file system to resolve files from memory when r.js requests them.
    //var requireJsOptions = new netpackrequirejs.RequireJsOptions();
    //requireJsOptions.files = options.files;
    //requireJsOptions.modules = options.modules;


   // var files = options.Files;
   // callback(null, options);

    //for (var i = 0; i < files.length; i++) {
    //    var file = files[i];
    //    requireJsOptions.push({ "path": file.FilePath, "contents": file.FileContents })
    //}

    //var modules = options.Modules;
    //if (modules && modules.length > 0) {
    //    for (var i = 0; i < modules.length; i++) {
    //        var module = modules[i];
    //        var amdModule = new netpackrequirejs.Module();
    //        amdModule.name = module.Name;
    //        amdModule.include = module.Include;
    //        amdModule.exclude = module.Exclude;
    //        amdModule.excludeShallow = module.ExcludeShallow;
    //        amdModule.create = module.Create;
    //        amdModule.insertRequire = module.InsertRequire;
    //        // var exclude = amdModule.Exclude;
    //        // var include = amdModule.Include;
    //        requireJsOptions.modules.push(amdModule);
    //    }
    //}

 //   requireJsOptions.mainConfigFile = options.mainConfigFile;
  //  requireJsOptions.baseUrl = options.baseUrl;
  //  requireJsOptions.dir = options.dir;

    var optimiser = new netpackrequirejs.NetPackRequireJs();
    optimiser.optimise(options, function (results) {
        for (var property in results) {
            if (results.hasOwnProperty(property)) {
                // do stuff
               // console.log("Property Name: " + property);
               // console.log("=====================================");
              //  var propVal = results[property];
               // console.log(propVal);
            }
        }
        callback(null, results);
    }, function (message) {
       // console.log("Error: " + message);
        callback(message, null);
        //done(message);
    });

};






