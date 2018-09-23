define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var Another = /** @class */ (function () {
        function Another(another) {
            this.another = another;
        }
        Another.prototype.greet = function () {
            return " < h1 > " + this.another + " < /h1>";
        };
        return Another;
    }());
    exports.Another = Another;
    ;
    var another = new Another("seasondawdsa greetings!");
    document.body.querySelector(".message").innerHTML += another.greet();
});
//# sourceMappingURL=Another.js.map