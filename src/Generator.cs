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

    private void addInstruction(params byte[] instructions)
    {
        current.Body.AddRange(instructions);
    }

    private void addInstruction(IEnumerable<byte> instructions)
    {
        current.Body.AddRange(instructions);
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

    // TODO store pointer
    public object VisitAssignmentStatement(Statement.Assignment stmt)
    {
        var pointer = Util.LEB128encode(memoryPointer);
        var index = environment.FindIndex(stmt.Identifier);
        
        // store a integer to memory at pointer
        addInstruction(0x41);
        addInstruction(pointer);
        
        memoryPointer += 4;
        stmt.Expr.Accept(this);
        // the store instruction and its immediates
        addInstruction(0x36, 0x02, 0x00);

        // store memory address into local variable
        addInstruction(0x41); // constant
        addInstruction(pointer); // value of the constant
        addInstruction(0x21); // setlocal
        addInstruction(Util.LEB128encode(index)); // local index

        return null;
    }
    public object VisitCallStatement(Statement.Call stmt) { return null; }
    public object VisitWriteStatement(Statement.Write stmt) { return null; }

    // TODO plus vs minus vs OR
    public object visitAdditionExpression(Expression.Addition expr)
    {
        expr.Left.Accept(this);
        expr.Right.Accept(this);

        var op = Util.OpToIntegerInstruction(expr.Operation);
        current.Body.Add(op);
        return null;
    }

    // TODO times vs divide vs AND
    public object visitMultiplicationExpression(Expression.Multiplication expr)
    {
        expr.Left.Accept(this);
        expr.Right.Accept(this);

        var op = Util.OpToIntegerInstruction(expr.Operation);
        current.Body.Add(op);
        return null;
    }
    public object visitLiteralExpression(Expression.Literal expr)
    {
        addInstruction(0x41);
        addInstruction(Util.LEB128encode((int)expr.Value));
        return null;
    }
    public object visitVariableExpression(Expression.Variable expr)
    {
        var index = environment.FindIndex(expr.Identifier.ToString());
        addInstruction(0x20); // get local
        addInstruction(Util.LEB128encode(index));
        addInstruction(0x28, 0x02, 0x00); // load from memory

        return null;
    }
}