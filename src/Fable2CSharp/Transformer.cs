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

        private SyntaxKind GetFieldType(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            var memberBody = (Expr.Value)member.Body;
            var memberType = (Fable.AST.Fable.Type.PrimitiveType)memberBody.Type;
            var memberTypeKind = (PrimitiveTypeKind.Number)memberType.Item;
            var memberTypeKindItem = memberTypeKind.Item;
            if (memberTypeKindItem.IsInt32) return SyntaxKind.IntKeyword;
            return SyntaxKind.ObjectKeyword;
        }
        private SyntaxToken GetFieldName(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            var memberKind = member.Kind;
            var memberKindGetter = (MemberKind.Getter)member.Kind;            
            return Identifier(memberKindGetter.name);
        }
        private LiteralExpressionSyntax GetFieldValue(Declaration.MemberDeclaration declaration)
        {
            var member = declaration.Item;
            var memberBody = (Expr.Value)member.Body;
            var memberValue = memberBody.value;
            var memberValueNumberConst = (ValueKind.NumberConst)memberValue;
            var res = (Fable.AST.U2<int, double>.Case1)memberValueNumberConst.Item1;            
            return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(res.Item));
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
