define(["require", "exports", "../ts/Another"], function (require, exports, Another_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    class Greeter {
        constructor(greeting) {
            this.greeting = greeting;
        }
        greet() {
            return this.greeting;
        }
    }
    ;
    var y = new Another_1.Another("foo bare");
    var greeter = new Greeter("Hi dazd!!!!");
    document.body.querySelector(".message").innerHTML += greeter.greet();
});
//# sourceMappingURL=Greeter.js.map