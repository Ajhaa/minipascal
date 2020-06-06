const fs = require("fs");
// const readlineSync = require('readline-sync');
// const textEncoding = require('text-encoding');

var memory = new WebAssembly.Memory({ initial: 4 });

function log(offset, length) {
    var bytes = new Uint8Array(memory.buffer, offset, length+1);    
    let s = ""
    for (char of bytes) {
        s += String.fromCharCode(char)
    }
    console.log(s);
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

function dispInt(val) {
    console.log(">", val);
}

const js = { read: read, mem: memory, log, write: log }
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

run().catch(e => console.error("Error:", e));