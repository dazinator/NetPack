class Greeter {
    constructor(public greeting: string) { }
    greet() {
        return " < h1 > " + this.greeting + " < /h1>";
    }
};

var greeter = new Greeter("Hi there!!!!");
document.body.querySelector(".message").innerHTML = greeter.greet(); 