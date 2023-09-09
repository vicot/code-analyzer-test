using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        TmpSetTextSuggester.SetTextSemanticAnalyzer>;
using VerifierFix =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<TmpSetTextSuggester.SetTextSemanticAnalyzer,
        TmpSetTextSuggester.SetTextCodeFixProvider>;

namespace TmpSetTextSuggester.Tests;

public class DebugTests
{
    [Fact]
    public async Task FindIssue()
    {
        const string text = @"
namespace TMPro;
public class TMP_Text
{
    public string text { get; set; }

    public void test()
    {
        var x = new TMP_Text();
        x.text = ""hello"";
    }
}
";

        var expected = Verifier.Diagnostic()
            .WithLocation(10, 9);
        await Verifier.VerifyAnalyzerAsync(text, expected); //.ConfigureAwait(false);
    }
    
    [Fact]
    public async Task FixIssue()
    {
        const string text = @"
namespace TMPro;
public class TMP_Text
{
    public string text { get; set; }

    public void test()
    {
        var x = new TMP_Text();
        x.text = ""hello"";
    }

    public void SetText(string s){}
}
";

        const string newText = @"
namespace TMPro;
public class TMP_Text
{
    public string text { get; set; }

    public void test()
    {
        var x = new TMP_Text();
        x.SetText(""hello"");
    }

    public void SetText(string s){}
}
";

        var expected = VerifierFix.Diagnostic()
            .WithLocation(10, 9);
        await VerifierFix.VerifyCodeFixAsync(text, expected, newText);
    }

/*
public class C
{
    public string text { get; set; }

    public void test()
    {
        var x = new C();
        x.text = "hello";
    }
}
*/
}