﻿export class Another {
    constructor(public another: string) { }
    greet() {
        return " < h1 > " + this.another + " < /h1>";
    }
};

var another = new Another("xmasses greetings!");
document.body.querySelector(".message").innerHTML += another.greet();  

