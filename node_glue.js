const fs = require("fs");
const readlineSync = require('readline-sync');
const textEncoding = require('text-encoding');

var memory = new WebAssembly.Memory({ initial: 4 });

function log(offset) {
    var bytes = new Uint8Array(memory.buffer, offset);    
    let i = 0;
    let s = ""
    while (true) {
        if (bytes[i] != 0) {
            s += String.fromCharCode(bytes[i]);
        } else {
            break;
        }
        i++;
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
    console.log(val);
    return val;
}

const js = { read: read, mem: memory, log, dispInt }
const core = { memory }

async function run() {
    const stdFile = fs.readFileSync("stdlib.wasm");
    const stdModule = await WebAssembly.instantiate(stdFile, { core, js });
    const stdlib = stdModule.instance.exports;

    fs.readFile("test.wasm", {}, (err, data) => {
        WebAssembly.instantiate(data, { stdlib, core, js }).then(wasm => {
            console.log(wasm.instance.exports.main());
        })
    });
}

run().catch(e => console.error("Error:", e));