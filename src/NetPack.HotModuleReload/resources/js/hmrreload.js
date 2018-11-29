"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/hmrhub").build();
connection.on("FilesChanged", function (filePaths) {

    filePaths.forEach(function (filePath) {
        console.log(filePath + " changed!");
    });    
});
connection.start().catch(function (err) {
    return console.error(err.toString());
});