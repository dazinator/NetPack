var tsc = require('typescript-compiler');
var tscli = require('./scripts/NetpackTypescriptCli.js');

module.exports = function (callback, options) {

    var self = this;

    self.getTsArgs = function (opts) {
        var args = '--module ' + opts.moduleFormat + ' -t ' + opts.target + ' --outFile ' + opts.out;
        if (opts.sourceMap) {
            args = args + ' --sourceMap';
            args = args + ' --inlineSourceMap';
        }
        args = args + ' --outFile ' + opts.out;
      
        return args;
    }

    
    var compileErrors = [];
    var errorHandler = function (err) {
        compileErrors.push(err);
    }

    var args = self.getTsArgs(options);
    var cli = new tscli.Default();
    var result = cli.compileStrings(options.files, args, null, errorHandler);

    // Invoke some external transpiler (e.g., an NPM module) then:
    callback(null, {
        Result: result,
        Errors: compileErrors
    });
};