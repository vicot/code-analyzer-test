// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace TMPro;
public class TMP_Text
{
    public string text { get; set; }

    public void test()
    {
        var x  = new TMP_Text();
        var zs = new TMP_Text[] { new TMP_Text() };

        zs[0].text = $"he{5}"+"lo"+3.ToString();
        x.text     = "hello";
        x.text     = "hello";
        x.text     = "hello";
    }

    public void SetText(string s){}
}