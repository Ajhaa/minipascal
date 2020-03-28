using System.Collections.Generic;

public class Expression
{
    public class Relation : Expression
    {
        Token Operation { get; }
        Expression Left { get; }
        Expression Right { get; }

        public Relation(Token operation, Expression left, Expression right)
        {
            Operation = operation;
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return string.Format("({0} {1} {2})", Operation.Type, Left, Right);
        }
    }
    
    public class Addition : Expression
    {

        Token Operation { get; }
        Expression Left { get; }
        Expression Right { get; }

        public Addition(Token operation, Expression left, Expression right)
        {
            Operation = operation;
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return string.Format("({0} {1} {2})", Operation.Type, Left, Right);
        }
    }

    public class Multiplication : Expression
    {

        Token Operation { get; }
        Expression Left { get; }
        Expression Right { get; }

        public Multiplication(Token operation, Expression left, Expression right)
        {
            Operation = operation;
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return string.Format("({0} {1} {2})", Operation.Type, Left, Right);
        }
    }

    public class Literal : Expression
    {   
        object Value { get; }

        public Literal(object val)
        {
            Value = val;
        }

        public override string ToString()
        {
            return Value.ToString();
        } 
    }

    public class Variable : Expression
    {   
        object Identifier { get; }

        public Variable(object val)
        {
            Identifier = val;
        }

        public override string ToString()
        {
            return Identifier.ToString();
        } 
    }

    public class FunctionCall : Expression
    {
        object Identifier { get; }
        List<Expression> Arguments { get; }

        public FunctionCall(object ident, List<Expression> arguments)
        {
            Identifier = ident;
            Arguments = arguments;
        }

        public override string ToString()
        {
            return string.Format("(call {0} ({1}))", Identifier, string.Join(',', Arguments));
        }
    }

    public class Unary : Expression
    {
        Token Operation;
        Expression Expr;

        public Unary(Token op, Expression expr)
        {
            Operation = op;
            Expr = expr;
        }
    }

    public class Size : Expression
    {
        Expression Expr;

        public Size(Expression expr)
        {
            Expr = expr;
        }
    }
}