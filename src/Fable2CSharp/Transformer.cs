using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Fable.AST.Fable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace fs2cs.Fable2CSharp
{
    public class Transformer
    {
        // F# module - The default is public. C# class -  Internal is the default if no access modifier is specified. 
        private ClassDeclarationSyntax GetClass(File file)
        {
            return
            ClassDeclaration(file.Root.Name)
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithMembers(List<MemberDeclarationSyntax>(GetClassMembers(file)));
        }

        private CSharpSyntaxNode TransformExpression(Fable.AST.Fable.Expr expr)
        {
            if (expr.IsValue)
            {
                var value = (Expr.Value)expr;
                var kind = value.value;
                if (kind.IsThis)
                {
                    return ThisExpression();
                }
                else if (kind.IsNumberConst)
                {
                    var const1 = (ValueKind.NumberConst)kind;

                    if (const1.Item1.IsCase1)
                    {
                        var res = (Fable.AST.U2<Int32, Double>.Case1)const1.Item1;
                        return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(res.Item));
                    } else
                    {
                        var res = (Fable.AST.U2<Int32, Double>.Case2)const1.Item1;
                        return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(res.Item));
                    }
                    
                }
                else if (kind.IsStringConst)
                {
                    var memberValueStringConst = (ValueKind.StringConst)kind;
                    return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(memberValueStringConst.Item)); ;

                }
                else if (kind.IsIdentValue)
                {
                    var ident = (ValueKind.IdentValue)kind;
                    return IdentifierName(ident.Item.name);
                }
                else if (kind.IsNull)
                {
                    return LiteralExpression(SyntaxKind.NullLiteralExpression);
                }
                else if (kind.IsArrayConst)
                {
                    var arrayConst = (ValueKind.ArrayConst)kind;

                    var arrayConsKind = arrayConst.Item1;
                    if (arrayConst.kind.IsTuple)
                    {
                        var ack = arrayConst.Item1;
                        if (ack.IsArrayValues)
                        {
                            return
                                ArrayCreationExpression(ArrayType(PredefinedType(Token(SyntaxKind.ObjectKeyword))).
                                WithRankSpecifiers(SingletonList<ArrayRankSpecifierSyntax>(
                                    ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))))
                                .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression, SeparatedList<ExpressionSyntax>(new SyntaxNodeOrToken[] { LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1)), Token(SyntaxKind.CommaToken), LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(2)) })));
                        }
                        else
                            throw new NotImplementedException(ack.ToString());
                        
                    }
                    else if (arrayConsKind.IsArrayValues)
                    {
                        var arrayValues = (ArrayConsKind.ArrayValues)arrayConsKind;
                        var values = arrayValues.Item;
                        var result = new List<SyntaxNodeOrToken>();
                        foreach (var value1 in values)
                        {
                            var e = TransformExpression(value1);
                            result.Add(e);
                            result.Add(Token(SyntaxKind.CommaToken));
                        }                        
                        return ArrayCreationExpression(ArrayType(IdentifierName("dynamic"))
                            .WithRankSpecifiers(SingletonList<ArrayRankSpecifierSyntax>(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))))
                            .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression, 
                            SeparatedList<ExpressionSyntax>(
                                //LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1)), Token(SyntaxKind.CommaToken), LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(2)), Token(SyntaxKind.CommaToken), LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(3))
                                result.Take(result.Count - 1).ToArray()
                            )));
                    }
                    else if (arrayConsKind.IsArrayConversion)
                    {
                        var arrayConversion = (ArrayConsKind.ArrayConversion)arrayConsKind;
                        return TransformExpression(arrayConversion.Item);
                    }
                    else
                    {
                        throw new NotImplementedException(arrayConst.kind.ToString());
                    }
                }
                else if (kind.IsEmit)
                {
                    var emit = (ValueKind.Emit)kind;
                    return Block(SingletonList<StatementSyntax>(ExpressionStatement(IdentifierName(emit.Item))));
                }
                else if (kind.IsLambda)
                {
                    var lambda = (ValueKind.Lambda)kind;
                    var lambdaBodyExpr = TransformExpression(lambda.body);
                    if (lambda.body.IsApply)
                    {
                        var body = (Expr.Apply)lambda.body;
                        if (body.args.Length > lambda.args.Length)
                        {
                            List<Ident> ars = new List<Ident>();
                            foreach (var ar in body.args)
                            {
                                if (((Expr.Value)ar).value.IsIdentValue)
                                {
                                    var argValue = ((Expr.Value)ar).value;
                                    var argKind = (ValueKind.IdentValue)argValue;
                                    ars.Add(argKind.Item);
                                }
                            }
                            return
                                ParenthesizedLambdaExpression(lambdaBodyExpr)
                                .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(
                                    GetLambdaParameters(ars.ToArray()))));
                        }
                    }
                    else if (lambda.body.IsValue)
                    {
                        if (((Expr.Value)lambda.body).value.IsLambda)
                        {
                            var lambdaValue = ((Expr.Value)lambda.body);
                            var lambdaArgs = ((ValueKind.Lambda)lambdaValue.value).args;

                            foreach (var a in lambdaArgs)
                            {
                                if (lambdaArgs.Contains(a))
                                {
                                    //return null;
                                    var e = TransformExpression(lambdaValue);
                                    return e;
                                }
                            }
                        }
                    }
                    return
                        ParenthesizedLambdaExpression(lambdaBodyExpr)
                        .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(GetLambdaParameters(lambda.args.ToArray()))));
                }
                else
                {
                    throw new NotImplementedException(kind.ToString());
                }
            }
            else if (expr.IsApply)
            {
                var apply = (Expr.Apply)expr;
                var kind = apply.kind;

                if (apply.callee.IsValue && !kind.IsApplyGet)
                {
                    var value = (Expr.Value)apply.callee;
                    if (value.value.IsBinaryOp)
                    {
                        var left = apply.args.First();
                        var right = apply.args.Last();
                        var op = (ValueKind.BinaryOp)value.value;
                        var leftES = (ExpressionSyntax)TransformExpression(left);
                        var rightES = (ExpressionSyntax)TransformExpression(right);
                        if (op.Item.IsBinaryPlus)
                        {
                            return BinaryExpression(SyntaxKind.AddExpression, leftES, rightES);
                        }
                        else if (op.Item.IsBinaryMinus)
                        {
                            return BinaryExpression(SyntaxKind.SubtractExpression, leftES, rightES);
                        }
                        else if (op.Item.IsBinaryMultiply)
                        {
                            return BinaryExpression(SyntaxKind.MultiplyExpression, leftES, rightES);
                        }
                        else if (op.Item.IsBinaryEqualStrict)
                        {
                            return BinaryExpression(SyntaxKind.EqualsExpression, leftES, rightES);
                        }
                        else if (op.Item.IsBinaryGreater)
                        {
                            return BinaryExpression(SyntaxKind.GreaterThanExpression, leftES, rightES);
                        }
                        else if (op.Item.IsBinaryLess)
                        {
                            return BinaryExpression(SyntaxKind.LessThanExpression, leftES, rightES);
                        }
                        else
                        {
                            throw new NotImplementedException(op.Item.ToString());
                        }
                    }
                    else if (value.value.IsLambda)
                    {
                        var lambda = (ValueKind.Lambda)value.value;
                        return
                            ParenthesizedLambdaExpression(TransformExpression(lambda.body))
                            .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(GetLambdaParameters(lambda.args.ToArray()))));
                    }
                    else if (value.value.IsIdentValue)
                    {
                        var ident = (ValueKind.IdentValue)value.value;
                        if (kind.IsApplyMeth)
                        {
                            var ex = (ExpressionSyntax)TransformExpression(apply.callee);
                            return
                                InvocationExpression(ex)
                                .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(
                                    GetCSharpMethodArguments(apply.args.ToArray())
                                )));
                        }
                        /*
                        return
                            LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                            .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(VariableDeclarator(Identifier(ident.Item.name)))));*/
                    }
                    else
                    {
                        throw new NotImplementedException(value.value.ToString());
                    }

                }
                else if (kind.IsApplyMeth)
                {
                    var methodArguments = GetAllMethodArguments(apply, apply.typ.FullName);
                    var functionExpression = GetFunctionExpression(apply);
                    var csharpCall = (ExpressionSyntax)TransformExpression(functionExpression);
                    var csharpArgs = GetCSharpMethodArguments(methodArguments);

                    return
                        InvocationExpression(csharpCall)
                        .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(
                            csharpArgs
                        )));
                }
                else if (kind.IsApplyGet)
                {
                    var left = apply.args.First();
                    var ex = (LiteralExpressionSyntax)TransformExpression(left);
                    var token = ex.Token;
                    return IdentifierName(token.ValueText);
                }
                else
                {
                    throw new NotImplementedException(kind.ToString());
                }
            }
            else if (expr.IsWrapped)
            {
                var wrapped = (Expr.Wrapped)expr;
                return TransformExpression(wrapped.Item1);
            }
            else if (expr.IsSequential)
            {
                var sequential = (Expr.Sequential)expr;
                var result = new List<StatementSyntax>();
                var i = 0;
                var lasti = sequential.Item1.Count() - 1;
                foreach (var expr1 in sequential.Item1)
                {
                    var transf = TransformExpression(expr1);
                    if (transf is ExpressionSyntax)
                    {
                        var expressionSyntax = (ExpressionSyntax)transf;
                        if (i == lasti) {
                            result.Add(ReturnStatement(expressionSyntax));
                        }
                        else {
                            result.Add(ExpressionStatement(expressionSyntax));
                        }
                    }
                    else if (transf is LocalDeclarationStatementSyntax)
                    {
                        result.Add((LocalDeclarationStatementSyntax)transf);
                    }
                    else
                        throw new NotImplementedException(transf.ToString());

                    i++;
                }                
                return InvocationExpression(ObjectCreationExpression(GenericName(Identifier("Func")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName("dynamic"))))).WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(Argument(ParenthesizedLambdaExpression(Block(result)))))));
            }
            else if (expr.IsVarDeclaration)
            {
                var varDeclaration = (Expr.VarDeclaration)expr;
                return
                    LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                    .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(Identifier(varDeclaration.var.name))
                        .WithInitializer(EqualsValueClause((ExpressionSyntax)TransformExpression(varDeclaration.value))))));
            } else if (expr.IsIfThenElse)
            {
                var ifThenElse = (Expr.IfThenElse)expr;

                return
                    /*IfStatement( (ExpressionSyntax)TransformExpression(ifThenElse.guardExpr), (StatementSyntax)TransformExpression(ifThenElse.thenExpr)) //Block(SingletonList<StatementSyntax>(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("a"), BinaryExpression(SyntaxKind.AddExpression, IdentifierName("a"), LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))))))))
                    .WithElse(ElseClause(Block(SingletonList<StatementSyntax>(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("a"), BinaryExpression(SyntaxKind.MultiplyExpression, IdentifierName("a"), LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(2)))))))));*/
                    ConditionalExpression((ExpressionSyntax)TransformExpression(ifThenElse.guardExpr), (ExpressionSyntax)TransformExpression(ifThenElse.thenExpr), (ExpressionSyntax)TransformExpression(ifThenElse.elseExpr));

            }

                throw new NotImplementedException(expr.ToString());
        }

        private SyntaxNodeOrToken[] GetLambdaParameters(Ident[] idents)
        {
            var result = new List<SyntaxNodeOrToken>();
            foreach (var ident in idents)
            {
                var parameter = Parameter(Identifier(ident.name));
                result.Add(parameter);
                result.Add(Token(SyntaxKind.CommaToken));
            }
            return result.Take(result.Count - 1).ToArray();
        }


        private TypeSyntax Typ2Type(Fable.AST.Fable.Type typ, out Boolean isVoid)
        {
            isVoid = false;
            if (typ.IsPrimitiveType)
            {
                var memberType = (Fable.AST.Fable.Type.PrimitiveType)typ;

                if (memberType.Item.IsUnit)
                {
                    isVoid = true;
                    return PredefinedType(Token(SyntaxKind.VoidKeyword));
                }
                else if (memberType.Item.IsFunction)
                {
                    var memberTypeKind = (PrimitiveTypeKind.Function)memberType.Item;
                    List<SyntaxNodeOrToken> methodParameters = new List<SyntaxNodeOrToken>();
                    for (Int32 i = 0; i <= memberTypeKind.arity; i++)
                    {
                        methodParameters.Add(IdentifierName("dynamic"));
                        methodParameters.Add(Token(SyntaxKind.CommaToken));
                    }
                    methodParameters.RemoveAt(methodParameters.Count - 1);
                    return GenericName(Identifier("Func"))
                      .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(methodParameters)));
                }
            }

            return IdentifierName("dynamic");
        }


        private Int32 GetMethodArity(Fable.AST.Fable.Type.PrimitiveType type)
        {
            if (type.Item.IsFunction)
            {
                var f = (PrimitiveTypeKind.Function)type.Item;
                return f.arity;
            }
            return 0;
        }

        private TypeSyntax GetFieldType(Declaration.MemberDeclaration declaration, out Boolean isVoid)
        {
            isVoid = false;
            var member = declaration.Item;
            if (member.Body.IsValue)
            {
                var memberBody = (Expr.Value)member.Body;
                return Typ2Type(memberBody.Type, out isVoid);
            }
            else if (member.Body.IsApply)
            {
                var memberBody = (Expr.Apply)member.Body;
                return Typ2Type(memberBody.typ, out isVoid);
            }
            else if (member.Body.IsWrapped)
            {
                var memberBody = (Expr.Wrapped)member.Body;
                return Typ2Type(memberBody.Item2, out isVoid);
            }
            else if (member.Body.IsSequential)
            {
                var memberBody = (Expr.Sequential)member.Body;
                return IdentifierName("dynamic");
            }
            throw new NotImplementedException(declaration.ToString());
        }
        private SyntaxToken GetFieldName(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            var memberKind = member.Kind;
            var memberKindGetter = (MemberKind.Getter)member.Kind;
            return Identifier(memberKindGetter.name);
        }
        private SyntaxToken GetMethodName(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            var memberKind = member.Kind;
            var memberKindMethod = (MemberKind.Method)member.Kind;
            return Identifier(memberKindMethod.name);
        }
        private Boolean IsField(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            var memberKind = member.Kind;
            return memberKind.IsGetter;
        }
        private Boolean IsMethod(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            var memberKind = member.Kind;
            return memberKind.IsMethod;
        }

        private ExpressionSyntax GetFieldValue(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            return (ExpressionSyntax)TransformExpression(member.Body);
        }

        private List<Expr> GetAllMethodArguments(Expr.Apply apply, String methodName)
        {
            var methodArgs = new List<Expr>();
            if (apply.typ.FullName.Equals(methodName))
            {
                if (apply.callee.IsApply)
                {

                    var callee = (Expr.Apply)apply.callee;
                    if (callee.kind.IsApplyMeth)
                    {
                        methodArgs.AddRange(GetAllMethodArguments((Expr.Apply)apply.callee, methodName));
                    }
                }
            }
            methodArgs.AddRange(apply.args);

            return methodArgs;
        }

        private Expr GetFunctionExpression(Expr.Apply apply)
        {
            var testObject = apply.callee;

            while (testObject.Type.IsPrimitiveType
                   && !testObject.IsValue)
            {
                testObject = ((Expr.Apply)testObject).callee;
            }

            if (testObject.IsValue)
            {
                var value = ((Expr.Value)testObject).value;
                if (!value.IsIdentValue || value.Type.IsUnknownType)
                {
                    return apply.callee;
                }
            }

            return testObject;
        }


        private SyntaxNodeOrToken[] GetMethodParameters(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            var result = new List<SyntaxNodeOrToken>();
            foreach (var ident in member.Arguments)
            {
                Boolean isVoid;
                var parameter = Parameter(Identifier(ident.name)).WithType(Typ2Type(ident.typ, out isVoid));
                result.Add(parameter);
                result.Add(Token(SyntaxKind.CommaToken));
            }
            return result.Take(result.Count - 1).ToArray();
        }
        private SyntaxNodeOrToken[] GetCSharpMethodArguments(IEnumerable<Expr> arguments)
        {
            //return new SyntaxNodeOrToken[] { Argument(IdentifierName("a")), Token(SyntaxKind.CommaToken), Argument(IdentifierName("b")) };
            var result = new List<SyntaxNodeOrToken>();
            foreach (var argument in arguments)
            {
                var argVal = TransformExpression(argument);
                if (argVal is ExpressionSyntax)
                {
                    var parameter = Argument((ExpressionSyntax)argVal);
                    result.Add(parameter);
                }
                else
                {
                    //result.Add(argVal);
                }
                result.Add(Token(SyntaxKind.CommaToken));
            }
            return result.Take(result.Count - 1).ToArray();
        }

        private Fable.AST.Fable.Expr GetMethodBody(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            return member.Body;
        }

        private MemberDeclarationSyntax[] GetClassMembers(File file)
        {
            var result = new List<MemberDeclarationSyntax>();

            foreach (var declaration in file.Declarations)
            {
                if (declaration.IsMemberDeclaration)
                {
                    var memberDeclaration = (Declaration.MemberDeclaration)declaration;
                    if (IsField(memberDeclaration))
                    {
                        Boolean isVoid;
                        var fieldDeclaration = FieldDeclaration(VariableDeclaration(GetFieldType(memberDeclaration, out isVoid))
                            .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(VariableDeclarator(GetFieldName(memberDeclaration))
                            .WithInitializer(EqualsValueClause(GetFieldValue(memberDeclaration))))))
                            .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.ReadOnlyKeyword) }));
                        if (isVoid) { throw new ArgumentException("Field cannot be void"); }
                        result.Add(fieldDeclaration);
                    }
                    else if (IsMethod(memberDeclaration))
                    {
                        var parameters = GetMethodParameters(memberDeclaration);
                        Boolean isVoid;
                        var returnType = GetFieldType(memberDeclaration, out isVoid);
                        var methodName = GetMethodName(memberDeclaration);
                        var returnStatement = isVoid ? null : (ExpressionSyntax)TransformExpression(GetMethodBody(memberDeclaration));
                        var methodModifiers = TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) });
                        var methodDeclaration =
                               MethodDeclaration(returnType, methodName)
                               .WithModifiers(methodModifiers)
                              .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(parameters)))
                              .WithBody(Block(ReturnStatement(returnStatement)));
                        result.Add(methodDeclaration);
                    }
                    else throw new NotImplementedException(memberDeclaration.ToString());
                }
                else if (declaration.IsActionDeclaration)
                {
                    var actionDeclaration = (Declaration.ActionDeclaration)declaration;
                    var expr = actionDeclaration.Item1;
                    var statement = TransformExpression(expr);
                    var returnExpression = statement as ExpressionSyntax;
                    Boolean isVoid;
                    var returnType = Typ2Type(expr.Type, out isVoid);
                    var methodDeclaration =
                          MethodDeclaration(returnType, "Invoke")
                          .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) }))
                          .WithBody(Block(
                              isVoid? 
                              (
                              ( returnExpression is LiteralExpressionSyntax ) ?
                              (StatementSyntax)(ReturnStatement()) : 
                              (StatementSyntax)(ExpressionStatement(returnExpression))
                              )
                              :
                              (StatementSyntax)(ReturnStatement(returnExpression))
                          ));
                    result.Add(methodDeclaration);
                }
                else throw new NotImplementedException(declaration.ToString());
            }

            return result.ToArray();

        }

        public CompilationUnitSyntax Transform(File file, CompilationUnitSyntax compilationUnit, AdhocWorkspace workspace, OptionSet options)
        {
            return

                compilationUnit
                .WithUsings(List<UsingDirectiveSyntax>(new UsingDirectiveSyntax[] { UsingDirective(IdentifierName("System")), UsingDirective(QualifiedName(IdentifierName("fs2csLib"), IdentifierName("Impl"))).WithStaticKeyword(Token(SyntaxKind.StaticKeyword)) }))
                .WithMembers(SingletonList<MemberDeclarationSyntax>(GetClass(file)))
                .NormalizeWhitespace()
            ;
        }
    }
}
