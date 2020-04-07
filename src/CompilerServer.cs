using System.Net;
using System.Threading.Tasks;
using System.IO;
using System;
class CompilerServer
{

    public void Run(string addr)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(addr);
        listener.Start();
        Console.WriteLine("Listening");

        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            var body = request.InputStream;
            var encoding = request.ContentEncoding;
            var reader = new System.IO.StreamReader(body, encoding);
            
            var code = reader.ReadToEnd();
            Console.WriteLine("JEE");
            Console.WriteLine(code);

            var tokens = new Scanner(code).Scan();

            var parser = new Parser(tokens);
            var program = parser.Parse();

            var wasm = new Generator(program).Generate();
            var binary = new WASMwriter(wasm).Write();

            // Obtain a response object.
            HttpListenerResponse response = context.Response;

            // Construct a response.
            byte[] buffer = binary.ToArray();
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            response.AppendHeader("Access-Control-Allow-Origin", "*");
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
        }
    }
}