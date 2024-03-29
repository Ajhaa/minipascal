const fs = require("fs");
// const readlineSync = require('readline-sync');
// const textEncoding = require('text-encoding');

var memory = new WebAssembly.Memory({ initial: 4 });

function log(offset) {
    const location = new Uint32Array(new Uint8Array(memory.buffer, offset))[0];
    // console.log("offset", location)
    let bytes = new Uint8Array(memory.buffer, location);
    const length = Number(bytes[0])
    // console.log("length", length)    
    let s = ""
    for (char of bytes.slice(1, length + 1)) {
        s += String.fromCharCode(char)
    }
    console.log(s);
}

function intToString(offset, pointer) {
    let int = new Int32Array(memory.buffer, offset)[0];
    let memory = new Uint8Array(memory.buffer, pointer);

    let str = int.toString()

    memory[0] = str.length

    for (let i = 1; i < str.length; i++) {
        memory[i] = str[i]
    }
}

function read(offset) {
    const data = readlineSync.prompt()
    const memAccess = new Uint8Array(memory.buffer);
    let i = offset;
    for (offset; i < data.length + offset; i++) {
        memAccess[i] = data.charCodeAt(i-offset)
    }

    memAccess[i] = 0;

    return data.length;
}

function dispInt(offset) {
    let bytes = new Uint8Array(memory.buffer, offset);
    let int = bytes[0];
    int += bytes[1] * 256;
    int += bytes[2] * 256 ** 2;
    int += bytes[3] * 256 ** 3
    console.log(">", int);
}

const js = { read: read, mem: memory, write: dispInt, toString: intToString }
const core = { memory }

async function run() {
    //const stdFile = fs.readFileSync(".wasm");
    //const stdModule = await WebAssembly.instantiate(stdFile, { core, js });
    //const stdlib = stdModule.instance.exports;

    fs.readFile("pascal.wasm", {}, (err, data) => {
        WebAssembly.instantiate(data, { core, js }).then(wasm => {
            wasm.instance.exports.__main__();
        })
    });
}

run();