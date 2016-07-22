var ts = require('typescript');
var TypeScriptSimple = require('typescript-simple').TypeScriptSimple;

module.exports = function (callback, contents) {
    var tss = new TypeScriptSimple({ target: ts.ScriptTarget.ES6, noImplicitAny: true });
    var js1 = tss.compile(contents);
    // Invoke some external transpiler (e.g., an NPM module) then:
    callback(null, {
        code: js1
    });
};