require(["ModuleB"], function (moduleB) {
    console.log("hi this is a page level module.");
    moduleB.addModuleToList("modules","SomePage");
});