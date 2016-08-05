var Another = (function () {
    function Another(another) {
        this.another = another;
    }
    Another.prototype.doSomething = function () {
        // return ""<h1>"" + this.greeting + ""</h1>"";
    };
    return Another;
}());
;
var another = new Another("Hello!");
another.doSomething();
