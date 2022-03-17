using System;
using System.Collections.Generic;

class Analyzer : Statement.Visitor<object>, Expression.Visitor<object>
{
  Dictionary<string, string> variables = new Dictionary<string, string>();
  private Boolean returningBranch = false;

  public Analyzer(List<Statement.Function> stmts)
  {
    variables.Add("writeln", null);
    foreach (var stmt in stmts)
    {
      variables.Add(stmt.Identifier, stmt.ReturnValue);
    }
    foreach (var stmt in stmts)
    {
      stmt.Accept(this);
    }
  }
  public object VisitFunctionStatement(Statement.Function stmt)
  {
    // todo params?
    foreach (var param in stmt.Parameters)
    {
      param.Accept(this);
    }
    return stmt.Body.Accept(this);
  }
  public object VisitBlockStatement(Statement.Block stmt)
  {
    foreach (var item in stmt.Statements)
    {
      item.Accept(this);
    }
    return null;
  }

  public object VisitParameterStatement(Statement.Parameter stmt)
  {
    variables.Add(stmt.Identifier, stmt.Type);
    return null;
  }
  public object VisitDeclarementStatement(Statement.Declarement stmt)
  {
    string type = null;
    if (stmt.Assigner != null)
    {
      type = stmt.Assigner.Accept(this).ToString();
    }
    else
    {
      type = stmt.Type.Content.ToString();
    }
    foreach (var ident in stmt.Identifiers)
    {
      variables.Add(ident, type);
    }
    return null;
  }


  public object VisitArrayDeclarementStatement(Statement.ArrayDeclarement stmt)
  {
    return null;
  }

  public object VisitAssignmentStatement(Statement.Assignment stmt)
  {
    var exprType = stmt.Expr.Accept(this);
    var varType = stmt.Variable.Accept(this);
    if (exprType.ToString() != varType.ToString())
    {

      throw new Exception(string.Format("cannot assign {0} to {1}", exprType, varType));
    }
    return null;
  }
  public object VisitExpressionStatement(Statement.ExpressionStatement stmt)
  {
    stmt.Expr.Accept(this);
    return null;
  }

  public object VisitIfStatement(Statement.If stmt)
  {
    // TODO fix returningBranch spaget
    returningBranch = false;
    stmt.Then.Accept(this);
    var thenReturning = returningBranch;
    returningBranch = false;
    if (stmt.Else != null)
    {
      stmt.Else.Accept(this);
    }

    stmt.Returning = thenReturning && returningBranch;

    return null;
  }

  public object VisitWhileStatement(Statement.While stmt)
  {
    // Return analysis here?
    stmt.Condition.Accept(this);
    stmt.Body.Accept(this);
    return null;
  }

  public object VisitReturnStatement(Statement.Return stmt)
  {
    returningBranch = true;
    if (stmt.Expr == null)
    {
      return "void";
    }
    return stmt.Expr.Accept(this);
  }

  public object VisitAssertStatement(Statement.Assert stmt)
  {
    return null;
  }
  public object VisitWriteStatement(Statement.Write stmt) { return null; }

  public object visitRelationExpression(Expression.Relation expr)
  {
    return null;
  }

  // TODO plus vs minus vs OR
  public object visitAdditionExpression(Expression.Addition expr)
  {
    var left = expr.Left.Accept(this);
    var right = expr.Right.Accept(this);

    if (left.ToString() != right.ToString())
    {
      throw new Exception(string.Format("Cannot add {0} to {1}", left, right));
    }

    expr.Type = left.ToString();

    return left;
  }

  // TODO times vs divide vs AND
  public object visitMultiplicationExpression(Expression.Multiplication expr)
  {
    var left = expr.Left.Accept(this);
    var right = expr.Right.Accept(this);

    if (left.ToString() != right.ToString())
    {
      throw new Exception("Cannot multiply different types");
    }

    expr.Type = left.ToString();

    return left;
  }
  public object visitLiteralExpression(Expression.Literal expr)
  {
    return expr.Type;
  }
  public object visitVariableExpression(Expression.Variable expr)
  {
    expr.Type = variables[expr.Identifier.ToString()];

    return variables[expr.Identifier.ToString()];
  }

  public object visitCallExpression(Expression.FunctionCall expr)
  {
    expr.Type = variables[expr.Identifier.ToString()];

    foreach (var arg in expr.Arguments)
    {
      // TODO validate that the arg really is correct type
      arg.Accept(this);
    }

    return variables[expr.Identifier];
  }
}
