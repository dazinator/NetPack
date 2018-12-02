//require.config({
//    // app entry point
//    deps: ["SomePage"],
//    //By default load any module IDs from amd
//    baseUrl: '/amd',
//    //except, if the module ID starts with "app",
//    //load it from the js/app directory. paths
//    //config is relative to the baseUrl, and
//    //never includes a ".js" extension since
//    //the paths config could be for a directory.
//    paths: {
//        '@hot': 'empty:'
//    }
//});


var require = {
    baseUrl: '/amd',
    waitSeconds: 0,
    paths: {
        '@hot': 'empty:'
    }
};

