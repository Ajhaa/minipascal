using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

// TODO do everything in a single loop?

namespace WASM
{
    class WASMwriter
    {
        private WASM program;
        private List<byte> wasm = new List<byte>();
        private Dictionary<Tuple<int, int>, int> types = new Dictionary<Tuple<int, int>, int>();

        public WASMwriter(WASM program)
        {
            this.program = program;
            types.Add(new Tuple<int, int>(1, 0), 0);
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
            generateData();
            return wasm;
        }

        private void generateTypes()
        {
            var typeIndex = 1;
            var index = wasm.Count + 1;
            wasm.AddRange(new byte[] {
            0x01, 0x00, 0x00,
            0x60, 0x01, 0x7f, 0x00
        });
            foreach (var func in program.functions)
            {
                if (!types.ContainsKey(func.Signature))
                {
                    types.Add(func.Signature, typeIndex);
                    typeIndex++;
                    wasm.AddRange(new byte[] {
                    0x60, Convert.ToByte(func.Signature.Item1)
                });
                    for (var i = 0; i < func.Signature.Item1; i++)
                    {
                        wasm.Add(0x7f);
                    }
                    wasm.Add(Convert.ToByte(func.Signature.Item2));
                    if (func.Signature.Item2 == 1)
                    {
                        wasm.Add(0x7f);
                    }
                }
            }

            var sectSize = wasm.Count - index - 1;
            var encSectSize = Util.LEB128encode(sectSize);
            //replace placeholder with actual section size
            wasm[index] = encSectSize[0];
            if (encSectSize.Count > 1)
            {
                wasm.InsertRange(index + 1, encSectSize.Skip(1));
            }

            wasm[index + encSectSize.Count] = Convert.ToByte(types.Count);
        }

        private void generateData()
        {

            wasm.AddRange(new byte[] { 0x0b, 0x00 });
            var index = wasm.Count - 1;

            wasm.AddRange(Util.LEB128encode(program.data.Count));
            foreach (var segment in program.data)
            {
                var pointer = Util.LEB128encode(segment.Item1);
                wasm.AddRange(new byte[] {
                0x00, 0x41 });
                wasm.AddRange(pointer);
                wasm.AddRange(new byte[] {
                0x0b, Convert.ToByte(segment.Item2.Length + 1)
            });
                // TODO store string length as bigger number
                var bytes = Encoding.UTF8.GetBytes(segment.Item2);
                wasm.Add(Convert.ToByte(bytes.Length));
                wasm.AddRange(bytes);
            }

            var sectSize = wasm.Count - index - 1;
            var encSectSize = Util.LEB128encode(sectSize);
            //replace placeholder with actual section size
            wasm[index] = encSectSize[0];
            if (encSectSize.Count > 1)
            {
                wasm.InsertRange(index + 1, encSectSize.Skip(1));
            }
        }

        private void generateFunctionMeta()
        {
            var size = Util.LEB128encode(program.Count() + 1);
            wasm.Add(0x03);
            wasm.AddRange(size);
            wasm.AddRange(Util.LEB128encode(program.Count()));
            foreach (var func in program.functions)
            {
                wasm.Add(Convert.ToByte(types[func.Signature]));
            }
        }

        private void generateExports()
        {
            // TODO increase the number when more funcs imported
            var mainIndex = program.functions.FindIndex(p => p.Name == "__main__") + 1;
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

            wasm.AddRange(Util.LEB128encode(program.Count()));

            foreach (var func in program.functions)
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
                wasm.InsertRange(index + 1, encSectSize.Skip(1));
            }
        }
    }
}
