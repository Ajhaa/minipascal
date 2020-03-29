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
        environment.AddFirst(new List<string>(environment.First.Value));
    }

    public int FindIndex(string elem)
    {
        return environment.First.Value.IndexOf(elem);
    }

    public void ExitInner()
    {
        environment.RemoveFirst();
        if (environment.First == null)
        {
            throw new System.Exception("Exited outermost environment");
        }
    }

    public void Declare(string ident)
    {   
        environment.First.Value.Add(ident);
    }
}