using System.Collections.Generic;
using System;

class WASMwriter
{
    private List<WASMFunction> program;
    private List<byte> wasm = new List<byte>();

    public WASMwriter(List<WASMFunction> program)
    {
        this.program = program;
    }

    // we need to generate 4 sections and the header
    // 1. type section: describes different function signatures
    // TODO 2. import section: describes imported functions
    // 3. function section: describes functions and attaches them to a type
    // 4. code section: the actual function code 
    public List<byte> Write()
    {
        wasm.AddRange(WASMbase.Header);
        generateTypes();
        wasm.AddRange(WASMbase.Import);
        generateFunctionMeta();
        generateFunctionCode();
        return wasm;
    }

    private void generateTypes()
    {
        wasm.AddRange(new byte[] { 
            0x01, 0x04, 0x01,
            0x60, 0x00, 0x00
        });
    }

    private void generateFunctionMeta()
    {
        wasm.AddRange(new byte[] {
            0x03, 0x02, 0x01, 0x00
        });
    }

    private void generateFunctionCode()
    {
        var size = program[0].Body.Count + 4;
        var sectSize = size + 2;
        var localCount = Convert.ToByte(program[0].Locals);

        wasm.Add(0x0a);
        wasm.AddRange(Util.LEB128encode(sectSize));
        wasm.Add(0x01);

        wasm.AddRange(Util.LEB128encode(size));
        wasm.AddRange(new byte[] {
            0x01, localCount, 0x7f
        });

        wasm.AddRange(program[0].Body);
        wasm.Add(0x0b);
    }
}