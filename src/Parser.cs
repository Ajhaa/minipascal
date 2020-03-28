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
        return match((other) => other == t, "expected " + t + " got {0}");
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
        match(PROGRAM);
        // TODO use the program name 
        match(IDENTIFIER);
        match(SEMICOLON);
        while (index < tokens.Count)
        {
            program.Add(function());
        }
        return program;
    }

    // TODO error if not func or proc
    private Statement function()
    {
        var type = tokens[index].Type;

        if (type == BEGIN)
        {
            return mainBlock();
        }

        advance();
        var id = match(IDENTIFIER);
        var parms = parameters();
        Token returnType = null;

        if (type == FUNCTION)
        {
            match(COLON);
            returnType = match(IDENTIFIER);
        }
        match(SEMICOLON);

        var body = block();
        match(SEMICOLON);

        return new Statement.Function(id, parms, returnType, body);
    }

    private List<Statement.Parameter> parameters()
    {
        match(LEFT_PAREN);
        match(RIGHT_PAREN);
        var parameters = new List<Statement.Parameter>();
        return parameters;
    }

    private Statement statement()
    {
        match(IDENTIFIER);
        var stmt = new Statement.Write(arguments());
        match(SEMICOLON);
        return stmt;
    }

    // TODO index oob with match(DOT)
    private Statement mainBlock()
    {
        // Speacial nameless ident for the main function
        var ident = new Token(IDENTIFIER, null, -1);
        var parameters = new List<Statement.Parameter>();
        var body = block();
        match(DOT);
        return new Statement.Function(ident, parameters, null, body);
    }

    private Statement.Block block()
    {
        var body = new List<Statement>();
        match(BEGIN);
        if (lookahead().Type == END)
        {
            index += 2;
            return new Statement.Block(body);
        }

        do
        {
            var stmt = statement();
            body.Add(stmt);
        } while (tokens[index].Type != END);

        advance();
        return new Statement.Block(body);
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
        return new Expression.FunctionCall(ident.Content, arguments());
    }

    // TODO mayhem
    private List<Expression> arguments()
    {
        match(LEFT_PAREN);
        var arguments = new List<Expression>();
        if (lookahead().Type == RIGHT_PAREN)
        {
            index += 2;
            return arguments;
        }

        do
        {
            var expr = expression();
            arguments.Add(expr);
        } while (tokens[index].Type != RIGHT_PAREN);
        match(RIGHT_PAREN);

        return arguments;
    }

}