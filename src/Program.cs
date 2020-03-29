using System;
using System.IO;

namespace minipascal
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileAsString = File.ReadAllText(args[0]);
            var tokens = new Scanner(fileAsString).Scan();
            foreach (var token in tokens) 
            {
                Console.WriteLine(token);
            }

            var parser = new Parser(tokens);

            var program = parser.Parse();

            var wasm = new Generator(program).Generate();

            foreach (var b in wasm[0].Body)
            {
                Console.WriteLine(b.ToString("X2"));
            }
        }
    }
}
