using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
namespace TmpSetTextSuggester;


[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SetTextSemanticAnalyzer)), Shared]
public class SetTextCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(SetTextSemanticAnalyzer.DiagnosticId);

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);

        var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: true);

        if (nodeToFix is not AssignmentExpressionSyntax assignmentExpression)
            return;
        
        context.RegisterCodeFix(
            CodeAction.Create(
                title: Resources.VI0001CodeFixTitle,
                createChangedDocument: c => FixAsync(context.Document, assignmentExpression, c),
                equivalenceKey: Resources.VI0001CodeFixTitle),
            context.Diagnostics);
    }
    
    private async Task<Document> FixAsync(Document document, AssignmentExpressionSyntax assignmentExpr, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        if (root == null) return document;

        if (assignmentExpr.Left is not MemberAccessExpressionSyntax memberAccessExpr) return document;

        SyntaxNode newExpr = //SyntaxFactory.ExpressionStatement(
            SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            memberAccessExpr.Expression,
                            SyntaxFactory.IdentifierName("SetText")
                        )
                        .WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken))
                )
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                            SyntaxFactory.Argument(assignmentExpr.Right)
                            )
                        )
                    );
        //);

        var newRoot = root.ReplaceNode(assignmentExpr, newExpr);

        return document.WithSyntaxRoot(newRoot);
    }
}