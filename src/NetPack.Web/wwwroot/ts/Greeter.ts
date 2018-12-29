import { Another } from "../ts/Another";

export class Greeter {
    constructor() { }
    greet() {
        return "Hi!!!!"
    }
};

var y = new Another();

var greeter = new Greeter();
document.body.querySelector(".message").innerHTML += greeter.greet();  