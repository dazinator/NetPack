define("ModuleB", ["require", "exports", "./ModuleA"], function (require, exports, moduleA) {
    "use strict";

    var self = {};
    console.log("hi from module b");
    moduleA.addModuleToList("modules", "ModuleB");
    self.addModuleToList = moduleA.addModuleToList;
    return self; 
});