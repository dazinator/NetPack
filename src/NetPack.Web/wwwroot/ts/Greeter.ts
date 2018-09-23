import {Another} from "./Another";

class Greeter {
    constructor(public greeting: string) { }
    greet() {
        return this.greeting;
    }
};

var greeter = new Greeter("Hi akkddddfdkk!!!!");
document.body.querySelector(".message").innerHTML += greeter.greet();  