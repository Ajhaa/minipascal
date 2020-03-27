using System.Collections.Generic;
using static TokenType;

class Scanner
{
    private string input;
    private int line = 0;
    private int index = 0;
    private List<Token> tokens = new List<Token>();

    public Scanner(string input)
    {
        this.input = input;
    }

    public List<Token> Scan()
    {
        while (index < input.Length)
        {
            scanToken();
            index++;
        }

        return tokens;
    }

    private void addToken(TokenType type, object content)
    {
        tokens.Add(new Token(type, content, line));
    }

    private char lookahead()
    {
        return input[index + 1];
    }

    private void scanToken()
    {
        char current = input[index];

        switch (current)
        {
            // Simple one char
            case '+':
                addToken(PLUS, null);
                break;
            case '-':
                addToken(MINUS, null);
                break;
            case '*':
                addToken(STAR, null);
                break;
            case '%':
                addToken(PERCENT, null);
                break;
            case '=':
                addToken(EQUAL, null);
                break;
            case '(':
                addToken(LEFT_PAREN, null);
                break;
            case ')':
                addToken(RIGHT_PAREN, null);
                break;
            case '[':
                addToken(LEFT_BRACKET, null);
                break;
            case ']':
                addToken(RIGHT_BRACKET, null);
                break;
            case '.':
                addToken(DOT, null);
                break;
            case ',':
                addToken(COMMA, null);
                break;
            case ';':
                addToken(SEMICOLON, null);
                break;

            // two char and amiguous
            case '>':
                if (lookahead() == '=')
                {
                    index++;
                    addToken(GREATER_EQ, null);
                }
                else
                {
                    addToken(GREATER, null);
                }
                break;

            case '<':
                if (lookahead() == '=')
                {
                    index++;
                    addToken(LESS_EQ, null);
                }
                else if (lookahead() == '>')
                {
                    index++;
                    addToken(NOT_EQUAL, null);
                }
                else
                {
                    addToken(LESS, null);
                }
                break;

            case ':':
                if (lookahead() == '=')
                {
                    index++;
                    addToken(ASSIGN, null);
                }
                else
                {
                    addToken(COLON, null);
                }
                break;


            case '"':
                index++;
                makeString();
                break;

            case ' ':
                break;
            default:
                if (isNumber(current))
                {
                    makeNumber();
                }
                break;
        }
    }
    // TODO multiLine?
    public void makeString()
    {
        int start = index;

        while (lookahead() != '"')
        {
            index++;
        }

        var str = input.Substring(start, index + start - 1);
        addToken(STRING, str);
    }

    // TODO e part of real
    private void makeNumber()
    {
        int numberStart = index;
        bool isReal = false;
        bool isExponent = false;

        while (true)
        {
            if (index >= input.Length - 1) break;

            var next = lookahead();
            // TODO care about letters next to numbers?

            if (!isNumber(next))
            {
                if (next == '.')
                {
                    if (isReal)
                    {
                        throw new System.Exception("'.' in wrong spot in float");
                    }
                    isReal = true;
                }
                else if (next == 'e') 
                {
                    if (isExponent)
                    {
                        throw new System.Exception("E");
                    }
                    isExponent = isReal = true;
                }
                else
                {
                    break;
                }
            }
            index++;
        }
        var str = input.Substring(numberStart, index - numberStart + 1);
        var type = isReal ? REAL : INTEGER;

        // TODO actual real??
        var value = isReal ? double.Parse(str) : int.Parse(str);

        addToken(type, value);
    }

    private bool isLetter(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z');
    }

    private bool isNumber(char c)
    {
        return (c >= '0' && c <= '9');
    }

}