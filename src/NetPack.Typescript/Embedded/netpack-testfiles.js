"use strict";

var allFiles = {};

module.exports = {
   
    build: function (callback, requestDto) {

        var typescriptFiles = requestDto.files;
        for (var property in typescriptFiles) {
            if (typescriptFiles.hasOwnProperty(property)) {
                allFiles[property] = typescriptFiles[property];
            }
        }         
        
        requestDto["echoFiles"] = allFiles;  
        callback(null, requestDto);

    }
};