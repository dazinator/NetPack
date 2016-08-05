"use strict";
///<reference path="ClassA.ts" />
var ClassA_1 = require("./ClassA");
var ClassB = (function () {
    function ClassB(another) {
        this.another = another;
    }
    ClassB.prototype.doSomething = function () {
        // return ""<h1>"" + this.greeting + ""</h1>"";
    };
    return ClassB;
}());
exports.ClassB = ClassB;
;
var classB = new ClassB("Hello, world!");
classB.doSomething();
var classA = new ClassA_1.ClassA("Hello, world!");
classA.doSomething();
//# sourceMappingURL=ClassB.js.map