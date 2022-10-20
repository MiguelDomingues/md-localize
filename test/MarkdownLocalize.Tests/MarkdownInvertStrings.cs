using MarkdownLocalize.Markdown;
namespace MarkdownLocalize.Tests;

public class MarkdownInvertString
{

    string InvertString(StringInfo si)
    {
        char[] charArray = si.String.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    [Fact]
    public void HeadingSingle()
    {
        string md = MarkdownParser.Translate("# Heading 1", InvertString, null, out _);
        Assert.Equal("# 1 gnidaeH", md);
    }

    [Fact]
    public void HeadingMultiple()
    {
        string md = MarkdownParser.Translate("# Heading 1\n\n## Heading 2", InvertString, null, out _);
        Assert.Equal("# 1 gnidaeH\n\n## 2 gnidaeH", md);
    }

}