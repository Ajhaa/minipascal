using System;
using System.Collections.Generic;
using static TokenType;

public class Parser
{
    private List<Token> tokens;
    private int index = 0;
    private List<Statement> program = new List<Statement>();

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    private Token lookahead()
    {
        if (index + 1 < tokens.Count)
        {
            return tokens[index + 1];
        }
        return null;
    }

    private Token advance()
    {
        var current = tokens[index];
        index++;
        return current;
    }

    private Token match(TokenType t)
    {
        return match((other) => other == t);
    }

    private Token match(Func<TokenType, bool> predicate)
    {
        var current = advance();
        if (!predicate(current.Type))
        {
            throw new System.Exception(string.Format("Unexpected {0}", current));
        }

        return current;
    }

    public List<Statement> Parse()
    {
        while (index < tokens.Count)
        {
            program.Add(statement());
        }
        return program;
    }

    private Statement statement() 
    {
        match(IDENTIFIER);
        match(LEFT_PAREN);

        var stmt = new Statement.Write(expression());
        match(RIGHT_PAREN);

        return stmt;
    }

    private Expression expression()
    {
        var left = simpleExpr();
        // TODO check for realtional operator
        return left;
    }

    //TODO everythin
    private Expression simpleExpr()
    {
        return term();
    }

    private Expression term()
    {
        return factor();
    }

    private Expression factor()
    {
        var number = match(INTEGER);

        return new Expression.Factor(number.Content);
    }

}