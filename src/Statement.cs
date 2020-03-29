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
        T VisitAssignmentStatement(Statement.Assignment stmt);
        T VisitCallStatement(Statement.Call stmt);
        T VisitWriteStatement(Statement.Write stmt);
    }

    public class Function : Statement
    {
        public object Identifier { get; }
        public List<Statement.Parameter> Parameters { get; }
        public Token ReturnValue { get; }
        public Statement.Block Body { get; }

        public Function(object id, List<Statement.Parameter> parameters, Token retVal, Statement.Block body)
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
        public Token Type { get; }
        public object Identifier { get; }

        public Parameter(bool isRef, Token type, object ident)
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

        public Declarement(Token type, List<string> idents)
        {
            Type = type;
            Identifiers = idents;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitDeclarementStatement(this);
        }
    }

    public class Assignment : Statement
    {
        public string Identifier { get; }
        public Expression Expr { get; }

        public Assignment(string ident, Expression expr)
        {
            Identifier = ident;
            Expr = expr;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitAssignmentStatement(this);
        }
    }

    public class Call : Statement
    {
        public Expression.FunctionCall Expr;

        public Call(Expression.FunctionCall expr)
        {
            Expr = expr;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitCallStatement(this);
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