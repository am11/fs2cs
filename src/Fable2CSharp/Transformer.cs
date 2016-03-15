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
                            .WithMembers(List<MemberDeclarationSyntax>( GetClassMembers(file) ));
        }

        private CSharpSyntaxNode TransformExpression (Fable.AST.Fable.Expr expr)
        {
            if ( expr.IsValue )
            {
                var value = (Expr.Value)expr;
                var kind = value.value;
                if ( kind.IsThis )
                {
                    return ThisExpression();
                } else if ( kind.IsNumberConst )
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
            } else if ( expr.IsApply )
            {
                var apply = (Expr.Apply)expr;
                var kind = apply.kind;

                if (apply.callee.IsValue && !kind.IsApplyGet )
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
                else if ( kind.IsApplyMeth )
                {
                    var ex = TransformExpression(apply.callee);
                }
                else if (kind.IsApplyGet)
                {
                    var left = apply.args.First();
                    return TransformExpression(left);
                }


                Console.WriteLine(apply.ToString());
            }

            throw new NotImplementedException(expr.ToString());
        }

        private SyntaxKind Typ2Type( Fable.AST.Fable.Type typ )
        {
            if ( typ.IsPrimitiveType )
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
        private ExpressionSyntax GetFieldValue(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            return (ExpressionSyntax)TransformExpression(member.Body);
        }

        private MemberDeclarationSyntax[] GetClassMembers(File file)
        {
            var result = new List<MemberDeclarationSyntax>();

            foreach( var declaration in file.Declarations )
            {
                if (declaration.IsMemberDeclaration)
                {
                    var fieldDeclaration = FieldDeclaration(VariableDeclaration(PredefinedType(Token(GetFieldType((Declaration.MemberDeclaration)declaration))))
                        .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(VariableDeclarator(GetFieldName((Declaration.MemberDeclaration)declaration))
                        .WithInitializer(EqualsValueClause(GetFieldValue((Declaration.MemberDeclaration)declaration))))))
                        .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword) }));
                    result.Add(fieldDeclaration);
                }
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
