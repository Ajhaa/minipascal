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
    private Dictionary<string, List<bool>> passByVar = new Dictionary<string, List<bool>>();

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
        var name = stmt.Identifier;
        var parameters = stmt.Parameters.Count;
        var returnValues = stmt.ReturnValue == null ? 0 : 1;
        current = new WASMFunction(parameters, returnValues, name);
        functions.Declare(name);

        // TODO too many nested envs?
        environment.EnterInner();
        passByVar.Add(name, new List<bool>());
        // TODO pass by
        foreach (var param in stmt.Parameters)
        {
            environment.Declare(param.Identifier);
            current.Locals++;
            passByVar[name].Add(param.IsRef);

        }

        stmt.Body.Accept(this);
        wasm.addFunction(current);

        environment.ExitInner();

        return null;
    }
    public object VisitBlockStatement(Statement.Block stmt)
    {
        // TODO handle locals in blocks
        //environment.EnterInner();

        foreach (var s in stmt.Statements)
        {
            s.Accept(this);
        }

        //environment.ExitInner();
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
            var pointer = Util.LEB128encode(memoryPointer);

            current.Locals++;
            var index = environment.Declare(ident);
            // store a placeholder 0 to the memory
            addInstruction(0x41);
            addInstruction(pointer);
            
            memoryPointer += 4;
            addInstruction(I32_CONST);
            addInstruction(Util.LEB128encode(0));

            addInstruction(0x36, 0x02, 0x00);

            addInstruction(I32_CONST);
            addInstruction(pointer);
            addInstruction(LOCAL_SET);
            addInstruction(Util.LEB128encode(index));
        }
        return null;
    }

    public object VisitArrayDeclarementStatement(Statement.ArrayDeclarement stmt)
    {
        foreach (var ident in stmt.Identifiers)
        {
            var pointer = Util.LEB128encode(memoryPointer);

            current.Locals++;
            var index = environment.Declare(ident);
            // store a placeholder 0 to the memory
            addInstruction(0x41);
            addInstruction(pointer);
            
            memoryPointer += 4 * stmt.Size;
            addInstruction(I32_CONST);
            addInstruction(Util.LEB128encode(0));

            addInstruction(0x36, 0x02, 0x00);

            addInstruction(I32_CONST);
            addInstruction(pointer);
            addInstruction(LOCAL_SET);
            addInstruction(Util.LEB128encode(index));
        }
        return null;
    }

    // TODO store pointer
    public object VisitAssignmentStatement(Statement.Assignment stmt)
    {
        var index = Util.LEB128encode(environment.FindIndex(stmt.Variable.Identifier));

        addInstruction(LOCAL_GET);
        addInstruction(index);
        if (stmt.Variable.Indexer != null)
        {
            stmt.Variable.Indexer.Accept(this);
            addInstruction(I32_CONST);
            addInstruction(4);
            addInstruction(0x6c);
            addInstruction(0x6a); // add index
        }

        stmt.Expr.Accept(this);

        addInstruction(0x36, 0x02, 0x00);

        return null;
    }
    public object VisitCallStatement(Statement.Call stmt) { 
        stmt.Expr.Accept(this);
        return null;
    }

    public object VisitIfStatement(Statement.If stmt)
    {
        stmt.Condition.Accept(this);

        addInstruction(0x04); // if
        addInstruction(0x40); // if returns void
        stmt.Then.Accept(this);
        if (stmt.Else != null)
        {
            addInstruction(0x05);
            stmt.Else.Accept(this);
        }

        addInstruction(0x0b); // end
        return null;
    }

    public object VisitReturnStatement(Statement.Return stmt)
    {
        stmt.Expr.Accept(this);
        return null;
    }
    public object VisitWriteStatement(Statement.Write stmt) { return null; }

    public object visitRelationExpression(Expression.Relation expr)
    {
        expr.Left.Accept(this);
        expr.Right.Accept(this);

        addInstruction(Util.OpToIntegerInstruction(expr.Operation));
        return null;
    }

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
        addInstruction(LOCAL_GET);
        addInstruction(Util.LEB128encode(index));
        if (expr.Indexer != null)
        {
            expr.Indexer.Accept(this);
            addInstruction(I32_CONST);
            addInstruction(4);
            addInstruction(0x6c);
            addInstruction(0x6a);     
        }
        addInstruction(0x28, 0x02, 0x00); // load from memory

        return null;
    }

    public object visitCallExpression(Expression.FunctionCall expr)
    {
        var index = functions.FindIndex(expr.Identifier);
        List<bool> outVar;
        var hasPassBy = passByVar.TryGetValue(expr.Identifier, out outVar);
        for (var i = 0; i < expr.Arguments.Count; i++)
        {
            var arg = expr.Arguments[i];

            if (hasPassBy && outVar[i])
            {
                var varIdx = environment.FindIndex(((Expression.Variable) arg).Identifier.ToString());
                addInstruction(LOCAL_GET);
                addInstruction(Util.LEB128encode(varIdx));
            } else {
                var pointer = Util.LEB128encode(memoryPointer);
                memoryPointer += 4;

                // pass by value; copy the value of the param to a new memory address

                addInstruction(I32_CONST);
                addInstruction(pointer);

                arg.Accept(this);


                addInstruction(0x36, 0x02, 0x00);
                
                addInstruction(I32_CONST);
                addInstruction(pointer);
            }
        }

        addInstruction(0x10);
        addInstruction(Util.LEB128encode(index));
        return null;
    }
}