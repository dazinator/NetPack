/// <reference path="./typings/typescript/typescript.d.ts" />
/// <reference path="./typings/node/node.d.ts"/>

import * as ts from "typescript";
import * as path from "path";

declare var __dirname;

import CompilerOptions = ts.CompilerOptions;

function shallowClone(obj) {
    var clone: ts.Map<string> = {};
    for (var k in obj)
        if (obj.hasOwnProperty(k)) {
            clone[k] = obj[k];
        }
    return clone;
}


export class CompositeCompilerHost implements ts.CompilerHost {

    private _currentDirectory: string;
    private _writer: IResultWriterFn;
    private _sources: ts.Map<string> = {};
    private _outputs: ts.Map<string> = {};
    public options: CompilerOptions;

    /**
     * Whether to search for files if a string source isn't found or not
     */
    fallbackToFiles: boolean = false;

    get sources(): ts.Map<string> {
        return shallowClone(this._sources);
    }

    get outputs(): ts.Map<string> {
        return shallowClone(this._outputs);
    }

    readsFrom: SourceType = SourceType.File;
    writesTo: SourceType = SourceType.File;

    constructor(options: CompilerOptions) {
        this.readsFrom = SourceType.File;
        this.getSourceFile = this._readFromFile;

        this.writesTo = SourceType.File;
        this.writeFile = this._writeToFile;

        this.options = options || {};
        // this.options.defaultLibFilename = this.options.defaultLibFilename || '';
    }

    // Implementing CompilerHost interface
    getSourceFile: ISourceReaderFn;

    // Implementing CompilerHost interface
    writeFile: IResultWriterFn;

    readFile = (fileName: string): string => {

        if (this._sources[fileName])
            return this._sources[fileName];

        if (path.normalize(fileName) === this.getDefaultLibFileName())
            return ts.sys.readFile(path.normalize(fileName));

        return "";

    }

    // Implementing CompilerHost interface
    getNewLine = (): string => ts.sys.newLine;

    // Implementing CompilerHost interface
    useCaseSensitiveFileNames(): boolean {
        return ts.sys.useCaseSensitiveFileNames;
    }

    // Implementing CompilerHost interface
    getCurrentDirectory(): string {
        if (this.getSourceFile === this._readFromStrings)
            return "";

        return this._currentDirectory || (this._currentDirectory = ts.sys.getCurrentDirectory());
    }

    // Implementing CompilerHost interface
    getDefaultLibFileName(): string {
        return path.join(__dirname, "lib", "lib.d.ts");
    }

    // Implementing CompilerHost interface
    getCanonicalFileName(fileName: string): string {
        // if underlying system can distinguish between two files whose names differs only in cases then file name already in canonical form.
        // otherwise use toLowerCase as a canonical form.
        return ts.sys.useCaseSensitiveFileNames ? fileName : fileName.toLowerCase();
    }

    // Implementing CompilerHost interface
    fileExists(path: string): boolean {
        return hasProperty(this.sources, path);
    }

    readFromStrings(fallbackToFiles: boolean = false): CompositeCompilerHost {
        this.fallbackToFiles = fallbackToFiles;
        this.readsFrom = SourceType.String;
        this.getSourceFile = this._readFromStrings;
        return this;
    }

    readFromFiles(): CompositeCompilerHost {
        this.readsFrom = SourceType.File;
        this.getSourceFile = this._readFromFile;
        return this;
    }

    addSource(contents: string);
    addSource(name: string, contents: string);
    addSource(nameOrContents, contents?): CompositeCompilerHost {
        var source;

        if (typeof contents == 'undefined')
            source = new StringSource(nameOrContents);
        else
            source = new StringSource(contents, nameOrContents);

        this._sources[source.fileName] = source.contents;
        return this;
    }

    getSourcesFilenames(): string[] {
        var keys = [];

        for (var k in this.sources)
            if (this.sources.hasOwnProperty(k))
                keys.push(k);

        return keys;
    }

    writeToString(): CompositeCompilerHost {
        this.writesTo = SourceType.String;
        this.writeFile = this._writeToString;
        return this;
    }

    writeToFiles(): CompositeCompilerHost {
        this.writesTo = SourceType.File;
        this.writeFile = this._writeToFile;
        return this;
    }

    redirectOutput(writer: boolean);
    redirectOutput(writer: IResultWriterFn);
    redirectOutput(writer): CompositeCompilerHost {
        if (typeof writer == 'function')
            this._writer = writer;
        else
            this._writer = null;

        return this;
    }

    //////////////////////////////
    // private methods
    //////////////////////////////

    private _readFromStrings(filename: string, languageVersion: ts.ScriptTarget, onError?: (message: string) => void): ts.SourceFile {

        if (path.normalize(filename) === this.getDefaultLibFileName())
            return this._readFromFile(filename, languageVersion, onError);

        if (this._sources[filename])
            return ts.createSourceFile(filename, this._sources[filename], languageVersion, true);

        if (this.fallbackToFiles)
            return this._readFromFile(filename, languageVersion, onError);

        return undefined;
    }

    private _writeToString(filename: string, data: string, writeByteOrderMark: boolean, onError?: (message: string) => void) {

        this._outputs[filename] = data;

        if (this._writer)
            this._writer(filename, data, writeByteOrderMark, onError);
    }

    private _readFromFile(filename: string, languageVersion: ts.ScriptTarget, onError?: (message: string) => void): ts.SourceFile {
        try {
            var text = ts.sys.readFile(path.normalize(filename));
        } catch (e) {
            if (onError) {
                onError(e.message);
            }

            text = "";
        }
        return text !== undefined ? ts.createSourceFile(filename, text, languageVersion, true) : undefined;
    }

    private _writeToFile(fileName: string, data: string, writeByteOrderMark: boolean, onError?: (message: string) => void) {
        var existingDirectories: ts.Map<boolean> = {};

        function directoryExists(directoryPath: string): boolean {
            if (hasProperty(existingDirectories, directoryPath)) {
                return true;
            }
            if (ts.sys.directoryExists(directoryPath)) {
                existingDirectories[directoryPath] = true;
                return true;
            }
            return false;
        }

        function ensureDirectoriesExist(directoryPath: string) {
            if (directoryPath.length > getRootLength(directoryPath) && !directoryExists(directoryPath)) {
                var parentDirectory = getDirectoryPath(directoryPath);
                ensureDirectoriesExist(parentDirectory);
                ts.sys.createDirectory(directoryPath);
            }
        }

        try {
            if (this._writer) {
                this._writer(fileName, data, writeByteOrderMark, onError);
            } else {
                ensureDirectoriesExist(getDirectoryPath(normalizePath(fileName)));
                ts.sys.writeFile(fileName, data, writeByteOrderMark);
            }
            this._outputs[fileName] = (writeByteOrderMark ? "\uFEFF" : "") + data;
        } catch (e) {
            if (onError) onError(e.message);
        }
    }
}


// Types
export enum SourceType { File, String }

export interface ISource {
    type: SourceType;
    fileName?: string;
    contents?: string;
}

export class StringSource implements ISource {
    private static _counter = 0;

    type: SourceType = SourceType.String;

    constructor(public contents: string, public fileName: string = StringSource._nextFilename()) {
    }

    private static _nextFilename() {
        return "input_string" + (++StringSource._counter) + '.ts';
    }

    resetCounter() {
        StringSource._counter = 0;
    }
}

class FileSource implements ISource {
    type: SourceType = SourceType.File;

    constructor(public fileName: string) {
    }
}

export interface ICompilationResult {
    sources: { [index: string]: string };
    errors: string[];
}

interface ICompilerOptions {
    defaultLibFilename?: string;
}

export interface ISourceReaderFn {
    (filename: string, languageVersion?: ts.ScriptTarget, onError?: (message: string) => void): ts.SourceFile;
}

export interface IResultWriterFn {
    (filename: string, data: string, writeByteOrderMark?: boolean, onError?: (message: string) => void);
}

const hasOwnProperty = Object.prototype.hasOwnProperty;

function hasProperty<T>(map: ts.Map<T>, key: string): boolean {
    return hasOwnProperty.call(map, key);
}

function getRootLength(path: string): number {
    if (path.charCodeAt(0) === CharacterCodes.slash) {
        if (path.charCodeAt(1) !== CharacterCodes.slash) return 1;
        const p1 = path.indexOf("/", 2);
        if (p1 < 0) return 2;
        const p2 = path.indexOf("/", p1 + 1);
        if (p2 < 0) return p1 + 1;
        return p2 + 1;
    }
    if (path.charCodeAt(1) === CharacterCodes.colon) {
        if (path.charCodeAt(2) === CharacterCodes.slash) return 3;
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
    const idx = path.indexOf("://");
    if (idx !== -1) {
        return idx + "://".length;
    }
    return 0;
}

const enum CharacterCodes {
    nullCharacter = 0,
    maxAsciiCharacter = 0x7F,

    lineFeed = 0x0A,              // \n
    carriageReturn = 0x0D,        // \r
    lineSeparator = 0x2028,
    paragraphSeparator = 0x2029,
    nextLine = 0x0085,

    // Unicode 3.0 space characters
    space = 0x0020,   // " "
    nonBreakingSpace = 0x00A0,   //
    enQuad = 0x2000,
    emQuad = 0x2001,
    enSpace = 0x2002,
    emSpace = 0x2003,
    threePerEmSpace = 0x2004,
    fourPerEmSpace = 0x2005,
    sixPerEmSpace = 0x2006,
    figureSpace = 0x2007,
    punctuationSpace = 0x2008,
    thinSpace = 0x2009,
    hairSpace = 0x200A,
    zeroWidthSpace = 0x200B,
    narrowNoBreakSpace = 0x202F,
    ideographicSpace = 0x3000,
    mathematicalSpace = 0x205F,
    ogham = 0x1680,

    _ = 0x5F,
    $ = 0x24,

    _0 = 0x30,
    _1 = 0x31,
    _2 = 0x32,
    _3 = 0x33,
    _4 = 0x34,
    _5 = 0x35,
    _6 = 0x36,
    _7 = 0x37,
    _8 = 0x38,
    _9 = 0x39,

    a = 0x61,
    b = 0x62,
    c = 0x63,
    d = 0x64,
    e = 0x65,
    f = 0x66,
    g = 0x67,
    h = 0x68,
    i = 0x69,
    j = 0x6A,
    k = 0x6B,
    l = 0x6C,
    m = 0x6D,
    n = 0x6E,
    o = 0x6F,
    p = 0x70,
    q = 0x71,
    r = 0x72,
    s = 0x73,
    t = 0x74,
    u = 0x75,
    v = 0x76,
    w = 0x77,
    x = 0x78,
    y = 0x79,
    z = 0x7A,

    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5a,

    ampersand = 0x26,             // &
    asterisk = 0x2A,              // *
    at = 0x40,                    // @
    backslash = 0x5C,             // \
    backtick = 0x60,              // `
    bar = 0x7C,                   // |
    caret = 0x5E,                 // ^
    closeBrace = 0x7D,            // }
    closeBracket = 0x5D,          // ]
    closeParen = 0x29,            // )
    colon = 0x3A,                 // :
    comma = 0x2C,                 // ,
    dot = 0x2E,                   // .
    doubleQuote = 0x22,           // "
    equals = 0x3D,                // =
    exclamation = 0x21,           // !
    greaterThan = 0x3E,           // >
    hash = 0x23,                  // #
    lessThan = 0x3C,              // <
    minus = 0x2D,                 // -
    openBrace = 0x7B,             // {
    openBracket = 0x5B,           // [
    openParen = 0x28,             // (
    percent = 0x25,               // %
    plus = 0x2B,                  // +
    question = 0x3F,              // ?
    semicolon = 0x3B,             // ;
    singleQuote = 0x27,           // '
    slash = 0x2F,                 // /
    tilde = 0x7E,                 // ~

    backspace = 0x08,             // \b
    formFeed = 0x0C,              // \f
    byteOrderMark = 0xFEFF,
    tab = 0x09,                   // \t
    verticalTab = 0x0B,           // \v
}

let directorySeparator = "/";
function getDirectoryPath(path: string): string;
function getDirectoryPath(path: string): any {
    return path.substr(0, Math.max(getRootLength(path), path.lastIndexOf(directorySeparator)));
}

function normalizePath(path: string): string {
    path = normalizeSlashes(path);
    const rootLength = getRootLength(path);
    const normalized = getNormalizedParts(path, rootLength);
    return path.substr(0, rootLength) + normalized.join(directorySeparator);
}

function normalizeSlashes(path: string): string {
    return path.replace(/\\/g, "/");
}

function getNormalizedParts(normalizedSlashedPath: string, rootLength: number) {
    const parts = normalizedSlashedPath.substr(rootLength).split(directorySeparator);
    const normalized: string[] = [];
    for (const part of parts) {
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
function lastOrUndefined<T>(array: T[]): T {
    if (array.length === 0) {
        return undefined;
    }

    return array[array.length - 1];
}


