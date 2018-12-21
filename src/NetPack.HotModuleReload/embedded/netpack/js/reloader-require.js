define("@hot", [], function () { return {}; });

var onFileChanged = function (msg, filename) {
    var allModules = Object.keys(require.s.contexts._.defined);
    var fileExtension = filename.slice((filename.lastIndexOf(".") - 1 >>> 0) + 2);

    var foundModuleId = allModules.find(function (moduleName) {
        var moduleNameWithExtension = moduleName;
        if (fileExtension) {
            moduleNameWithExtension = moduleName + "." + fileExtension;
        }
        var moduleUrl = require.toUrl(moduleNameWithExtension);
        return moduleUrl == filename;
    });


    if (foundModuleId) {
        // first allow the current instance of module to unload.
        var oldInstance = require.s.contexts._.defined[foundModuleId];
        if (oldInstance) {
            if (typeof oldInstance.__unload === "function") {
                oldInstance.__unload();
            };
        }

        require.undef(foundModuleId);
        require.undef("@hot");
        define("@hot", [], function () { return oldInstance; });

        var hot = require(['require', '@@hot', foundModuleId], function (require, oldInstance, newInstance) {
            //   require(['require', foundModuleId], function (require, foundModuleId) {
            PubSub.publishSync('ModuleReloaded', { Id: foundModuleId, old: oldInstance, new: newInstance });
            //  });
        });


        //require([foundModuleId, "@@hot"], (mod) => {                                         
        //    //  define("@@hot", [], function () { return {}; });

        //});

    }
};

var token = PubSub.subscribe('FileChanged', onFileChanged);

//return { token: token };





//var onFileChanged = function (msg, data) {
//    requirejs.undef(data);
//    require(data, () => {
//        PubSub.publishSync('RenderModule', data);
//    });
//    //SystemJS.reload(data);
//   // console.log(msg, data);
//};

