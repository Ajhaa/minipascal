enum TokenType 
{
    ID, LITERAL, 
    
    OR, AND, NOT, IF,
    THEN, ELSE, OF, WHILE, DO, BEGIN,
    END, VAR, ARRAY, PROCEDURE, FUNCTION,
    PROGRAM, ASSERT, RETURN,

    PLUS, MINUS, STAR, PERCENT, EQUAL,
    NOT_EQUAL, LESS, GREATER, LESS_EQ, GREATER_EQ,
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACKET, RIGHT_BRACKET,
    ASSIGN, DOT, COMMA, SEMICOLON, COLON
}

class Token
{
    TokenType Type { get; }
    object Content { get; }
    int Line { get; }

    public Token(TokenType type, object content, int line)
    {
        Type = type;
        Content = content;
        Line = line;
    }
}