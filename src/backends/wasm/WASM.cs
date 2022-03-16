using System;
using System.Collections.Generic;

namespace WASM
{
    public class WASM
    {

        public List<WASMFunction> functions = new List<WASMFunction>();
        public List<Tuple<int, string>> data = new List<Tuple<int, string>>();

        public void addFunction(WASMFunction func)
        {
            functions.Add(func);
        }

        public void addData(int index, string content)
        {
            data.Add(new Tuple<int, string>(index, content));
        }

        public int Count()
        {
            return functions.Count;
        }
    }

    public enum Instruction : byte
    {
        ZERO = 0x0,
        LOCAL_GET = 0x20,
        LOCAL_SET = 0X21,
        LOCAL_TEE = 0x22,
        I32_CONST = 0x41,

    }

    public class WASMbase
    {
        public static byte[] Header = new byte[] {
        0x00, 0x61, 0x73, 0x6d, // wasm magic
        0x01, 0x00, 0x00, 0x00, // wasm version number
    };

        // need to import three things from js
        // the linear memory, and two i/o functions

        // TODO remember to change import number
        public static byte[] Import = new byte[] {
        0x02, 0x16, 0x02, // section code, size and number of imports

        0x02, 0x6a, 0x73, // first import from 'js'
        0x03, 0x6d, 0x65, 0x6d, // 'mem'
        0x02, 0x00, 0x04, // initial mem size of at least 4 pages (256KiB)

        0x02, 0x6a, 0x73, // second import from 'js'
        0x05, 0x77, 0x72, 0x69, 0x74, 0x65, // 'write'
        0x00, 0x00

        // 0x02, 0x6a, 0x73, // third import from 'js'
        // 0x04, 0x72, 0x65, 0x61, 0x64, // 'read'
        // 0x00, 0x00,
    };
    }

    public class WASMFunction
    {
        // params and return values
        public Tuple<int, int> Signature { get; }
        // Number of local variables
        public int Locals { get; set; }

        // Name is used if functio is wasm exported
        public string Name { get; }
        public List<byte> Body { get; }
        public int TypeIndex { get; set; }

        public WASMFunction(int parameters, int results, string name)
        {
            Signature = Tuple.Create(parameters, results);
            Name = name;
            Body = new List<byte>();
            Locals = 0;
        }

    }
}
