using System.Collections.Generic;
using System;
using System.Linq;

// TODO do everything in a single loop?
class WASMwriter
{
    private List<WASMFunction> program;
    private List<byte> wasm = new List<byte>();
    private HashSet<Tuple<int, int>> types = new HashSet<Tuple<int, int>>();

    public WASMwriter(List<WASMFunction> program)
    {
        this.program = program;
        types.Add(new Tuple<int, int>(1, 0));
    }

    // we need to generate 4 sections and the header
    // 1. type section: describes different function signatures
    // 2. import section: describes imported functions
    // 3. function section: describes functions and attaches them to a type
    // 4. code section: the actual function code 
    public List<byte> Write()
    {
        wasm.AddRange(WASMbase.Header);
        generateTypes();
        wasm.AddRange(WASMbase.Import);
        generateFunctionMeta();
        generateExports();
        generateFunctionCode();
        return wasm;
    }

    private void generateTypes()
    {
        var typeIndex = 0;
        var patchIndex = wasm.Count + 1;
        wasm.AddRange(new byte[] {
            0x01, 0x08, 0x02,
            0x60, 0x01, 0x7f, 0x00,
            0x60, 0x00, 0x00
        });
        foreach (var func in program)
        {
            if (types.Add(func.Signature))
            {

            }
        }
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

    private void generateExports()
    {
        var mainIndex = program.FindIndex(p => p.Name == "__main__") + 1; // +1 because write is a func
        wasm.AddRange(new Byte[] {
            0x07, 0x0c, 0x01, 0x08,

            0x5f, 0x5f, 0x6d, 0x61, 
            0x69, 0x6e, 0x5f, 0x5f,

            0x00, Convert.ToByte(mainIndex)
        });
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