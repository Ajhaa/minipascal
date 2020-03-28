using System.Collections.Generic;

public class Statement
{
    public interface Visitor<T>
    {
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
    }

    public class Block : Statement
    {
        public List<Statement> Statements { get; }

        public Block(List<Statement> stmts)
        {
            Statements = stmts;
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
    }

    // TODO multiple expr
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
    }
}