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
    private Dictionary<string, TokenType> stringToKeyword = new Dictionary<string, TokenType>();

    public Reserved() {
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
}