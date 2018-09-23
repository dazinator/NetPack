define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var Greeter = /** @class */ (function () {
        function Greeter(greeting) {
            this.greeting = greeting;
        }
        Greeter.prototype.greet = function () {
            return this.greeting;
        };
        return Greeter;
    }());
    ;
    var greeter = new Greeter("Hi akkddddfdkk!!!!");
    document.body.querySelector(".message").innerHTML += greeter.greet();
});
//# sourceMappingURL=Greeter.js.map