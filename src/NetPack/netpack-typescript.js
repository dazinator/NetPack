var ts = require('typescript');
var TypeScriptSimple = require('typescript-simple').TypeScriptSimple;

module.exports = function (callback, requestDto) {

    var options = requestDto.options;
    var contents = requestDto.typescriptCode;
    var filePath = requestDto.filePath;

    var tss = new TypeScriptSimple({ target: options.target, noImplicitAny: options.noImplicitAny, sourceMap: options.sourceMap });
    var js = "";
    if (options.sourceMap) {
        js = tss.compile(contents, filePath);
    } else {
        js = tss.compile(contents);
    }
  
    // Invoke some external transpiler (e.g., an NPM module) then:
    callback(null, {
        code: js
    });
};