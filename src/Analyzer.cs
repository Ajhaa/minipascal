using System;
using System.Collections.Generic;

class Analyzer : Statement.Visitor<object>, Expression.Visitor<object>
{

  Dictionary<string, string> variables = new Dictionary<string, string>();

  public Analyzer(List<Statement.Function> stmts)
  {
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
    // TODO all identifiers
    foreach (var ident in stmt.Identifiers)
    {
      variables.Add(ident, stmt.Type.Content.ToString());
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
    if (exprType != varType) {

      throw new Exception(string.Format("cannot assign {0} to {1}", exprType, varType));
    }
    return null;
  }
  public object VisitCallStatement(Statement.Call stmt)
  {
    stmt.Expr.Accept(this);
    return null;
  }


  public object VisitIfStatement(Statement.If stmt)
  {
      return null;
  }

  public object VisitWhileStatement(Statement.While stmt)
  {
    return null;
  }

  public object VisitReturnStatement(Statement.Return stmt)
  {
    return stmt.Expr.Accept(this);
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

    if (left != right) {
      throw new Exception("Cannot add different types");
    }

    return left;
  }

  // TODO times vs divide vs AND
  public object visitMultiplicationExpression(Expression.Multiplication expr)
  {
    var left = expr.Left.Accept(this);
    var right = expr.Right.Accept(this);

    if (left != right) {
      throw new Exception("Cannot multiply different types");
    }

    return left;
  }
  public object visitLiteralExpression(Expression.Literal expr)
  {
    return expr.Type;
  }
  public object visitVariableExpression(Expression.Variable expr)
  {
    return variables[expr.Identifier.ToString()];
  }

  public object visitCallExpression(Expression.FunctionCall expr)
  {
    return null;
  }
}