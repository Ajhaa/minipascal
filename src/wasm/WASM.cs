using System;
using System.Collections.Generic;

public class WASM
{

  public List<WASMFunction> functions = new List<WASMFunction>();
  public List<Tuple<int, string>> data = new List<Tuple<int, string>>();

  public void addFunction(WASMFunction func)
  {
    functions.Add(func);
  }

  public void addData(int index, string content)
  {
    data.Add(new Tuple<int, string>(index, content));
  }

  public int Count()
  {
    return functions.Count;
  }
}