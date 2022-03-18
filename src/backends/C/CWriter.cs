using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

// TODO do everything in a single loop?

namespace C
{
    class CWriter
    {
        private CProgram program;
        private string cFile = CBase.Base.TrimStart();
        // private Dictionary<Tuple<int, int>, int> types = new Dictionary<Tuple<int, int>, int>();

        public CWriter(CProgram program)
        {
            this.program = program;
        }

        // we need to generate 4 sections and the header
        // 1. type section: describes different function signatures
        // 2. import section: describes imported functions
        // 3. function section: describes functions and attaches them to a type
        // 4. code section: the actual function code
        public string Write()
        {
            // wasm.AddRange(WASMbase.Header);
            // generateTypes();
            // wasm.AddRange(WASMbase.Import);
            // generateFunctionMeta();
            generateFunctionCode();
            // generateData();
            return cFile;
        }

        // TODO fix length stuff
        private void generateFunctionCode()
        {
            // Forward declarations
            // TODO a lot of repeating code
            foreach (var func in program.functions)
            {
                var returnType = func.ReturnType;
                var name = func.Name;
                if (func.Name == "__main__") {
                    continue;
                }

                cFile += string.Format("{0} {1}(", returnType, name);
                if (func.Parameters.Count == 0) {
                    cFile += ")";
                } else {
                    foreach (var param in func.Parameters)
                    {
                        // TODO pass by ref
                        // TODO move type transformations to generator
                        var type = Util.TypeToCType(param.Item2);
                        cFile += string.Format("{0} {1},", type, param.Item1);
                    }
                    cFile = cFile.Remove(cFile.Length - 1) + ")";
                };

                cFile += ";\n";
            }
            cFile += '\n';
            foreach (var func in program.functions)
            {
                var returnType = func.ReturnType;
                var name = func.Name;
                if (func.Name == "__main__") {
                    returnType = "int";
                    name = "main";
                }

                cFile += string.Format("{0} {1}(", returnType, name);
                if (func.Parameters.Count == 0) {
                    cFile += ") {\n";
                } else {
                    foreach (var param in func.Parameters)
                    {
                        // TODO pass by ref
                        // TODO move type transformations to generator
                        var type = Util.TypeToCType(param.Item2);
                        cFile += string.Format("{0} {1},", type, param.Item1);
                    }
                    cFile = cFile.Remove(cFile.Length - 1) + ") {\n";
                }

                foreach (var stmt in func.Body)
                {
                    cFile += stmt +'\n';
                }

                cFile += "}\n";
            }

        }
    }
}
