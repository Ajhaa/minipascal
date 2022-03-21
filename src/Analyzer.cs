using System;
using System.Collections.Generic;

class Analyzer : Statement.Visitor<object>, Expression.Visitor<object>
{
  AnalyzerEnvironment variables = new AnalyzerEnvironment();
  private Boolean returningBranch = false;

  public Analyzer(List<Statement.Function> stmts)
  {
    variables.Declare("writeln", null);
    variables.Declare("read", null);

    foreach (var stmt in stmts)
    {
      variables.Declare(stmt.Identifier, stmt.ReturnValue);
    }
    foreach (var stmt in stmts)
    {
      stmt.Accept(this);
    }
  }
  public object VisitFunctionStatement(Statement.Function stmt)
  {
    // todo params?
    variables.EnterInner();
    foreach (var param in stmt.Parameters)
    {
      param.Accept(this);
    }
    stmt.Body.Accept(this);
    variables.ExitInner();
    return null;
  }
  public object VisitBlockStatement(Statement.Block stmt)
  {
    variables.EnterInner();
    foreach (var item in stmt.Statements)
    {
      item.Accept(this);
    }
    variables.ExitInner();
    return null;
  }

  public object VisitParameterStatement(Statement.Parameter stmt)
  {
    variables.Declare(stmt.Identifier, stmt.Type);
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
      variables.Declare(ident, type);
    }
    return null;
  }


  public object VisitArrayDeclarementStatement(Statement.ArrayDeclarement stmt)
  {
    var type = stmt.Type.Content.ToString();
    foreach (var ident in stmt.Identifiers)
    {
      variables.Declare(ident, type);
    }

    var sizeType = stmt.Size.Accept(this);

    if (sizeType.ToString() != "integer")
    {
      // TODO line number
      throw new Exception($"Array size must be an integer");
    }

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
    stmt.Expr.Accept(this);
    return null;
  }
  // public object VisitWriteStatement(Statement.Write stmt) { return null; }

  public object visitRelationExpression(Expression.Relation expr)
  {
    var left = expr.Left.Accept(this);
    var right = expr.Right.Accept(this);

    if (left.ToString() != right.ToString())
    {
      throw new Exception(string.Format("Cannot compare {0} to {1}", left, right));
    }

    expr.Type = left.ToString();

    return left;
  }

  // TODO plus vs minus vs OR
  public object visitAdditionExpression(Expression.Addition expr)
  {
    Console.WriteLine("ENtering addition expression");
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
    if (expr.Indexer != null)
    {
      var indexerType = expr.Indexer.Accept(this);
      if (indexerType.ToString() != "integer")
      {
        throw new Exception("Indexer must be an integer");
      }
    }
    expr.Type = variables.GetType(expr.Identifier.ToString());

    return expr.Type;
  }

  public object visitCallExpression(Expression.FunctionCall expr)
  {
    expr.Type = variables.GetType(expr.Identifier.ToString());

    foreach (var arg in expr.Arguments)
    {
      // TODO validate that the arg really is correct type
      arg.Accept(this);
    }

    return expr.Type;
  }


  // TODO only accept Array.size
    object Expression.Visitor<object>.visitSizeExpression(Expression.Size expr)
    {
        expr.Expr.Accept(this);
        expr.Type = "integer";
        return "integer";
    }
}
