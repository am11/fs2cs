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
                    var res = (Fable.AST.U2<int, double>.Case1)const1.Item1;
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(res.Item));
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
                    var ident = (ValueKind.IdentValue)kind;
                    return LiteralExpression(SyntaxKind.NullLiteralExpression); // TODO : make sure it works
                }
                else
                {
                    Console.WriteLine(kind.ToString());
                }
            } else if (expr.IsApply)
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
                        if (op.Item.IsBinaryPlus)
                        {
                            var leftES = (ExpressionSyntax)TransformExpression(left);
                            var rightES = (ExpressionSyntax)TransformExpression(right);
                            return BinaryExpression(SyntaxKind.AddExpression, leftES, rightES);
                        }
                    }

                }
                else if (kind.IsApplyMeth)
                {
                    var ex = (ExpressionSyntax)TransformExpression(apply.callee);
                    return
                        InvocationExpression(ex)
                        .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(
                            GetMethodArguments(apply.args.ToArray())
                        )));
                }
                else if (kind.IsApplyGet)
                {
                    var left = apply.args.First();
                    var ex = (LiteralExpressionSyntax)TransformExpression(left);
                    var token = ex.Token;
                    return IdentifierName(token.ValueText);
                } else
                {
                    Console.WriteLine(kind.ToString());
                }
            }
            else if ( expr.IsWrapped )
            {
                var wrapped = (Expr.Wrapped)expr;
                return TransformExpression(wrapped.Item1);
            }

            throw new NotImplementedException(expr.ToString());
        }

        private SyntaxKind Typ2Type(Fable.AST.Fable.Type typ)
        {
            if (typ.IsPrimitiveType)
            {
                var memberType = (Fable.AST.Fable.Type.PrimitiveType)typ;
                if (memberType.Item.IsNumber)
                {
                    var memberTypeKind = (PrimitiveTypeKind.Number)memberType.Item;
                    var memberTypeKindItem = memberTypeKind.Item;
                    if (memberTypeKindItem.IsInt32) return SyntaxKind.IntKeyword;
                }
                else if (memberType.Item.IsString)
                {
                    return SyntaxKind.StringKeyword;
                }
            }
            return SyntaxKind.ObjectKeyword;
        }

        private SyntaxKind GetFieldType(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            if (member.Body.IsValue)
            {
                var memberBody = (Expr.Value)member.Body;
                return Typ2Type(memberBody.Type);
            } else if (member.Body.IsApply)
            {
                var memberBody = (Expr.Apply)member.Body;
                return Typ2Type(memberBody.typ);
            }
            return SyntaxKind.ObjectKeyword;
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
        private bool IsField(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            var memberKind = member.Kind;
            return memberKind.IsGetter;
        }
        private bool IsMethod(Declaration.MemberDeclaration declaration)
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
        private SyntaxNodeOrToken[] GetMethodParameters(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            var result = new List<SyntaxNodeOrToken>();
            foreach ( var ident in member.Arguments )
            {
                var parameter = Parameter(Identifier(ident.name)).WithType(PredefinedType(Token(Typ2Type(ident.typ))));
                result.Add(parameter);
            }
            return result.ToArray();
        }
        private SyntaxNodeOrToken[] GetMethodArguments(Expr[] arguments)
        {
            //return new SyntaxNodeOrToken[] { Argument(IdentifierName("a")), Token(SyntaxKind.CommaToken), Argument(IdentifierName("b")) };
            var result = new List<SyntaxNodeOrToken>();
            foreach (var argument in arguments)
            {
                var parameter = Argument( (ExpressionSyntax) TransformExpression(argument) );
                result.Add(parameter);
                result.Add(Token(SyntaxKind.CommaToken));
            }
            return result.Take(result.Count-1).ToArray();
        }

        private Fable.AST.Fable.Expr GetMethodBody(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            return member.Body;
        }

        private MemberDeclarationSyntax[] GetClassMembers(File file)
        {
            var result = new List<MemberDeclarationSyntax>();

            foreach( var declaration in file.Declarations )
            {
                if (declaration.IsMemberDeclaration)
                {
                    var memberDeclaration = (Declaration.MemberDeclaration)declaration;
                    if (IsField(memberDeclaration))
                    {
                        var fieldDeclaration = FieldDeclaration(VariableDeclaration(PredefinedType(Token(GetFieldType(memberDeclaration))))
                            .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(VariableDeclarator(GetFieldName(memberDeclaration))
                            .WithInitializer(EqualsValueClause(GetFieldValue(memberDeclaration))))))
                            .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.ReadOnlyKeyword) }));
                        result.Add(fieldDeclaration);
                    }
                    else if (IsMethod(memberDeclaration))
                    {
                        var methodDeclaration =
                              MethodDeclaration(PredefinedType(Token(GetFieldType(memberDeclaration))), GetMethodName(memberDeclaration))
                              .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) }))
                              .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(GetMethodParameters(memberDeclaration))))
                              .WithBody(Block(
                                  ReturnStatement(
                                      (ExpressionSyntax)TransformExpression(GetMethodBody(memberDeclaration))
                                  )
                              ));
                        result.Add(methodDeclaration);
                    }
                    else throw new NotImplementedException(memberDeclaration.ToString());
                }
                else if (declaration.IsActionDeclaration) 
                {
                    var actionDeclaration = (Declaration.ActionDeclaration)declaration;
                    var expr = actionDeclaration.Item1;                    
                    var methodDeclaration =
                          MethodDeclaration(PredefinedType(Token(Typ2Type(expr.Type))), "Invoke")
                          .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) }))
                          .WithBody(Block(
                              ReturnStatement(
                                  (ExpressionSyntax)TransformExpression(expr)
                              )
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
                .WithMembers(SingletonList<MemberDeclarationSyntax>(GetClass(file)))
                .NormalizeWhitespace()
            ;
        }
    }
}
