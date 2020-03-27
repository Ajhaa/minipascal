using System;

namespace minipascal
{
    class Program
    {
        static void Main(string[] args)
        {
            var scanner = new Scanner("12313.123e10");
            Console.WriteLine(scanner.Scan()[0].Content);
        }
    }
}
