define(["require", "exports", "../ts/Another"], function (require, exports, Another_1) {
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
    var y = new Another_1.Another("foo bare");
    var greeter = new Greeter("Hi daz!!!!");
});
//document.body.querySelector(".message").innerHTML += greeter.greet();  
//# sourceMappingURL=Greeter.js.map