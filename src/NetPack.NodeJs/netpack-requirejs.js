
//var mock = require('mock-fs');
//var requirejs = require('requirejs');
var requirejs = require("requirejs-memfiles");

module.exports = function (callback, options) {

    // set up virtual file system to resolve files from memory when r.js requests them.
    var inMemoryFiles = {};
    var files = options.Files;

    var arrayLength = files.length;
    for (var i = 0; i < arrayLength; i++) {

        var file = files[i];
        inMemoryFiles[file.FilePath] = file.FileContents;
        //Do something
    }

    //mock(dir);

    var baseUrl = options.BaseUrl;
    var appDir = options.AppDir;
    var modules = options.Modules;
    var out = options.Out;
    var dir = options.Dir;
    var mainConfig = options.MainConfig;
    //   name: 'moduleB',
    var config = {
        baseUrl: baseUrl,
        modules: modules,
        mainConfigFile: mainConfig,
       // out: out,
        dir: dir
    };


    var allDone = function(done) {
        done();
        callback();
    }

    requirejs.setFiles(inMemoryFiles, function (done) {
        requirejs.optimize({
            optimize: 'uglify2',
            preserveLicenseComments: false,
            generateSourceMaps: true,
            appDir: appDir,
            baseUrl: baseUrl,
            dir: dir,
            modules: modules
        }, function () {
           // var output = inMemoryFiles["dist/output.js"];
            callback(null, inMemoryFiles);
            done();

    }, function (error) {
    // handle error
        callback(null, error);
        done();

});
});

//    requirejs.optimize(config, function (buildResponse) {
//        //buildResponse is just a text output of the modules
//        //included. Load the built file for the contents.
//        //Use config.out to get the optimized file contents.

//        // unmock filesystem.
//        mock.restore();

//        // Invoke some external transpiler (e.g., an NPM module) then:
//        var contents = fs.readFileSync(config.out, 'utf8');

//        callback(null, {
//            Result: contents
//        });

//    }, function (err) {
//        //optimization err callback
//        var errorMessage = err.toString();
//        callback(null,
//            {
//                Error: errorMessage
//            });
//    });

};






