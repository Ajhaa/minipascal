using System.Collections.Generic;

public abstract class Statement
{
    public abstract T Accept<T>(Visitor<T> visitor);

    public interface Visitor<T>
    {
        T VisitFunctionStatement(Statement.Function stmt);
        T VisitBlockStatement(Statement.Block stmt);
        T VisitParameterStatement(Statement.Parameter stmt);
        T VisitDeclarementStatement(Statement.Declarement stmt);
        T VisitArrayDeclarementStatement(Statement.ArrayDeclarement stmt);
        T VisitAssignmentStatement(Statement.Assignment stmt);
        T VisitReturnStatement(Statement.Return stmt);
        T VisitExpressionStatement(Statement.ExpressionStatement stmt);
        T VisitWriteStatement(Statement.Write stmt);
        T VisitIfStatement(Statement.If stmt);
        T VisitWhileStatement(Statement.While stmt);
    }

    public class Function : Statement
    {
        public string Identifier { get; }
        public List<Statement.Parameter> Parameters { get; }
        public Token ReturnValue { get; }
        public Statement.Block Body { get; }

        public Function(string id, List<Statement.Parameter> parameters, Token retVal, Statement.Block body)
        {
            Identifier = id;
            Parameters = parameters;
            ReturnValue = retVal;
            Body = body;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitFunctionStatement(this);
        }
    }

    public class Block : Statement
    {
        public List<Statement> Statements { get; }

        public Block(List<Statement> stmts)
        {
            Statements = stmts;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitBlockStatement(this);
        }
    }

    public class Parameter : Statement
    {
        public bool IsRef { get; }
        public string Type { get; }
        public string Identifier { get; }

        public Parameter(bool isRef, string type, string ident)
        {
            IsRef = isRef;
            Type = type;
            Identifier = ident;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitParameterStatement(this);
        }
    }

    public class Declarement : Statement
    {
        public Token Type { get; }
        public List<string> Identifiers { get; }
        public Expression Assigner { get; }

        public Declarement(Token type, List<string> idents, Expression assigner)
        {
            Type = type;
            Identifiers = idents;
            Assigner = assigner;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitDeclarementStatement(this);
        }
    }

    public class ArrayDeclarement : Statement
    {
        public TokenType Type { get; }
        public int Size { get; }
        public List<string> Identifiers { get; }

        public ArrayDeclarement(TokenType type, int size, List<string> idents)
        {
            Type = type;
            Size = size;
            Identifiers = idents;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitArrayDeclarementStatement(this);
        }
    }

    public class Assignment : Statement
    {
        public Expression.Variable Variable { get; }
        public Expression Expr { get; }

        public Assignment(Expression.Variable variable, Expression expr)
        {
            Variable = variable;
            Expr = expr;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitAssignmentStatement(this);
        }
    }

    public class If : Statement
    {
        public Expression Condition { get; }
        public Statement Then { get; }
        public Statement Else { get; }
        public bool Returning { get; set; }

        public If(Expression condition, Statement thenStmt, Statement elseStmt)
        {
            Condition = condition;
            Then = thenStmt;
            Else = elseStmt;
            Returning = false;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
        }
    }

    public class While : Statement
    {
        public Expression Condition { get; }
        public Statement Body { get; }

        public While(Expression condition, Statement body)
        {
            Condition = condition;
            Body = body;
        } 

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitWhileStatement(this);
        }
    }

    public class Return : Statement
    {
        public Expression Expr { get; }

        public Return(Expression expr)
        {
            Expr = expr;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
        }
    }

    public class ExpressionStatement : Statement
    {
        public Expression.FunctionCall Expr;

        public ExpressionStatement(Expression.FunctionCall expr)
        {
            Expr = expr;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }

    public class Write : Statement
    {
        public List<Expression> Arguments { get; }

        public Write(List<Expression> args)
        {
            Arguments = args;
        }

        public override string ToString()
        {
            return string.Format("(writeln {0})", string.Join(',', Arguments));
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitWriteStatement(this);
        }
    }
}