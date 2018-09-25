import {Another} from "../ts/Another";

class Greeter {
    constructor(public greeting: string) { }
    greet() {
        return this.greeting;
    }
};

var y = new Another("foo bare");

var greeter = new Greeter("Hi daz!!!!");
//document.body.querySelector(".message").innerHTML += greeter.greet();  