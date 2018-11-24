"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/reloadhub").build();
connection.on("Reload", function () {
    location.reload(true);
});
connection.start().catch(function (err) {
    return console.error(err.toString());
});