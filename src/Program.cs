using System;

namespace minipascal
{
    class Program
    {
        static void Main(string[] args)
        {
            var scanner = new Scanner("program x assert 1.3e10");
            foreach (var token in scanner.Scan()) 
            {
                Console.WriteLine(token);
            }
        }
    }
}
