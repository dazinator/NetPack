define("ModuleA", ["require", "exports", "@hot"], function (require, exports, hot) {
    "use strict"; 
          
    var self = {};
    self._state = hot ? hot._state ? hot._state : {} : {};
    self._state.item = null;
    
    self.addModuleToList = function (listId, moduleName) {
        var ul = document.getElementById(listId);
        var li = document.createElement("li");
        li.appendChild(document.createTextNode(moduleName + " Loaded.. I am productive today.."));
        ul.appendChild(li);            
        return { listId: listId, item: li };
    }; 

    self.removeModuleFromList = function (data) {
        var ul = document.getElementById(data.listId);
        ul.removeChild(data.item);

        //var lis = ul.getElementsByTagName("li");
        //for (i = 0; i < lis.length; i++) {
        //    if (lis[i].textContent === moduleName) {
        //        ul.removeChild(lis[i]);
        //    };
        //}      
    }; 

    self.__unload = function () {     
        self.removeModuleFromList(self._state.item);       
        console.log("module a unloaded.");       
    };   
          
    self._state.item = self.addModuleToList("modules", "ModuleA");
    return self;

});