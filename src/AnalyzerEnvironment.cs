using System.Collections.Generic;

class AnalyzerEnvironment
{
    private LinkedList<Dictionary<string, string>> environment;

    public AnalyzerEnvironment()
    {
        environment = new LinkedList<Dictionary<string, string>>();
        environment.AddFirst(new Dictionary<string, string>());
    }

    public void EnterInner()
    {
        environment.AddFirst(new Dictionary<string, string>());
    }

    public string GetType(string elem)
    {
        var first = environment.First.Value.GetValueOrDefault(elem, null);
        if (first == null) {
            var current = environment.First.Next;
            while (current != null)
            {
                var value = current.Value.GetValueOrDefault(elem, null);
                if (value != null) return value;
                current = current.Next;
            }

            return null;
        }
        return first;
    }

    public void ExitInner()
    {
        environment.RemoveFirst();
        if (environment.First == null)
        {
            throw new System.Exception("Exited outermost environment");
        }
    }

    public void Declare(string ident, string type)
    {
        environment.First.Value.Add(ident, type);
    }
}
