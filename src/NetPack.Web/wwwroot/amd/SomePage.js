define("SomePage", ["require", "exports", "ModuleB"], function (require, exports, moduleB) {
    "use strict";


    function bar() {
        alert("hi");
    };

    console.log("hi this is a page level module.");

    bar();
    moduleB.foo();
    
});