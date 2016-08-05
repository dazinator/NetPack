define("ClassA", ["require", "exports"], function (require, exports) {
    "use strict";
    var ClassA = (function () {
        function ClassA(another) {
            this.another = another;
        }
        ClassA.prototype.doSomething = function () {
        };
        return ClassA;
    }());
    exports.ClassA = ClassA;
    var classA = new ClassA("Hello, world!");
    classA.doSomething();
});
define("ClassB", ["require", "exports", "ClassA"], function (require, exports, A) {
    "use strict";
    var ClassA = A.ClassA;
    var ClassB = (function () {
        function ClassB(another) {
            this.another = another;
        }
        ClassB.prototype.doSomething = function () {
        };
        return ClassB;
    }());
    exports.ClassB = ClassB;
    ;
    var classB = new ClassB("Hello, world!");
    classB.doSomething();
    var classA = new ClassA("Hello, world!");
    classA.doSomething();
});
//# sourceMappingURL=tscompiled.js.map