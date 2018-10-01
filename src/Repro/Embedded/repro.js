"use strict";

var state = {};

module.exports = {
   
    build: function (callback, requestDto) {

        var items = requestDto.items;
        for (var property in items) {
            if (items.hasOwnProperty(property)) {
                state[property] = items[property];
            }
        }         
        
        requestDto["echoState"] = state;  
        callback(null, requestDto);

    }
};