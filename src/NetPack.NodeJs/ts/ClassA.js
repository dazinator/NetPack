"use strict";
var ClassA = (function () {
    function ClassA(another) {
        this.another = another;
    }
    ClassA.prototype.doSomething = function () {
        // return ""<h1>"" + this.greeting + ""</h1>"";
    };
    return ClassA;
}());
exports.ClassA = ClassA;
var classA = new ClassA("Hello, world!");
classA.doSomething();
//# sourceMappingURL=ClassA.js.map