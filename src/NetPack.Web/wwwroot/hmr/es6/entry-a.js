import usedByA from './used-by-a';
import usedByBoth from './used-by-both';
import hot from '@hot';

import('./dynamically-imported/apply-color-and-message').then(({ default: apply }) => {
    apply('#a [data-used-by="a"]', usedByA);
    apply('#a [data-used-by="both"]', usedByBoth);
}); 


var self = {};
self._state = hot ? hot._state : {};
self.addModuleToList = function (listId, moduleName) {

    var ul = document.getElementById(listId);
    var li = document.createElement("li");
    li.appendChild(document.createTextNode(moduleName + " Please make a change to /hmr/es6/entry-a.js and watch what happens.."));
    ul.appendChild(li);
    self._state.oldChild = li;
};


self._state.oldChild = null;

self.addModuleToList("modules", "ModuleA..");
var unload = () => {
    var ul = document.getElementById("modules");
    var child = self._state.oldChild;
    ul.removeChild(child);
    console.log('Unloaded..');
    // force unload React components
};

self.__unload = unload;
export default self;