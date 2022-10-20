using MarkdownLocalize.Markdown;
namespace MarkdownLocalize.Tests;

public class EchoTests
{

    [Fact]
    public void HeadingSingle()
    {
        string md = "# Heading 1";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void HeadingMultiple()
    {
        string md = "# Heading 1\n\n## Heading 2";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void FrontMatterSimple()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableFrontMatter = true,
        });
        string md = "---\ndescription: This is a text value\n---\n\n# Heading 1";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void FrontMatterList()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableFrontMatter = true,
        });
        string md = "---\ndescription: This is a text value\ntags:\n  - First\n  - Second\n---\n\n# Heading 1";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void TaskLists()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableGitHubFlavoredMarkdownTaskLists = true,
        });
        var md = @"- [ ] This is a task item
        - [x] Task done!
        ";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void FrontMatterExclude()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableFrontMatter = true,
            FrontMatterExclude = new[] { "theme" },
        });
        var md = File.ReadAllText("resources/front-matter.md");
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void Bold()
    {
        string md = "This sentenced has some **bold** text.";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void BoldNonTrimmedEnd()
    {
        string md = "This sentenced has some **bold** text. ";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void BoldNonTrimmed()
    {
        string md = "   This sentenced has some **bold** text. ";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void ImageNoAlt()
    {
        string md = "![](./images/some-image.png)";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void ImageNoAltText()
    {
        string md = "An image ![](./images/some-image.png) without alt text";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void ImageAlt()
    {
        string md = "![Landscape](./images/some-image.png)";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void ImageAltSkip()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            SkipImageAlt = true,
        });
        string md = "![Landscape](./images/some-image.png)";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void ImageAndText()
    {
        string md = "![Landscape](./images/some-image.png) Beautiful";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void TextImageText()
    {
        string md = "The following image ![Landscape](./images/some-image.png) is beautiful";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void Link()
    {
        string md = "[Google](https://www.google.com)";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void CustomAttributes()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableCustomAttributes = true,
        });
        string md = "[Google](https://www.google.com) {.css-class}";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void IgnorePattern()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            IgnorePatterns = new string[] { "<!--.*-->" },
        });
        string md = @"Some text followed by

<!-- a comment -->";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void IgnorePatternUnderscore()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            IgnorePatterns = new string[] { "<!--\\s*_.*-->" },
        });
        string md = @"Some text followed by

<!-- a comment -->

<!-- _ ignored -->";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void Html()
    {
        string md = @"Some text followed by an html break:
        
<br>

with some text after.";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void HtmlParse()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ParseHtml = true,
        });
        string md = @"Some text followed by an html break:
        
<br>

with some text after.";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void ParseHtmlComments()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ParseHtml = true,
        });
        string md = @"Some text followed by

<!-- a comment -->

<!-- _ not ignored -->";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void ParseHtmlCommentsIgnore()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            IgnorePatterns = new string[] { "<!--\\s*_.*-->" },
            ParseHtml = true,
        });
        string md = @"Some text followed by

<!-- a comment -->

<!-- _ ignored -->";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void CodeInline()
    {
        string md = @"`a = b`";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void CodeInlineNonTrimmed()
    {
        string md = @"   `a = b`
";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void CodeInlineWithTextStart()
    {
        string md = @"Example: `a = b`";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void CodeInlineWithTextEnd()
    {
        string md = @"`a = b` is a good example";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void CodeBlock()
    {
        string md = @"```language
a = b
```";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }


    [Fact]
    public void CodeBlockSurroundedByText()
    {
        string md = @"See the following example:
```language
a = b
```
This is a simple assign.";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void TaskWithLink()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableGitHubFlavoredMarkdownTaskLists = true,
        });
        string md = @"- [ ] <https://github.com/octo-org/octo-repo/issues/740>
";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void Quote()
    {
        string md = @"- An item

    > with
    two lines
";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void QuoteTwo()
    {
        string md = @"- An item

    > with
    > two lines
";
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md.ReplaceLineEndings("\n"), echoMD.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void PipeTable()
    {
        string md = @"
Column A | Column B
---------|---------
 A1 | B1";

        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnablePipeTables = true,
        });
        string echoMD = MarkdownParser.Echo(md);
        Assert.Equal(md, echoMD);
    }

}