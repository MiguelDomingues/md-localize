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
        string md = MarkdownParser.Translate("# Heading 1", InvertString, null, null, null, out _);
        Assert.Equal("# 1 gnidaeH", md);
    }

    [Fact]
    public void HeadingMultiple()
    {
        string md = MarkdownParser.Translate("# Heading 1\n\n## Heading 2", InvertString, null, null, null, out _);
        Assert.Equal("# 1 gnidaeH\n\n## 2 gnidaeH", md);
    }

    [Theory]
    [InlineData("![abc](./images/some-image.png)", "![cba](../../../original-doc-path/images/some-image.png)")]
    [InlineData("![abc](images/some-image.png)", "![cba](../../../original-doc-path/images/some-image.png)")]
    [InlineData("The image ![abc](./images/some-image.png)", "egami ehT ![cba](../../../original-doc-path/images/some-image.png)")]
    public void UpdateImageRelativePath(string source, string expected)
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ImageRelativePath = "../../../original-doc-path/",
        });
        string md = MarkdownParser.Translate(source, InvertString, null, null, null, out _);
        Assert.Equal(expected, md);
    }

    [Theory]
    [InlineData("![abc](./images/some-image.png)", "![abc](../../../original-doc-path/images/some-image.png)")]
    public void UpdateImageRelativePathSkipAlt(string source, string expected)
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ImageRelativePath = "../../../original-doc-path/",
            SkipImageAlt = true,
        });
        string md = MarkdownParser.Translate(source, InvertString, null, null, null, out _);
        Assert.Equal(expected, md);
    }

    [Theory]
    [InlineData("[abc](https://example.com/image.png)", "[cba](https://example.com/image.png)")]
    [InlineData("![abc](https://example.com/image.png)", "![cba](https://example.com/image.png)")]
    [InlineData("[abc](./image.png)", "[cba](../../../original-doc-path/image.png)")]
    public void UpdateLinkRelativePath(string source, string expected)
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ImageRelativePath = "../../../original-doc-path/",
            LinkRelativePath = "../../../original-doc-path/",
        });
        string md = MarkdownParser.Translate(source, InvertString, null, null, null, out _);
        Assert.Equal(expected, md);
    }

    [Fact]
    public void Quote()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableGitHubFlavoredMarkdownTaskLists = true
        });

        string original = @"- [ ] Task

    > first
    > second
    > third
";
        string expected = @"- [ ] ksaT

    > driht
    > dnoces
    > tsrif
";
        string md = MarkdownParser.Translate(original, InvertString, null, null, null, out _);
        Assert.Equal(expected, md);
    }


    [Fact]
    public void LinkWithAttr()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableCustomAttributes = true,
            SkipImageAlt = false,
        });

        string original = @"[label](https://www.google.com) {.footnote}
";
        string expected = @"[lebal](https://www.google.com) {.footnote}
";
        string md = MarkdownParser.Translate(original, InvertString, null, null, null, out _);
        Assert.Equal(expected, md);
    }

    [Fact]
    public void LinkWithTextAttr()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableCustomAttributes = true,
            SkipImageAlt = false,
        });

        string original = @"The link [label](https://www.google.com) {.footnote}
";
        string expected = @"knil ehT [lebal](https://www.google.com) {.footnote}
";
        string md = MarkdownParser.Translate(original, InvertString, null, null, null, out _);
        Assert.Equal(expected, md);
    }
}


