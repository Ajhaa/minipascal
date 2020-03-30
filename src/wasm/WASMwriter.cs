using System.Collections.Generic;
using System;
using System.Linq;
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
            0x01, 0x08, 0x02,
            0x60, 0x01, 0x7f, 0x00,
            0x60, 0x00, 0x00
        });
    }

    private void generateFunctionMeta()
    {
        var size = Util.LEB128encode(program.Count + 1);
        wasm.Add(0x03);
        wasm.AddRange(size);
        wasm.AddRange(Util.LEB128encode(program.Count));
        foreach (var func in program)
        {
            wasm.Add(0x01); // for now everything is () => ()
        }
    }


    // TODO fix length stuff
    private void generateFunctionCode()
    {
        wasm.Add(0x0a);
        wasm.Add(0); // placeholder for section size
        var index = wasm.Count - 1;

        wasm.AddRange(Util.LEB128encode(program.Count));

        foreach (var func in program)
        {
            // TODO currently allows only 64 variables per function
            var localCount = Convert.ToByte(func.Locals);
            var size = func.Body.Count + 4;
            var encSize = Util.LEB128encode(size);
            wasm.AddRange(encSize);
            wasm.AddRange(new byte[] {
                0x01, localCount, 0x7f
            });

            wasm.AddRange(func.Body);
            wasm.Add(0x0b);
        }

        var sectSize = wasm.Count - index - 1;
        var encSectSize = Util.LEB128encode(sectSize);
        //replace placeholder with actual section size
        wasm[index] = encSectSize[0];
        if (encSectSize.Count > 1)
        {
            wasm.InsertRange(index+1, encSectSize.Skip(1));
        }
    }
}