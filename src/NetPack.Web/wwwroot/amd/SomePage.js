require(["ModuleB"], function (moduleB) {

    console.log("Some Page running..");

    var self = {};
    //self._state = hot ? hot._state ? hot._state : {} : {};
    //self._state.oldChild = null;
    //self._state.listId = null;   
    
    //moduleB.addModuleToList("modules", "SomePage...");

    //self.__unload = function () {
    //    console.log("Some Page Unloading..");
    //    moduleB.__unload();
    //};   
    return self;
});