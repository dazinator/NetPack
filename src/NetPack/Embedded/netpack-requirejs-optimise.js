
var mock = require('mock-fs');
var requirejs = require('requirejs');

module.exports = function (callback, requestDto) {

    var options = requestDto.options;
   
    // set up virtual file system to resolve files from memory when r.js requests them.
    mock({
        'wwwroot': {
            'moduleA.js': 'file content here',
            'moduleB.js': 'more file content here'
        },
    });
    
    var config = {
        baseUrl: 'wwwroot',
        name: 'moduleB',
        out: '../build/main-built.js'
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
            code: contents
        });
       
    }, function (err) {
        //optimization err callback
    });
   
};



