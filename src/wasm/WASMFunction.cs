using System;
using System.Collections.Generic;
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