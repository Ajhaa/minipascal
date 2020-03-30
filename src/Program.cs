﻿using System;
using System.IO;

namespace minipascal
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileAsString = File.ReadAllText(args[0]);
            var tokens = new Scanner(fileAsString).Scan();

            var parser = new Parser(tokens);

            var program = parser.Parse();

            var wasm = new Generator(program).Generate();

            var binary = new WASMwriter(wasm).Write();

            var bWriter = new BinaryWriter(File.Open("pascal.wasm", FileMode.Create));
            
            foreach (var b in binary)
            {
                bWriter.Write(b);
            }

            bWriter.Close();
        }
    }
}
