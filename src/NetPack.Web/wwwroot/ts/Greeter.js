define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var Greeter = /** @class */ (function () {
        function Greeter(greeting) {
            this.greeting = greeting;
        }
        Greeter.prototype.greet = function () {
            return " < h1 > " + this.greeting + " < /h1>";
        };
        return Greeter;
    }());
    ;
    var greeter = new Greeter("Hi there!!!!");
    document.body.querySelector(".message").innerHTML += greeter.greet();
});
//# sourceMappingURL=Greeter.js.map