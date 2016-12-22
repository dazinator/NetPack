define("ModuleB", ["require", "exports", "ModuleA"], function (require, exports) {
    "use strict";

    function foo() {
        alert("hi");
    };

    console.log("hi from module b");
});