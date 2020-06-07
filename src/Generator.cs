using System.Collections.Generic;
using System.Linq;
using System;
using static Instruction;

class Generator : Statement.Visitor<object>, Expression.Visitor<object>
{
    private List<Statement.Function> program;
    private WASM wasm = new WASM();
    private WASMFunction current;
    private Environment environment = new Environment();
    private Environment functions = new Environment();
    private int memoryPointer = 0;

    public Generator(List<Statement.Function> program)
    {
        this.program = program;
        functions.Declare("write");
    }

    public WASM Generate()
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

    private void addInstruction(params Instruction[] instructions)
    {
        current.Body.AddRange(instructions.Select(elem => (byte) elem));
    }

    private void addInstruction(IEnumerable<byte> instructions)
    {
        current.Body.AddRange(instructions);
    }

    public object VisitFunctionStatement(Statement.Function stmt)
    {
        var name = ((Token)stmt.Identifier).Content.ToString();
        var parameters = stmt.Parameters.Count;
        var returnValues = stmt.ReturnValue == null ? 0 : 1;
        current = new WASMFunction(parameters, returnValues, name);
        functions.Declare(name);

        // TODO too many nested envs?
        environment.EnterInner();
        byte i = 0;
        // TODO pass by
        foreach (var param in stmt.Parameters)
        {
            environment.Declare(param.Identifier.ToString());

            var pointer = Util.LEB128encode(memoryPointer);
            memoryPointer += 4;

            // pass by value; copy the value of the param to a new memory address
            addInstruction(I32_CONST);
            addInstruction(pointer);

            addInstruction(LOCAL_GET);
            addInstruction(i);

            addInstruction(0x36, 0x02, 0x00);

            addInstruction(I32_CONST);
            addInstruction(pointer);

            addInstruction(LOCAL_SET);
            addInstruction(i);

            i++;
        }

        stmt.Body.Accept(this);
        wasm.addFunction(current);

        environment.ExitInner();

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

    // TODO handle var param vs normal param
    public object VisitParameterStatement(Statement.Parameter stmt) { 
        return null;
    }
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
        addInstruction(LOCAL_SET);
        addInstruction(Util.LEB128encode(index)); // local index

        return null;
    }
    public object VisitCallStatement(Statement.Call stmt) { 
        stmt.Expr.Accept(this);
        return null;
    }

    public object VisitReturnStatement(Statement.Return stmt)
    {
        stmt.Expr.Accept(this);
        return null;
    }
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
        if (expr.Value is string)
        {
            // TODO handle the memory slot correctly
            wasm.addData(memoryPointer, expr.Value.ToString());
            addInstruction(0x41);
            addInstruction(Util.LEB128encode(memoryPointer));
            // addInstruction(0x41);
            // addInstruction(Util.LEB128encode(memoryPointer));
            // addInstruction(0x2d, 0x00, 0x00); 
            memoryPointer += expr.Value.ToString().Length + 1; 
            return null;
        }
        addInstruction(0x41);
        addInstruction(Util.LEB128encode((int)expr.Value));
        return null;
    }
    public object visitVariableExpression(Expression.Variable expr)
    {
        var index = environment.FindIndex(expr.Identifier.ToString());
        Console.WriteLine(index + " " + expr.Identifier);
        addInstruction(LOCAL_GET);
        addInstruction(Util.LEB128encode(index));
        addInstruction(0x28, 0x02, 0x00); // load from memory

        return null;
    }

    public object visitCallExpression(Expression.FunctionCall expr)
    {
        foreach (var arg in expr.Arguments)
        {
            arg.Accept(this);
        }

        var index = Util.LEB128encode(functions.FindIndex(expr.Identifier.ToString()));
        addInstruction(0x10);
        addInstruction(index);
        return null;
    }
}