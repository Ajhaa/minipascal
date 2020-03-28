using System;
using System.Collections.Generic;
using static TokenType;

public class Parser
{
    private List<Token> tokens;
    private int index = 0;
    private List<Statement> program = new List<Statement>();
    private Reserved reserved = new Reserved();

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
        return match((other) => other == t, "expected " + t  + " got {0}");
    }

    private Token match(Func<TokenType, bool> predicate, string message)
    {
        var current = advance();
        if (!predicate(current.Type))
        {
            throw new System.Exception(string.Format(message, current));
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
        if (reserved.IsRelation(tokens[index]))
        {
            var relation = tokens[index];
            index++;

            var right = simpleExpr();
            return new Expression.Relation(relation, left, right);
        }
        
        return left;
    }

    //TODO sign
    private Expression simpleExpr()
    {
        var left = term();
        if (reserved.IsAddition(tokens[index]))
        {
            var addition = tokens[index];
            index++;
            return new Expression.Addition(addition, left, simpleExpr());
        }

        return left;
    }

    private Expression term()
    {
        var left = factor();
        if (reserved.IsMultiplication(tokens[index]))
        {
            var mult = tokens[index];
            index++;
            return new Expression.Multiplication(mult, left, term());
        }

        return left;
    }

    // TODO implement <factor>.size
    private Expression factor()
    {
        var current = tokens[index];
        if (reserved.IsLiteral(current))
        {
            return new Expression.Literal(advance().Content);
        }
        if (current.Type == IDENTIFIER)
        {
            return handleIdentifier(current);
        }
        if (current.Type == LEFT_PAREN)
        {
            advance();
            var expr = expression();
            match(RIGHT_PAREN);
            return expr;
        }
        if (current.Type == NOT)
        {
            return new Expression.Unary(current, factor());
        }

        throw new System.Exception("Invalid factor");
    }

    private Expression handleIdentifier(Token current)
    {
        if (lookahead().Type == LEFT_PAREN)
        {
            return functionCall();
        }
        
        return new Expression.Variable(advance().Content);
    }

    private Expression functionCall() 
    {
        var ident = advance();
        return new Expression.FunctionCall(ident.Content, parameters());
    }

    private List<Expression> parameters()
    {
        match(LEFT_PAREN);
        var parameters = new List<Expression>();
        while (lookahead().Type != RIGHT_PAREN)
        {
            parameters.Add(expression());
            match(COMMA);
        }
        parameters.Add(expression());
        match(RIGHT_PAREN);

        return parameters;
    }

}