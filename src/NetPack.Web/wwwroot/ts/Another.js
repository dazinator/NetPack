define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    class Another {
        constructor(another) {
            this.another = another;
        }
        greet() {
            return " < h1 > " + this.another + " < /h1>";
        }
    }
    exports.Another = Another;
    ;
    var another = new Another("awd de");
    document.body.querySelector(".message").innerHTML += another.greet();
});
//# sourceMappingURL=Another.js.map