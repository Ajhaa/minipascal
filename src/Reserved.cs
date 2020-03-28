using System.Collections.Generic;
using static TokenType;
class Reserved
{
    private static TokenType[] keywords = new TokenType[] {
        OR, AND, NOT, IF,
        THEN, ELSE, OF, WHILE, DO, BEGIN,
        END, VAR, ARRAY, PROCEDURE, FUNCTION,
        PROGRAM, ASSERT, RETURN,
    };

    private static HashSet<TokenType> relations = new HashSet<TokenType>() {
        EQUAL, NOT_EQUAL, LESS, GREATER, LESS_EQ, GREATER_EQ
    };

    private static HashSet<TokenType> multiplications = new HashSet<TokenType>() {
        STAR, SLASH, PERCENT, AND
    };

    private Dictionary<string, TokenType> stringToKeyword = new Dictionary<string, TokenType>();

    public Reserved()
    {
        foreach (var token in keywords)
        {
            var strRepr = token.ToString().ToLower();
            stringToKeyword[strRepr] = token;
        }
    }

    public TokenType StringToKeyword(string str)
    {
        return stringToKeyword.GetValueOrDefault(str, NONE);
    }

    public bool IsRelation(Token t)
    {
        return relations.Contains(t.Type);
    }

    public bool IsAddition(Token t)
    {
        return t.Type == PLUS || t.Type == MINUS || t.Type == OR;
    }

    public bool IsMultiplication(Token t)
    {
        return multiplications.Contains(t.Type);
    }

    public bool IsLiteral(Token t)
    {
        return t.Type == INTEGER || t.Type == REAL || t.Type == STRING;
    }
}