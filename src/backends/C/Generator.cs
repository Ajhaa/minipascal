
using System.Collections.Generic;
using System.Linq;
using System;

namespace C
{
    class Generator : Statement.Visitor<object>, Expression.Visitor<string>
    {
        private List<Statement.Function> program;
        private CProgram cProgram = new CProgram();
        private CFunction current;
        private Environment environment = new Environment();
        private Environment functions = new Environment();
        // private int memoryPointer = 0;
        private Dictionary<string, List<bool>> passByVar = new Dictionary<string, List<bool>>();
        private int tempVariableIndex = 0;

        public Generator(List<Statement.Function> program)
        {
            this.program = program;
            functions.Declare("writeln");
        }

        public CProgram Generate()
        {
            foreach (var func in program)
            {
                func.Accept(this);
            }

            return cProgram;
        }

        private void addInstruction(params string[] instructions)
        {
            current.Body.AddRange(instructions);
        }

        private void addInstruction(IEnumerable<string> instructions)
        {
            current.Body.AddRange(instructions);
        }

        public object VisitFunctionStatement(Statement.Function stmt)
        {
            var name = stmt.Identifier;
            var returnType = Util.TypeToCType(stmt.ReturnValue);
            current = new CFunction(returnType, name);
            functions.Declare(name);

            // TODO too many nested envs?
            environment.EnterInner();
            passByVar.Add(name, new List<bool>());
            // TODO pass by
            foreach (var param in stmt.Parameters)
            {
                environment.Declare(param.Identifier);
                current.Locals++;
                current.AddParam(param.Identifier, param.Type);
                passByVar[name].Add(param.IsRef);

            }

            stmt.Body.Accept(this);
            cProgram.addFunction(current);

            environment.ExitInner();

            return null;
        }

        object Statement.Visitor<object>.VisitBlockStatement(Statement.Block stmt)
        {
            // TODO brackets to c ?
            //environment.EnterInner();

            foreach (var s in stmt.Statements)
            {
                s.Accept(this);
            }

            //environment.ExitInner();
            return null;
        }

        object Statement.Visitor<object>.VisitParameterStatement(Statement.Parameter stmt)
        {
            throw new NotImplementedException();
        }

        object Statement.Visitor<object>.VisitDeclarementStatement(Statement.Declarement stmt)
        {
            foreach (var ident in stmt.Identifiers)
            {
                current.Locals++;
                var index = environment.Declare(ident);
                // if declarement has assignment, evaluate it
                var type = Util.TypeToCType(stmt.Type);

                if (stmt.Assigner != null)
                {
                    // string tempVariable = "__temp_" + tempVariableIndex++;
                    // // TODO type
                    // addInstruction("int " + tempVariable + ";");
                    //tempVariableStack.Push(tempVariable);
                    var result_temp = stmt.Assigner.Accept(this);

                    addInstruction(type +" "+ ident + " = " + result_temp);
                } else {
                    addInstruction(type + " " + ident);
                }

                // addInstruction(I32_CONST);
                // addInstruction(pointer);
                // addInstruction(LOCAL_SET);
                // addInstruction(Util.LEB128encode(index));
            }
            return null;
        }

        object Statement.Visitor<object>.VisitArrayDeclarementStatement(Statement.ArrayDeclarement stmt)
        {
            throw new NotImplementedException();
        }

        object Statement.Visitor<object>.VisitAssignmentStatement(Statement.Assignment stmt)
        {
            var result_temp = stmt.Expr.Accept(this);
            // TODO indexing
            addInstruction(stmt.Variable.Identifier + " = " + result_temp);
            return null;
        }

        object Statement.Visitor<object>.VisitReturnStatement(Statement.Return stmt)
        {
            if (stmt.Expr == null)
            {
                addInstruction("return");
                return null;
            }
            var res = stmt.Expr.Accept(this);
            addInstruction("return " + res);
            return null;
        }

        object Statement.Visitor<object>.VisitExpressionStatement(Statement.ExpressionStatement stmt)
        {
            var result = stmt.Expr.Accept(this);
            addInstruction(result);
            return null;
        }

        object Statement.Visitor<object>.VisitWriteStatement(Statement.Write stmt)
        {
            throw new NotImplementedException();
        }

        object Statement.Visitor<object>.VisitIfStatement(Statement.If stmt)
        {
            throw new NotImplementedException();
        }

        object Statement.Visitor<object>.VisitWhileStatement(Statement.While stmt)
        {
            throw new NotImplementedException();
        }

        object Statement.Visitor<object>.VisitAssertStatement(Statement.Assert stmt)
        {
            throw new NotImplementedException();
        }

        string Expression.Visitor<string>.visitAdditionExpression(Expression.Addition expr)
        {
            string tempVariable = "__temp_" + tempVariableIndex++;

            var rTemp1 = expr.Left.Accept(this);
            var rTemp2 = expr.Right.Accept(this);

            addInstruction("int " + tempVariable + " = " + rTemp1 + Util.OpToC(expr.Operation) + rTemp2);
            return tempVariable;
        }


        string Expression.Visitor<string>.visitMultiplicationExpression(Expression.Multiplication expr)
        {
            string tempVariable = "__temp_" + tempVariableIndex++;

            var rTemp1 = expr.Left.Accept(this);
            var rTemp2 = expr.Right.Accept(this);

            addInstruction("int " + tempVariable + " = " + rTemp1 + Util.OpToC(expr.Operation) + rTemp2);
            return tempVariable;
        }

        string Expression.Visitor<string>.visitLiteralExpression(Expression.Literal expr)
        {
            //string tempVariable = "__temp_" + tempVariableIndex++;
            //addInstruction("int " + tempVariable + "=" + expr.Value);
            if (expr.Type == "string")
            {
                return string.Format("\"{0}\"", expr.Value.ToString());
            }
            return expr.Value.ToString();
        }

        string Expression.Visitor<string>.visitVariableExpression(Expression.Variable expr)
        {
            // TODO arrayThing??
            // TODO handle the pass as ref case var*
            return expr.Identifier;
        }

        string Expression.Visitor<string>.visitCallExpression(Expression.FunctionCall expr)
        {
            if (expr.Identifier == "writeln")
            {
                // TODO multiple params
                var format = "%d";
                if (expr.Arguments[0].Type == "string") {
                    format = "%s";
                } else if (expr.Arguments[0].Type == "Boolean") {
                    format = "%s";
                }

                var target = expr.Arguments[0].Accept(this);

                return string.Format("printf(\"{0}\\n\", {1})", format, target);
            } else {
                var args = new List<string>();
                // TODO pass by ref
                foreach (var arg in expr.Arguments)
                {
                    args.Add(arg.Accept(this));
                }

                return string.Format("{0}({1})", expr.Identifier, string.Join(",", args));
            }
            throw new NotImplementedException();
        }

        string Expression.Visitor<string>.visitRelationExpression(Expression.Relation expr)
        {
            throw new NotImplementedException();
        }
    }
}
