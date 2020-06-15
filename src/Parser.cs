using System;
using System.Collections.Generic;
using static TokenType;


// TODO right semicolon positions
public class Parser
{
    private List<Token> tokens;
    private int index = 0;
    private List<Statement.Function> program = new List<Statement.Function>();
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
        return match((other) => other == t, "line {0}: expected " + t + " got {1}");
    }

    private Token match(Func<TokenType, bool> predicate, string message)
    {
        var current = advance();
        if (!predicate(current.Type))
        {
            throw new System.Exception(string.Format(message, current.Line, current));
        }

        return current;
    }

    private Token matchIf(TokenType t)
    {
        if (tokens[index].Type == t)
        {
            return advance();
        }

        return null;
    }

    public List<Statement.Function> Parse()
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
    private Statement.Function function()
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

        return new Statement.Function(id.Content.ToString(), parms, returnType, body);
    }

    private List<Statement.Parameter> parameters()
    {
        var parameters = new List<Statement.Parameter>();

        match(LEFT_PAREN);
        while(true)
        {
            if (tokens[index].Type == RIGHT_PAREN) break;
            var isRef = matchIf(VAR) != null;
            var ident = match(IDENTIFIER).Content.ToString();
            match(COLON);
            var type = match(IDENTIFIER);
            parameters.Add(new Statement.Parameter(isRef, type.Content.ToString(), ident));
            if (matchIf(COMMA) == null)
            {
                break;
            }
        }
        match(RIGHT_PAREN);
        return parameters;
    }

    private Statement statement()
    {
        var current = tokens[index];

        switch (current.Type)
        {
            case VAR:
                return varDeclarement();
            case IDENTIFIER:
                // if (current.Content.Equals("write"))
                // {
                //     return write();
                // }
                switch(lookahead().Type)
                {
                    case LEFT_PAREN:
                        return new Statement.Call(functionCall());
                    case ASSIGN:
                        return assignment();
                    default:
                        throw new Exception("wtf");
                }
            case IF:
                index++;
                return ifStatement();
            case RETURN:
                index++;
                return new Statement.Return(expression());
            case BEGIN:
                return block();
            default:
                throw new Exception("unknown stmt " + current);
        }
    }

    public Statement.If ifStatement()
    {
        var condition = expression();
        match(THEN);
        var thenStmt = statement();
        Statement elseStmt = null;
        if (matchIf(ELSE) != null)
        {
            elseStmt = statement();
        }

        return new Statement.If(condition, thenStmt, elseStmt);
    }

    // TODO identifers as strings vs objects
    private Statement.Declarement varDeclarement()
    {
        // Consume the VAR
        advance();

        var identifiers = new List<string>();
        identifiers.Add((string) match(IDENTIFIER).Content);
        
        while (tokens[index].Type == COMMA)
        {
            advance();
            identifiers.Add((string) match(IDENTIFIER).Content);
        }

        match(COLON);
        var type = match(IDENTIFIER);
        
        return new Statement.Declarement(type, identifiers);
    }

    private Statement.Assignment assignment()
    {
        var ident = (string) advance().Content;
        match(ASSIGN);
        var expr = expression();

        return new Statement.Assignment(ident, expr);
    }

    private Statement write()
    {
        match(IDENTIFIER);
        var stmt = new Statement.Write(arguments());
        match(SEMICOLON);
        return stmt;
    }

    // TODO index oob with match(DOT)
    private Statement.Function mainBlock()
    {
        // Speacial name for the main function block thingy
        var ident = new Token(IDENTIFIER, "__main__", -1);
        var parameters = new List<Statement.Parameter>();
        var body = block();
        match(DOT);
        return new Statement.Function(ident.Content.ToString(), parameters, null, body);
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
            match(SEMICOLON);
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
            return new Expression.Relation(relation.Type, left, right);
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
            return new Expression.Addition(addition.Type, left, simpleExpr());
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
            return new Expression.Multiplication(mult.Type, left, term());
        }

        return left;
    }

    // TODO implement <factor>.size
    private Expression factor()
    {
        var current = tokens[index];
        if (reserved.IsLiteral(current))
        {
            return new Expression.Literal(advance().Content, current.Type.ToString().ToLower());
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

        return new Expression.Variable(advance().Content.ToString());
    }

    private Expression.FunctionCall functionCall()
    {
        var ident = advance();
        return new Expression.FunctionCall(ident.Content.ToString(), arguments());
    }

    // TODO mayhem
    private List<Expression> arguments()
    {
        match(LEFT_PAREN);
        var arguments = new List<Expression>();
        if (tokens[index].Type == RIGHT_PAREN)
        {
            index += 1;
            return arguments;
        }

        do
        {
            var expr = expression();
            arguments.Add(expr);
            if (matchIf(RIGHT_PAREN) != null)
            {
                break;
            }
            match(COMMA);
        } while (tokens[index].Type != RIGHT_PAREN);

        return arguments;
    }
}
