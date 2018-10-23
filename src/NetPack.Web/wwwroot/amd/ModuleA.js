define("ModuleA", ["require", "exports"], function (require, exports) {
    "use strict";

    var self = {};
    self.addModuleToList = function (listId, moduleName) {

        var ul = document.getElementById(listId);
        var li = document.createElement("li");
        li.appendChild(document.createTextNode(moduleName + " Loaded.. "));
        ul.appendChild(li);      
    };

    self.addModuleToList("modules", "ModuleA");
    return self;

});