/// <reference path="./typings/typescript/typescript.d.ts" />
/// <reference path="./typings/node/node.d.ts"/>
"use strict";
var ts = require("typescript");
var path = require("path");
function shallowClone(obj) {
    var clone = {};
    for (var k in obj)
        if (obj.hasOwnProperty(k)) {
            clone[k] = obj[k];
        }
    return clone;
}
var CompositeCompilerHost = (function () {
    function CompositeCompilerHost(options) {
        var _this = this;
        this._sources = {};
        this._outputs = {};
        /**
         * Whether to search for files if a string source isn't found or not
         */
        this.fallbackToFiles = false;
        this.readsFrom = SourceType.File;
        this.writesTo = SourceType.File;
        this.readFile = function (fileName) {
            if (_this._sources[fileName])
                return _this._sources[fileName];
            if (path.normalize(fileName) === _this.getDefaultLibFileName())
                return ts.sys.readFile(path.normalize(fileName));
            return "";
        };
        // Implementing CompilerHost interface
        this.getNewLine = function () { return ts.sys.newLine; };
        this.readsFrom = SourceType.File;
        this.getSourceFile = this._readFromFile;
        this.writesTo = SourceType.File;
        this.writeFile = this._writeToFile;
        this.options = options || {};
        // this.options.defaultLibFilename = this.options.defaultLibFilename || '';
    }
    Object.defineProperty(CompositeCompilerHost.prototype, "sources", {
        get: function () {
            return shallowClone(this._sources);
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(CompositeCompilerHost.prototype, "outputs", {
        get: function () {
            return shallowClone(this._outputs);
        },
        enumerable: true,
        configurable: true
    });
    // Implementing CompilerHost interface
    CompositeCompilerHost.prototype.useCaseSensitiveFileNames = function () {
        return ts.sys.useCaseSensitiveFileNames;
    };
    // Implementing CompilerHost interface
    CompositeCompilerHost.prototype.getCurrentDirectory = function () {
        if (this.getSourceFile === this._readFromStrings)
            return "";
        return this._currentDirectory || (this._currentDirectory = ts.sys.getCurrentDirectory());
    };
    // Implementing CompilerHost interface
    CompositeCompilerHost.prototype.getDefaultLibFileName = function () {
        return path.join(__dirname, "lib", "lib.d.ts");
    };
    // Implementing CompilerHost interface
    CompositeCompilerHost.prototype.getCanonicalFileName = function (fileName) {
        // if underlying system can distinguish between two files whose names differs only in cases then file name already in canonical form.
        // otherwise use toLowerCase as a canonical form.
        return ts.sys.useCaseSensitiveFileNames ? fileName : fileName.toLowerCase();
    };
    // Implementing CompilerHost interface
    CompositeCompilerHost.prototype.fileExists = function (path) {
        return hasProperty(this.sources, path);
    };
    CompositeCompilerHost.prototype.readFromStrings = function (fallbackToFiles) {
        if (fallbackToFiles === void 0) { fallbackToFiles = false; }
        this.fallbackToFiles = fallbackToFiles;
        this.readsFrom = SourceType.String;
        this.getSourceFile = this._readFromStrings;
        return this;
    };
    CompositeCompilerHost.prototype.readFromFiles = function () {
        this.readsFrom = SourceType.File;
        this.getSourceFile = this._readFromFile;
        return this;
    };
    CompositeCompilerHost.prototype.addSource = function (nameOrContents, contents) {
        var source;
        if (typeof contents == 'undefined')
            source = new StringSource(nameOrContents);
        else
            source = new StringSource(contents, nameOrContents);
        this._sources[source.fileName] = source.contents;
        return this;
    };
    CompositeCompilerHost.prototype.getSourcesFilenames = function () {
        var keys = [];
        for (var k in this.sources)
            if (this.sources.hasOwnProperty(k))
                keys.push(k);
        return keys;
    };
    CompositeCompilerHost.prototype.writeToString = function () {
        this.writesTo = SourceType.String;
        this.writeFile = this._writeToString;
        return this;
    };
    CompositeCompilerHost.prototype.writeToFiles = function () {
        this.writesTo = SourceType.File;
        this.writeFile = this._writeToFile;
        return this;
    };
    CompositeCompilerHost.prototype.redirectOutput = function (writer) {
        if (typeof writer == 'function')
            this._writer = writer;
        else
            this._writer = null;
        return this;
    };
    //////////////////////////////
    // private methods
    //////////////////////////////
    CompositeCompilerHost.prototype._readFromStrings = function (filename, languageVersion, onError) {
        if (path.normalize(filename) === this.getDefaultLibFileName())
            return this._readFromFile(filename, languageVersion, onError);
        if (this._sources[filename])
            return ts.createSourceFile(filename, this._sources[filename], languageVersion, true);
        if (this.fallbackToFiles)
            return this._readFromFile(filename, languageVersion, onError);
        return undefined;
    };
    CompositeCompilerHost.prototype._writeToString = function (filename, data, writeByteOrderMark, onError) {
        this._outputs[filename] = data;
        if (this._writer)
            this._writer(filename, data, writeByteOrderMark, onError);
    };
    CompositeCompilerHost.prototype._readFromFile = function (filename, languageVersion, onError) {
        try {
            var text = ts.sys.readFile(path.normalize(filename));
        }
        catch (e) {
            if (onError) {
                onError(e.message);
            }
            text = "";
        }
        return text !== undefined ? ts.createSourceFile(filename, text, languageVersion, true) : undefined;
    };
    CompositeCompilerHost.prototype._writeToFile = function (fileName, data, writeByteOrderMark, onError) {
        var existingDirectories = {};
        function directoryExists(directoryPath) {
            if (hasProperty(existingDirectories, directoryPath)) {
                return true;
            }
            if (ts.sys.directoryExists(directoryPath)) {
                existingDirectories[directoryPath] = true;
                return true;
            }
            return false;
        }
        function ensureDirectoriesExist(directoryPath) {
            if (directoryPath.length > getRootLength(directoryPath) && !directoryExists(directoryPath)) {
                var parentDirectory = getDirectoryPath(directoryPath);
                ensureDirectoriesExist(parentDirectory);
                ts.sys.createDirectory(directoryPath);
            }
        }
        try {
            if (this._writer) {
                this._writer(fileName, data, writeByteOrderMark, onError);
            }
            else {
                ensureDirectoriesExist(getDirectoryPath(normalizePath(fileName)));
                ts.sys.writeFile(fileName, data, writeByteOrderMark);
            }
            this._outputs[fileName] = (writeByteOrderMark ? "\uFEFF" : "") + data;
        }
        catch (e) {
            if (onError)
                onError(e.message);
        }
    };
    return CompositeCompilerHost;
}());
exports.CompositeCompilerHost = CompositeCompilerHost;
// Types
(function (SourceType) {
    SourceType[SourceType["File"] = 0] = "File";
    SourceType[SourceType["String"] = 1] = "String";
})(exports.SourceType || (exports.SourceType = {}));
var SourceType = exports.SourceType;
var StringSource = (function () {
    function StringSource(contents, fileName) {
        if (fileName === void 0) { fileName = StringSource._nextFilename(); }
        this.contents = contents;
        this.fileName = fileName;
        this.type = SourceType.String;
    }
    StringSource._nextFilename = function () {
        return "input_string" + (++StringSource._counter) + '.ts';
    };
    StringSource.prototype.resetCounter = function () {
        StringSource._counter = 0;
    };
    StringSource._counter = 0;
    return StringSource;
}());
exports.StringSource = StringSource;
var FileSource = (function () {
    function FileSource(fileName) {
        this.fileName = fileName;
        this.type = SourceType.File;
    }
    return FileSource;
}());
var hasOwnProperty = Object.prototype.hasOwnProperty;
function hasProperty(map, key) {
    return hasOwnProperty.call(map, key);
}
function getRootLength(path) {
    if (path.charCodeAt(0) === 47 /* slash */) {
        if (path.charCodeAt(1) !== 47 /* slash */)
            return 1;
        var p1 = path.indexOf("/", 2);
        if (p1 < 0)
            return 2;
        var p2 = path.indexOf("/", p1 + 1);
        if (p2 < 0)
            return p1 + 1;
        return p2 + 1;
    }
    if (path.charCodeAt(1) === 58 /* colon */) {
        if (path.charCodeAt(2) === 47 /* slash */)
            return 3;
        return 2;
    }
    // Per RFC 1738 'file' URI schema has the shape file://<host>/<path>
    // if <host> is omitted then it is assumed that host value is 'localhost',
    // however slash after the omitted <host> is not removed.
    // file:///folder1/file1 - this is a correct URI
    // file://folder2/file2 - this is an incorrect URI
    if (path.lastIndexOf("file:///", 0) === 0) {
        return "file:///".length;
    }
    var idx = path.indexOf("://");
    if (idx !== -1) {
        return idx + "://".length;
    }
    return 0;
}
var directorySeparator = "/";
function getDirectoryPath(path) {
    return path.substr(0, Math.max(getRootLength(path), path.lastIndexOf(directorySeparator)));
}
function normalizePath(path) {
    path = normalizeSlashes(path);
    var rootLength = getRootLength(path);
    var normalized = getNormalizedParts(path, rootLength);
    return path.substr(0, rootLength) + normalized.join(directorySeparator);
}
function normalizeSlashes(path) {
    return path.replace(/\\/g, "/");
}
function getNormalizedParts(normalizedSlashedPath, rootLength) {
    var parts = normalizedSlashedPath.substr(rootLength).split(directorySeparator);
    var normalized = [];
    for (var _i = 0, parts_1 = parts; _i < parts_1.length; _i++) {
        var part = parts_1[_i];
        if (part !== ".") {
            if (part === ".." && normalized.length > 0 && lastOrUndefined(normalized) !== "..") {
                normalized.pop();
            }
            else {
                // A part may be an empty string (which is 'falsy') if the path had consecutive slashes,
                // e.g. "path//file.ts".  Drop these before re-joining the parts.
                if (part) {
                    normalized.push(part);
                }
            }
        }
    }
    return normalized;
}
/**
 * Returns the last element of an array if non-empty, undefined otherwise.
 */
function lastOrUndefined(array) {
    if (array.length === 0) {
        return undefined;
    }
    return array[array.length - 1];
}
//# sourceMappingURL=compilerhost.js.map