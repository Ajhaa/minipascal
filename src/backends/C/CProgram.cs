using System;
using System.Collections.Generic;

namespace C
{
    // TODO globals?
    class CProgram
    {

        public List<CFunction> functions = new List<CFunction>();

        public void addFunction(CFunction func)
        {
            functions.Add(func);
        }

        public int Count()
        {
            return functions.Count;
        }
    }


    class CFunction
    {
        // params and return values
        public List<Tuple<string, string>> Parameters { get; }

        // TODO return type mapping
        public string ReturnType { get; }
        // Number of local variables
        public int Locals { get; set; }

        // Name is used if functio is wasm exported
        public string Name { get; }
        public List<string> Body { get; }

        public CFunction(string returnType, string name)
        {
            Parameters = new List<Tuple<string, string>>();
            ReturnType = returnType;
            Name = name;
            Body = new List<string>();
        }

        public void AddParam(string name, string type)
        {
            Parameters.Add(new Tuple<string, string>(name, type));
        }
    }
}
