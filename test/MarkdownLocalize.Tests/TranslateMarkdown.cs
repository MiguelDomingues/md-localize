
using Karambolo.PO;
using MarkdownLocalize.Markdown;
using static MarkdownLocalize.Markdown.TranslateRenderer;

namespace MarkdownLocalize.Tests;

[Collection("MDLocalize Tests")]
public class TranslateMarkdown
{

    public static string ReadPO(string lang)
    {
        return File.ReadAllText($"resources/{lang}");
    }

    [Theory]
    [InlineData("headings.pt-PT.po", "---\ntag: Hello\n---\n\n# Heading\n\n## New Heading", "---\ntag: Olá\n---\n\n# Título\n\n## ", 3, 2)]
    [InlineData("headings.pt-PT.po", "# Heading", "# Título", 1, 1)]
    [InlineData("headings.pt-PT.po", "# Heading\n\n## Another Heading", "# Título\n\n## Outro Título", 2, 2)]
    [InlineData("headings.pt-PT.po", "# Heading\n\n## New Heading", "# Título\n\n## ", 2, 1)]
    public void TranslateSimple(string poFile, string originalMarkdown, string translatedMarkdown, int expectedTotalCount, int expectedTranslatedCount)
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableFrontMatter = true,
        });
        var catalog = POT.Load(ReadPO(poFile));
        TranslationInfo info;
        string md = POT.Translate(catalog, originalMarkdown, null, null, false, false, new string[] { "&quot;" }, out info);

        Assert.Equal(translatedMarkdown, md);
        Assert.Equal(expectedTotalCount, info.TotalCount);
        Assert.Equal(expectedTranslatedCount, info.TranslatedCount);
    }

    [Theory]
    [InlineData("headings.pt-PT.po", "<div class=\"awesome\">\nHeading\n</div>", "<div class=\"awesome\">\nTítulo\n</div>", 1, 1)]
    [InlineData("headings.pt-PT.po", "<div class=\"awesome\">\n\nHeading\n\n</div>", "<div class=\"awesome\">\n\nTítulo\n\n</div>", 1, 1)]
    public void TranslateHtml(string poFile, string originalMarkdown, string translatedMarkdown, int expectedTotalCount, int expectedTranslatedCount)
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ParseHtml = true,
        });
        var catalog = POT.Load(ReadPO(poFile));
        TranslationInfo info;
        string md = POT.Translate(catalog, originalMarkdown, null, null, false, false, new string[] { "&quot;" }, out info);

        Assert.Equal(translatedMarkdown, md);
        Assert.Equal(expectedTotalCount, info.TotalCount);
        Assert.Equal(expectedTranslatedCount, info.TranslatedCount);
    }

    [Fact]
    public void TranslateKeepSource()
    {
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, "# Heading\n\n##New Heading", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal("# Título\n\n##New Heading", md);
        Assert.Equal(2, info.TotalCount);
        Assert.Equal(2, info.TranslatedCount);
    }

    [Fact]
    public void Quote()
    {
        MarkdownParser.SetParserOptions(new RendererOptions());
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"- Heading

    > Hello
    World
", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"- Título

    > Olá
    Mundo
", md);
        Assert.Equal(2, info.TotalCount);
        Assert.Equal(2, info.TranslatedCount);
    }

    [Fact]
    public void QuoteTwo()
    {
        MarkdownParser.SetParserOptions(new RendererOptions());
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"- Heading

    > Hello
    > World
", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"- Título

    > Olá
    > Mundo
", md);
        Assert.Equal(2, info.TotalCount);
        Assert.Equal(2, info.TranslatedCount);
    }

    [Fact]
    public void ListWithIndentedText()
    {
        MarkdownParser.SetParserOptions(new RendererOptions());
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"1. Heading

    ![Hello](./image.png)

    ![Hello](./image2.png)

    Hello
    World

1. Heading
", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"1. Título

    ![Olá](./image.png)

    ![Olá](./image2.png)

    Olá
    Mundo

1. Título
", md);
        Assert.Equal(7, info.TotalCount);
        Assert.Equal(7, info.TranslatedCount);
    }

    [Fact]
    public void MultipleLiteralsTogetherImage()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            KeepLiteralsTogether = true,
        });
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"![Hello](image.png)", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"![Olá](image.png)", md);

        Assert.Equal(1, info.TotalCount);
        Assert.Equal(1, info.TranslatedCount);
    }


    [Fact]
    public void KeepHtmlTagsTogether()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            KeepLiteralsTogether = true,
            ParseHtml = true,
            KeepHtmlTagsTogether = new string[] { "br", "b", "i", "sup", "code", "strong", "em", "a", },
        });
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"# Heading

Hello

<p style=""font-size:12px"" markdown=""1"">

Hello
World

</p>", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"# Título

Olá

<p style=""font-size:12px"" markdown=""1"">

Olá
Mundo

</p>".ReplaceLineEndings(), md.ReplaceLineEndings());

        Assert.Equal(3, info.TotalCount);
        Assert.Equal(3, info.TranslatedCount);
    }


    [Fact]
    public void WhiteSpaceAfterHTML()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            KeepLiteralsTogether = true,
            ParseHtml = true,
            KeepHtmlTagsTogether = new string[] { "br", "b", "i", "sup", "code", "strong", "em", "a", },
        });
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"# Heading

<div class=""info"" markdown=""1"">

Hello

</div> 

Hello", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"# Título

<div class=""info"" markdown=""1"">

Olá

</div> 

Olá".ReplaceLineEndings(), md.ReplaceLineEndings());

        Assert.Equal(3, info.TotalCount);
        Assert.Equal(3, info.TranslatedCount);
    }

    [Fact]
    public void HTMLTable()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            KeepLiteralsTogether = true,
            ParseHtml = true,
            KeepHtmlTagsTogether = new string[] { "br", "b", "i", "sup", "code", "strong", "em", "a", },
        });
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"<table>  
<tbody>
<tr>  
<td>
Text 1
</td>  
<td>
Text 2
</td></tr>  
<tr>  
<td>
Text 3
</td>  
<td>
Text 4
</td></tr>  
<tr>  
<td>
Text 5
</td>  
<td>
Text 6
</td></tr>  
<tr>  
<td>
Text 7
</td>  
<td>
</td></tr>  
</tbody>
</table>", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"<table>  
<tbody>
<tr>  
<td>
Text 1
</td>  
<td>
Text 2
</td></tr>  
<tr>  
<td>
Text 3
</td>  
<td>
Text 4
</td></tr>  
<tr>  
<td>
Text 5
</td>  
<td>
Text 6
</td></tr>  
<tr>  
<td>
Text 7
</td>  
<td>
</td></tr>  
</tbody>
</table>".ReplaceLineEndings(), md.ReplaceLineEndings());

        Assert.Equal(7, info.TotalCount);
        Assert.Equal(7, info.TranslatedCount);
    }

    [Fact]
    public void UpdateHTMLImagePath()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ParseHtml = true,
            ImageRelativePath = "../../",
        });
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"<img src=""images/img.png"">", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"<img src=""../../images/img.png"">".ReplaceLineEndings(), md.ReplaceLineEndings());

        Assert.Equal(0, info.TotalCount);
        Assert.Equal(0, info.TranslatedCount);
    }

    [Theory]
    [InlineData("Hello \"Hey!\"")]
    [InlineData("Some `\"code\"`")]
    public void Quotes(string original)
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ParseHtml = true,
        });
        var catalog = POT.Load(ReadPO("quotes.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, original, null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(original, md);

        Assert.Equal(1, info.TotalCount);
        Assert.Equal(1, info.TranslatedCount);
    }


    [Fact]
    public void LinksRelativePath()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            LinkRelativePath = "../",
        });
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"
![](./image.png)

[url](./file.md)

[\[url\]](./file.md)

[http url](https://www.github.com)", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"
![](../image.png)

[url](../file.md)

[\[url\]](../file.md)

[http url](https://www.github.com)".ReplaceLineEndings(), md.ReplaceLineEndings());

    }

    [Fact]
    public void ImagesRelativePath()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ImageRelativePath = "../images",
        });
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"
![](./image.png)

[url](./file.md)

[\[url\]](./file.md)

[http url](https://www.github.com)", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"
![](../images/image.png)

[url](./file.md)

[\[url\]](./file.md)

[http url](https://www.github.com)".ReplaceLineEndings(), md.ReplaceLineEndings());

    }

    [Fact]
    public void AllRelativePath()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            LinkRelativePath = "../",
            ImageRelativePath = "../images/",
        });
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"
![](./image.png)

[url](./file.md)

[\[url\]](./file.md)

[http url](https://www.github.com)", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"
![](../image.png)

[url](../file.md)

[\[url\]](../file.md)

[http url](https://www.github.com)".ReplaceLineEndings(), md.ReplaceLineEndings());

    }


    [Fact]
    public void AnchorLinksNoReplace()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            LinkRelativePath = "../",
        });
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, @"[url](#anchor)", null, null, true, false, new string[] { "&quot;" }, out info);

        Assert.Equal(@"[url](#anchor)".ReplaceLineEndings(), md.ReplaceLineEndings());
    }


    [Fact]
    public void TableWithLineBreaks()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            LinkRelativePath = "../",
            EnablePipeTables = true,
            EnableCustomAttributes = true,
            KeepLiteralsTogether = true,
            ParseHtml = true,
            KeepHtmlTagsTogether = new string[] { "br", "b", "i", "sup", "code", "strong", "em", "a", "u", "ul", "li" },
            ReplaceNewLineInsideTable = true,
        });
        var catalog = POT.Load(ReadPO("table.po"));
        TranslationInfo info;
        string markdown = @"**ColA** |  **ColB** | 
---|---
Value A|Value B<br/><br/>Second Line<br/><br/><ul><li>Item 1</li><li>Item 2.</li></ul> |";
        string md = POT.Translate(catalog, markdown, null, null, true, true, new string[] { "&quot;" }, out info);

        string expected = @"**JP-ColA** |  **JP-ColB** | 
---|---
JP Value A|JP Value B<br /><br />JP Second Line<br /><br /><ul><li>JP Item 1</li><li>JP Item 2.</li></ul> |";

        Assert.Equal(expected.ReplaceLineEndings(), md.ReplaceLineEndings());
    }


    [Fact]
    public void HeadingWithLineBreaks()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            LinkRelativePath = "../",
            EnablePipeTables = true,
            EnableCustomAttributes = true,
            KeepLiteralsTogether = true,
            ParseHtml = true,
            KeepHtmlTagsTogether = new string[] { "br", "b", "i", "sup", "code", "strong", "em", "a", "u", "ul", "li" },
            ReplaceNewLineInsideHeading = true,
        });
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string markdown = @"## Line<br>Second";
        string md = POT.Translate(catalog, markdown, null, null, true, true, new string[] { "&quot;" }, out info);

        string expected = @"## Linha  <br />Segunda";

        Assert.Equal(expected.ReplaceLineEndings(), md.ReplaceLineEndings());
    }
}
