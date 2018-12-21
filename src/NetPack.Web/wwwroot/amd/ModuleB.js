define("ModuleB", ["require", "exports", "./ModuleA", "@hot"], function (require, exports, moduleA, hot) {
    "use strict";
    console.log("ModuleB running");

    var self = {};
    
    self.state = moduleA.addModuleToList("modules", "ModuleB");
    self.addModuleToList = moduleA.addModuleToList;
    self.moduleA = moduleA;

    // todo, think about a way to subscribe to dependency reloads, so for example,
    // when moduleA is reloading, we can update our reference of it to the new version.
    self.__unload = function () {
        console.log("module b unloading..");
        self.moduleA.removeModuleFromList(self.state);
    };   

    return self; 
});