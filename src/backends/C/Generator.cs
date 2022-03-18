
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
            functions.Declare("read");
        }

        public CProgram Generate()
        {
            foreach (var func in program)
            {
                func.Accept(this);
            }

            return cProgram;
        }

        private void addStatement(params string[] stmts)
        {
            foreach (var stmt in stmts)
            {
                current.Body.Add(stmt + ";");
            }
        }

        private void addLine(params string[] lines)
        {
            current.Body.AddRange(lines);
        }

        private string getTempVar()
        {
            return "__temp_" + tempVariableIndex++;
        }

        private string getTempVar(string template)
        {
            return template + tempVariableIndex++;
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
                environment.Declare(ident);
                // if declarement has assignment, evaluate it

                if (stmt.Assigner != null)
                {
                    var type = Util.TypeToCType(stmt.Assigner.Type);
                    var result_temp = stmt.Assigner.Accept(this);

                    addStatement(type + " " + ident + " = " + result_temp);
                }
                else
                {
                    var type = Util.TypeToCType(stmt.Type);
                    addStatement(type + " " + ident);
                }
            }
            return null;
        }

        object Statement.Visitor<object>.VisitArrayDeclarementStatement(Statement.ArrayDeclarement stmt)
        {
            foreach (var ident in stmt.Identifiers)
            {
                environment.Declare(ident);

                var size = stmt.Size;
                var type = Util.TypeToCType(stmt.Type);
                var stmtType = stmt.Type.Content.ToString();
                var temp = getTempVar();
                addStatement(
                    $"{stmtType}Array* {ident} = malloc(sizeof({stmtType}Array))",
                    $"{type}* {temp} = malloc(sizeof({type}) * {size})",
                    $"{ident}->size = {size}",
                    $"{ident}->content = {temp}"
                );

            }
            return null;
        }

        object Statement.Visitor<object>.VisitAssignmentStatement(Statement.Assignment stmt)
        {
            var result_temp = stmt.Expr.Accept(this);
            var target = stmt.Variable.Accept(this);
            addStatement(target + " = " + result_temp);
            return null;
        }

        object Statement.Visitor<object>.VisitReturnStatement(Statement.Return stmt)
        {
            if (stmt.Expr == null)
            {
                addStatement("return");
                return null;
            }
            var res = stmt.Expr.Accept(this);
            addStatement("return " + res);
            return null;
        }

        object Statement.Visitor<object>.VisitExpressionStatement(Statement.ExpressionStatement stmt)
        {
            var result = stmt.Expr.Accept(this);
            if (result == null)
            {
                return null;
            }
            addStatement(result);
            return null;
        }

        object Statement.Visitor<object>.VisitWriteStatement(Statement.Write stmt)
        {
            throw new NotImplementedException();
        }

        //TODO lables and others without ;
        object Statement.Visitor<object>.VisitIfStatement(Statement.If stmt)
        {
            var cond = stmt.Condition.Accept(this);
            var elseLabel = "label_" + tempVariableIndex++;
            var endLabel = "label_" + tempVariableIndex++;

            addStatement(string.Format("if ({0} == 0) goto {1}", cond, elseLabel));
            stmt.Then.Accept(this);
            addStatement("goto " + endLabel);

            addStatement(elseLabel + ":");
            if (stmt.Else != null)
            {
                stmt.Else.Accept(this);
            }

            addStatement(endLabel + ":");
            return null;
        }

        object Statement.Visitor<object>.VisitWhileStatement(Statement.While stmt)
        {
            var startLabel = "label_" + tempVariableIndex++;
            var endLabel = "label_" + tempVariableIndex++;
            addStatement(startLabel + ":");
            var cond = stmt.Condition.Accept(this);

            addStatement(string.Format("if ({0} == 0) goto {1}", cond, endLabel));
            stmt.Body.Accept(this);
            addStatement("goto " + startLabel);

            addStatement(endLabel + ":");
            return null;
        }

        object Statement.Visitor<object>.VisitAssertStatement(Statement.Assert stmt)
        {
            var res = stmt.Expr.Accept(this);
            addStatement(string.Format("assert({0} != 0)", res));
            return null;
        }

        string Expression.Visitor<string>.visitAdditionExpression(Expression.Addition expr)
        {
            string tempVariable = "__temp_" + tempVariableIndex++;

            var rTemp1 = expr.Left.Accept(this);
            var rTemp2 = expr.Right.Accept(this);

            if (expr.Type == "integer")
            {
                addStatement(
                    string.Format("int {0} = {1} {2} {3}", tempVariable, rTemp1, Util.OpToC(expr.Operation), rTemp2)
                );
            }
            else if (expr.Type == "string")
            {
                // TODO handle string + integer
                var lenTemp = "__temp_" + tempVariableIndex++;
                addStatement(
                    string.Format("size_t {0} = strlen({1}) + strlen({2})", lenTemp, rTemp1, rTemp2),
                    string.Format("{0} = {0} + 1", lenTemp),
                    string.Format("char* {0} = malloc({1})", tempVariable, lenTemp),
                    string.Format("strcat({0},{1})", tempVariable, rTemp1, rTemp2),
                    string.Format("strcat({0},{2})", tempVariable, rTemp1, rTemp2)
                );
            }
            else
            {
                throw new Exception("Addition expression wrong type " + expr.Type);
            }
            return tempVariable;
        }


        string Expression.Visitor<string>.visitMultiplicationExpression(Expression.Multiplication expr)
        {
            string tempVariable = "__temp_" + tempVariableIndex++;

            var rTemp1 = expr.Left.Accept(this);
            var rTemp2 = expr.Right.Accept(this);

            addStatement("int " + tempVariable + " = " + rTemp1 + Util.OpToC(expr.Operation) + rTemp2);
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
            // TODO index out of bounds check
            if (expr.Indexer != null)
            {
                var indexer = expr.Indexer.Accept(this);
                return string.Format("{0}->content[{1}]", expr.Identifier, indexer);
            }
            // TODO handle the pass as ref case var*
            return expr.Identifier;
        }

        string Expression.Visitor<string>.visitCallExpression(Expression.FunctionCall expr)
        {
            if (expr.Identifier == "writeln")
            {
                foreach (var arg in expr.Arguments)
                {
                    generateWriteLn(arg);
                }
                addStatement(
                    "printf(\"\\n\")"
                );
                return null;
            }
            else if (expr.Identifier == "read")
            {
                foreach (var arg in expr.Arguments)
                {
                    generateRead(arg);
                }
                return null;
            }
            else
            {
                var temp = getTempVar();
                var args = new List<string>();
                // TODO pass by ref
                foreach (var arg in expr.Arguments)
                {
                    args.Add(arg.Accept(this));
                }

                var type = Util.TypeToCType(expr.Type);
                var argString = string.Join(",", args);
                if (type == "void")
                {
                    addStatement($"{expr.Identifier}({argString})");
                    return null;
                }
                addStatement($"{type} {temp} = {expr.Identifier}({argString})");
                return temp;
            }
        }

        string generateWriteLn(Expression argument)
        {
            var type = argument.Type;
            var format = "%d";
            switch (type)
            {
                case "string":
                    format = "%s";
                    break;

                case "Boolean":
                    format = "%b";
                    break;

                case "real":
                    format = "%f";
                    break;
            }

            var target = argument.Accept(this);

            addStatement($"printf(\"{format}\", {target})");
            return null;
        }

        string generateRead(Expression argument)
        {

            var readVar = getTempVar("__read_var_");
            var readSize = getTempVar("__read_size_");

            var param1 = argument.Accept(this);

            addStatement(
                string.Format("char* {0} = malloc(50)", readVar),
                string.Format("size_t {0} = 50", readSize),
                string.Format("getline(&{0},&{1},stdin)", readVar, readSize)
            );


            // TODO check that params are variables

            var exprType = argument.Type;

            if (exprType == "string")
            {
                addStatement(
                    string.Format("{0}[strlen({0})-1] = 0", readVar),
                    string.Format("{0} = {1}", param1, readVar)
                );
            }
            else if (exprType == "integer")
            {
                addStatement(
                    string.Format("{0} = atoi({1})", param1, readVar)
                );
            }
            else if (exprType == "real")
            {
                addStatement(
                    $"{param1} = strtod({readVar}, 0)"
                );
            }
            else
            {
                throw new Exception("Not supported read type " + exprType);
            }

            return readVar;
        }

        string Expression.Visitor<string>.visitRelationExpression(Expression.Relation expr)
        {
            string tempVariable = "__temp_" + tempVariableIndex++;

            var rTemp1 = expr.Left.Accept(this);
            var rTemp2 = expr.Right.Accept(this);

            addStatement("bool " + tempVariable + " = " + rTemp1 + Util.OpToC(expr.Operation) + rTemp2);
            return tempVariable;
        }

        string Expression.Visitor<string>.visitSizeExpression(Expression.Size expr)
        {
            var res = expr.Expr.Accept(this);
            return $"{res}->size";
        }
    }
}
