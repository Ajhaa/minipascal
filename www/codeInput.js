const codeWindow = document.getElementById("code-window");

codeWindow.onkeydown = (keydown) => {
    const { key } = keydown;

    if (key === "Tab") {
        keydown.preventDefault();
        keydown.stopPropagation();
        codeWindow.value += "\t";
    }
};

export const setCodeWindow = (text) => {
    codeWindow.value = text;
}

export const getCode = () => {
    return codeWindow.value;
}