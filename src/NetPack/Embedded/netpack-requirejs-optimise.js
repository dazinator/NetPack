
var mock = require('mock-fs');
var requirejs = require('requirejs');

module.exports = function (callback, requestDto) {

    // set up virtual file system to resolve files from memory when r.js requests them.
    var dir = {};
    var files = requestDto.Files;

    callback(null, {
        Result: contents
    });

    var arrayLength = files.length;
    for (var i = 0; i < arrayLength; i++) {

        var file = files[i];
        dir[file.FilePath] = file.FileContents;
        //Do something
    }

    mock(dir);

    var config = {
        baseUrl: 'wwwroot',
        name: 'moduleB',
        out: 'main-built.js'
    };

    requirejs.optimize(config, function (buildResponse) {
        //buildResponse is just a text output of the modules
        //included. Load the built file for the contents.
        //Use config.out to get the optimized file contents.

        // unmock filesystem.
        mock.restore();

        // Invoke some external transpiler (e.g., an NPM module) then:
        var contents = fs.readFileSync(config.out, 'utf8');

        callback(null, {
            Result: contents
        });

    }, function (err) {
        //optimization err callback
        var errorMessage = err.toString();
        callback(null,
        {
            Error: errorMessage
        });
    });

};



