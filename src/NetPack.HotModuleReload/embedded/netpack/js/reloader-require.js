var onFileChanged = function (msg, data) {
    requirejs.undef(data);
    require(data, () => {
        PubSub.publishSync('RenderModule', data);
    });
    //SystemJS.reload(data);
   // console.log(msg, data);
};

var token = PubSub.subscribe('FileChanged', onFileChanged);