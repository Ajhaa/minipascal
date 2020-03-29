using System.Collections.Generic;

public abstract class Expression
{
    public abstract T Accept<T>(Visitor<T> visitor);
    public interface Visitor<T>
    {
        T visitAdditionExpression(Expression.Addition expr);
        T visitMultiplicationExpression(Expression.Multiplication expr);
        T visitLiteralExpression(Expression.Literal expr);
        T visitVariableExpression(Expression.Variable expr);
        
    }
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
        public override T Accept<T>(Visitor<T> visitor)
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class Addition : Expression
    {

        public Token Operation { get; }
        public Expression Left { get; }
        public Expression Right { get; }

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

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitAdditionExpression(this);
        }
    }

    public class Multiplication : Expression
    {

        public Token Operation { get; }
        public Expression Left { get; }
        public Expression Right { get; }

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

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitMultiplicationExpression(this);
        }
    }

    public class Literal : Expression
    {   
        public object Value { get; }

        public Literal(object val)
        {
            Value = val;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitLiteralExpression(this);
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

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitVariableExpression(this);
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

        public override T Accept<T>(Visitor<T> visitor)
        {
            throw new System.NotImplementedException();
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

        public override T Accept<T>(Visitor<T> visitor)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Size : Expression
    {
        Expression Expr;

        public Size(Expression expr)
        {
            Expr = expr;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            throw new System.NotImplementedException();
        }
    }
}