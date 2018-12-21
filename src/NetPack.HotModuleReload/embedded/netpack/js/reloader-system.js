var onFileChanged = function (msg, data) {
    SystemJS.reload(data);
   // console.log(msg, data);
};

var token = PubSub.subscribe('FileChanged', onFileChanged);