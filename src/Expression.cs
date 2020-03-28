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
    }

    public class Factor : Expression
    {   
        object Value { get; }

        public Factor(object val)
        {
            Value = val;
        }

        public override string ToString()
        {
            return Value.ToString();
        } 
    }
}