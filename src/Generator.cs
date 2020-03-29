using System.Collections.Generic;
using System;

class Generator : Statement.Visitor<object>, Expression.Visitor<object>
{
    private List<Statement.Function> program;
    private List<WASMFunction> wasm = new List<WASMFunction>();
    private WASMFunction current;
    private Environment environment = new Environment();
    private int memoryPointer = 0;

    public Generator(List<Statement.Function> program)
    {
        this.program = program;
    }

    public List<WASMFunction> Generate()
    {
        foreach (var func in program)
        {
            func.Accept(this);
        }

        return wasm;
    }

    public object VisitFunctionStatement(Statement.Function stmt)
    {
        var parameters = stmt.Parameters.Count;
        var returnValues = stmt.ReturnValue == null ? 0 : 1;
        current = new WASMFunction(parameters, returnValues, ((Token)stmt.Identifier).Content.ToString());

        stmt.Body.Accept(this);
        wasm.Add(current);

        return null;
    }
    public object VisitBlockStatement(Statement.Block stmt)
    {
        environment.EnterInner();

        foreach (var s in stmt.Statements)
        {
            s.Accept(this);
        }

        environment.ExitInner();
        return null;
    }
    public object VisitParameterStatement(Statement.Parameter stmt) { return null; }
    public object VisitDeclarementStatement(Statement.Declarement stmt)
    {
        foreach (var ident in stmt.Identifiers)
        {
            current.Locals++;
            environment.Declare(ident);
        }
        return null;
    }
    public object VisitAssignmentStatement(Statement.Assignment stmt)
    {
        var index = environment.FindIndex(stmt.Identifier);
        stmt.Expr.Accept(this);
        // TODO Store result to memory pointer, store pointer to local

        return null;
    }
    public object VisitCallStatement(Statement.Call stmt) { return null; }
    public object VisitWriteStatement(Statement.Write stmt) { return null; }

    public object visitAdditionExpression(Expression.Addition expr)
    {
        expr.Right.Accept(this);
        expr.Left.Accept(this);
        current.Body.Add(0x6A);
        return null;
    }
    public object visitMultiplicationExpression(Expression.Multiplication expr)
    {
        expr.Right.Accept(this);
        expr.Left.Accept(this);
        current.Body.Add(0x6C);
        return null;
    }
    public object visitLiteralExpression(Expression.Literal expr)
    {
        System.Console.WriteLine(expr.Value);
        current.Body.Add(0x41);
        current.Body.Add(Convert.ToByte(expr.Value));
        return null;
    }
    public object visitVariableExpression(Expression.Variable expr)
    {
        return null;
    }
}