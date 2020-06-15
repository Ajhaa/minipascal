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
        T visitCallExpression(Expression.FunctionCall expr);
        T visitRelationExpression(Expression.Relation expr);
        
    }
    public class Relation : Expression
    {
        public TokenType Operation { get; }
        public Expression Left { get; }
        public Expression Right { get; }

        public Relation(TokenType operation, Expression left, Expression right)
        {
            Operation = operation;
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return string.Format("({0} {1} {2})", Operation, Left, Right);
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitRelationExpression(this);
        }
    }
    
    public class Addition : Expression
    {

        public TokenType Operation { get; }
        public Expression Left { get; }
        public Expression Right { get; }

        public Addition(TokenType operation, Expression left, Expression right)
        {
            Operation = operation;
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return string.Format("({0} {1} {2})", Operation, Left, Right);
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitAdditionExpression(this);
        }
    }

    public class Multiplication : Expression
    {

        public TokenType Operation { get; }
        public Expression Left { get; }
        public Expression Right { get; }

        public Multiplication(TokenType operation, Expression left, Expression right)
        {
            Operation = operation;
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return string.Format("({0} {1} {2})", Operation, Left, Right);
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visitMultiplicationExpression(this);
        }
    }

    public class Literal : Expression
    {   
        public object Value { get; }
        public string Type { get; }

        public Literal(object val, string type)
        {
            Value = val;
            Type = type;
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
        public string Identifier { get; }

        public Variable(string val)
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
        public string Identifier { get; }
        public List<Expression> Arguments { get; }

        public FunctionCall(string ident, List<Expression> arguments)
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
            return visitor.visitCallExpression(this);
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