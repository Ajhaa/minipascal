public class Statement
{
    public interface Visitor<T>
    {
        T VisitWriteStatement(Statement.Write stmt);
    }

    // TODO multiple expr
    public class Write : Statement
    {
        public Expression Content { get; }

        public Write(Expression content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return string.Format("(writeln {0})", Content);
        }
    }
}