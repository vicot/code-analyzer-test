using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace TmpSetTextSuggester;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SetTextSemanticAnalyzer : DiagnosticAnalyzer
{
    // Metadata of the analyzer
    public const string DiagnosticId = "VI0001";

    // You could use LocalizedString but it's a little more complicated for this sample
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.VI0001Title),
        Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Resources.VI0001MessageFormat), Resources.ResourceManager,
            typeof(Resources));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Resources.VI0001Description), Resources.ResourceManager,
            typeof(Resources));

    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
        Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    // Register the list of rules this DiagnosticAnalizer supports
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleAssignmentExpression);
        context.EnableConcurrentExecution();
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        const string tmpTextPropertyText = "text";
        var          tmpTextType         = context.Compilation.GetTypeByMetadataName("TMPro.TMP_Text");

        if (context.Node is not AssignmentExpressionSyntax assignmentExpr) return;
        if (assignmentExpr.Left is not MemberAccessExpressionSyntax memberAccessExpr) return;

        var memberSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpr).Symbol;
        if (memberSymbol == null) return;

        if (!SymbolEqualityComparer.Default.Equals(memberSymbol.ContainingType, tmpTextType)) return;
        if (!string.Equals(memberSymbol.Name, tmpTextPropertyText)) return;

        var diag = Diagnostic.Create(Rule, assignmentExpr.GetLocation());
        context.ReportDiagnostic(diag);
    }
}