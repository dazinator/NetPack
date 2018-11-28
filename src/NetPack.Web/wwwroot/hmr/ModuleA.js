define(["module", "exports", "@hot"], function (module, exports, hot) {
    "use strict";

    var self = {};
    self._state = hot ? hot._state : {};
    self.addModuleToList = function (listId, moduleName) {

        var ul = document.getElementById(listId);
        var li = document.createElement("li");
        li.appendChild(document.createTextNode(moduleName + " Loaded.. I am productive !"));
        ul.appendChild(li);      
        self._state.oldChild = li;
    };

   
    self._state.oldChild = null;

    self.addModuleToList("modules", "ModuleA");
    var unload = () => {
        var ul = document.getElementById("modules");
        var child = self._state.oldChild;
        ul.removeChild(child);
        console.log('Unloaded..');
        // force unload React components
    };

    self.__unload = unload;
   // module.exports.__unload = unload;
   // return module.exports;


    //exports.__unload = () => {
    //    console.log('Unload something (unsubscribe from listeners, disconnect from socket, etc...)')
    //    // force unload React components
    //};

    return self;

});