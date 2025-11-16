export class Another {
    constructor() { }
    greet() {
        return "Loaded /ts/Another.ts yeah";
    }
};

var another = new Another();
document.body.querySelector(".message").innerHTML += another.greet(); 