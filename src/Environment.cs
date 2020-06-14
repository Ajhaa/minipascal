using System.Collections.Generic;

class Environment
{
    private LinkedList<List<string>> environment;

    public Environment()
    {
        environment = new LinkedList<List<string>>();
        environment.AddFirst(new List<string>());
    }

    public void EnterInner()
    {
        environment.AddFirst(new List<string>());
    }

    public int FindIndex(string elem)
    {
        // TODO check deeper
        var first = environment.First.Value.IndexOf(elem);
        if (first == -1) {
            return environment.First.Next.Value.IndexOf(elem);
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

    public int Declare(string ident)
    {   
        environment.First.Value.Add(ident);
        return environment.First.Value.IndexOf(ident);
    }
}