define("ModuleA", ["require", "exports"], function (require, exports) {
    "use strict";

    function bar() {
        alert("hi");
    };

    console.log("hi from module a");
});