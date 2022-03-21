using System.Collections.Generic;
using static TokenType;

class Scanner
{
    private string input;
    private int line = 0;
    private int index = 0;
    private List<Token> tokens = new List<Token>();
    private Reserved reservedWords = new Reserved();

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
                addToken(PLUS, "+");
                break;
            case '-':
                addToken(MINUS, "-");
                break;
            case '*':
                addToken(STAR, "*");
                break;
            case '%':
                addToken(PERCENT, "%");
                break;
            case '/':
                addToken(SLASH, "/");
                break;
            case '=':
                addToken(EQUAL, "=");
                break;
            case '(':
                addToken(LEFT_PAREN, "(");
                break;
            case ')':
                addToken(RIGHT_PAREN, ")");
                break;
            case '[':
                addToken(LEFT_BRACKET, "[");
                break;
            case ']':
                addToken(RIGHT_BRACKET, "]");
                break;
            case '.':
                addToken(DOT, ".");
                break;
            case ',':
                addToken(COMMA, ",");
                break;
            case ';':
                addToken(SEMICOLON, ";");
                break;

            // two char and amiguous
            case '>':
                if (lookahead() == '=')
                {
                    index++;
                    addToken(GREATER_EQ, ">=");
                }
                else
                {
                    addToken(GREATER, ">");
                }
                break;

            case '<':
                if (lookahead() == '=')
                {
                    index++;
                    addToken(LESS_EQ, "<=");
                }
                else if (lookahead() == '>')
                {
                    index++;
                    addToken(NOT_EQUAL, "<>");
                }
                else
                {
                    addToken(LESS, "<");
                }
                break;

            case ':':
                if (lookahead() == '=')
                {
                    index++;
                    addToken(ASSIGN, ":=");
                }
                else
                {
                    addToken(COLON, ":");
                }
                break;


            case '"':
                index++;
                makeString();
                break;

            case ' ':
                break;
            case '\n':
                line++;
                break;
            default:
                if (isNumber(current))
                {
                    makeNumber();
                }

                if (isLetter(current))
                {
                    identOrKeyword();
                }
                break;
        }
    }

    // TODO lowercase somewhere
    private void identOrKeyword()
    {
        int stringStart = index;
        while (isLegalChar(lookahead()))
        {
            index++;
        }

        var result = input.Substring(stringStart, index - stringStart + 1);
        var keyword = reservedWords.StringToKeyword(result);

        if (keyword != NONE)
        {
            addToken(keyword, null);
        }
        else
        {
            addToken(IDENTIFIER, result);
        }
    }

    // TODO Check this
    public void makeString()
    {
        int stringStart = index;
        string newString = "";
        while (input[index] != '"')
        {
            if (input[index] == '\\' && input[index + 1] == 'n')
            {
                newString += '\n';
                index += 2;
                continue;
            }

            newString += input[index];
            index++;

            if (index >= input.Length)
            {
                throw new System.Exception("Unterminated string");
            }

        }
        addToken(STRING, newString);
    }

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
        if (isReal)
        {
            addToken(type, double.Parse(str));
        }
        else
        {
            addToken(type, int.Parse(str));
        }

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

    private bool isLegalChar(char c)
    {
        return isLetter(c) || isNumber(c) || c == '_';
    }

}
