public enum TokenType
{
    IDENTIFIER, INTEGER, REAL, STRING,

    OR, AND, NOT, IF,
    THEN, ELSE, OF, WHILE, DO, BEGIN,
    END, VAR, ARRAY, PROCEDURE, FUNCTION,
    PROGRAM, ASSERT, RETURN,

    PLUS, MINUS, STAR, PERCENT, SLASH, EQUAL,
    NOT_EQUAL, LESS, GREATER, LESS_EQ, GREATER_EQ,
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACKET, RIGHT_BRACKET,
    ASSIGN, DOT, COMMA, SEMICOLON, COLON,

    NONE // hack, a 'null value' from TokenType
}

public class Token
{
    public TokenType Type { get; }
    public object Content { get; }
    public int Line { get; }

    public Token(TokenType type, object content, int line)
    {
        Type = type;
        Content = content;
        Line = line;
    }

    public override string ToString()
    {
        if (Content == null) return Type.ToString();
        return string.Format("{0}:{1}", Type, Content);
    }

    public string toMinipascal()
    {
        if (Type == TokenType.STRING)
        {
            return $"\\\"{Content.ToString()}\\\"";
        }
        return Content.ToString();
    }
}
