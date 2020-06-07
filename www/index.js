import { setCodeWindow, getCode } from './codeInput.js';

const compileButton = document.getElementById("compile-and-run");
const outputWindow = document.getElementById("output-window");
const fileInput = document.getElementById("file-input");


compileButton.onclick = () => {
    var memory = new WebAssembly.Memory({ initial: 64 });
    outputWindow.value = "compiling...\n";
    fetch("http://localhost:3001/", {
        method: 'POST',
        mode: 'cors',
        cache: 'no-cache', 
        credentials: 'same-origin',
        headers: {
          'Content-Type': 'text/plain'
        },
        redirect: 'follow',
        referrerPolicy: 'no-referrer', 
        body: getCode()
    })
    .then(res => res.arrayBuffer())
    .then(buf => {
        WebAssembly.instantiate(buf, { js: { mem: memory, write: (val) => outputWindow.value += val + "\n" } }).then(wasm => {
            wasm.instance.exports.__main__();
        })
    });


    // fetch("http://localhost:3001")
    //     .then(res => res.json())
    //     .then(({ data }) => outputWindow.value += data[0].employee_name)
    //     .catch(e => console.log("Error", e));
};

fileInput.oninput = () => {
    const file = fileInput.files[0];
    file.text().then(setCodeWindow);
}
