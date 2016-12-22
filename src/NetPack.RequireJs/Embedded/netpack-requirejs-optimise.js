
//var mock = require('mock-fs');
//var requirejs = require('requirejs');
var netpackrequirejs = require("netpack-requirejs");

module.exports = function (callback, request) {

    var options = request.options;
    options.files = request.files;

    if (options.modules !== undefined && options.modules !== null && options.modules.length === 0)
    {
        options.modules = null;
    }

    function hasFile(path) {
        for (var i = 0; i < options.files.length; i++) {
            if (options.files[i].path == path) return true;
        }
    }

    var optimiser = new netpackrequirejs.NetPackRequireJs();
    optimiser.optimise(options, function (results) {
        var response = { files: [] };
        for (var property in results) {
            if (results.hasOwnProperty(property)) {

                if (!hasFile(property)) {
                    response.files.push({ path: property, contents: results[property] })
                }
            }
        }
        callback(null, response);
    }, function (message) {
        // console.log("Error: " + message);
        callback(message, null);
        //done(message);
    });

};






