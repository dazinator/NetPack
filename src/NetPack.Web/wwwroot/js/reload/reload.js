"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/reloadhub").build();

connection.on("Reload", function (message) {

    location.reload(true);

    //var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    //var encodedMsg = user + " says " + msg;
    //var li = document.createElement("li");
    //li.textContent = encodedMsg;
    //document.getElementById("messagesList").appendChild(li);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    //var user = document.getElementById("userInput").value;
    var message = "foo";
    connection.invoke("TriggerReload", message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
