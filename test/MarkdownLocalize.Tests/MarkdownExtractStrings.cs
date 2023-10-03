using MarkdownLocalize.Markdown;
namespace MarkdownLocalize.Tests;

[Collection("MDLocalize Tests")]
public class MarkdownExtractStrings
{

    [Fact]
    public void HeadingSingle()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("# Heading 1", null).Select(si => si.String);
        Assert.Equal(new[] { "Heading 1" }, strings);
    }

    [Fact]
    public void HeadingMultiple()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("# Heading 1\n\n## Heading 2", null).Select(si => si.String);
        Assert.Equal(new[] { "Heading 1", "Heading 2" }, strings);
    }


    [Fact]
    public void FrontMatterSimple()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableFrontMatter = true,
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("---\ndescription: This is a text value\n---\n\n# Heading 1", null).Select(si => si.String);
        Assert.Equal(new[] { "This is a text value", "Heading 1" }, strings);
    }

    [Fact]
    public void FrontMatterList()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableFrontMatter = true,
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("---\ndescription: This is a text value\ntags:\n  - First\n  - Second\n---\n\n# Heading 1", null).Select(si => si.String);
        Assert.Equal(new[] { "This is a text value", "First", "Second", "Heading 1" }, strings);
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
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String);
        Assert.Equal(new[] { "This is a task item", "Task done!" }, strings);
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
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String);
        Assert.Equal(new[] { "This text should be translated", "First", "Second", "Heading" }, strings);
    }

    [Fact]
    public void Bold()
    {
        MarkdownParser.SetParserOptions(new RendererOptions());
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("This sentenced has some **bold** text.", null).Select(si => si.String);
        Assert.Equal(new[] { "This sentenced has some **bold** text." }, strings);
    }

    [Fact]
    public void BoldNonTrimmedEnd()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("This sentenced has some **bold** text. ", null).Select(si => si.String);
        Assert.Equal(new[] { "This sentenced has some **bold** text." }, strings);
    }

    [Fact]
    public void BoldNonTrimmed()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("   This sentenced has some **bold** text. ", null).Select(si => si.String);
        Assert.Equal(new[] { "This sentenced has some **bold** text." }, strings);
    }

    [Fact]
    public void ImageNoAlt()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("![](./images/some-image.png)", null).Select(si => si.String);
        Assert.Equal(new string[] { }, strings);
    }

    [Fact]
    public void ImageNoAltText()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("An image ![](./images/some-image.png) without alt text", null).Select(si => si.String);
        Assert.Equal(new string[] { "An image ![](./images/some-image.png) without alt text" }, strings);
    }

    [Fact]
    public void ImageAlt()
    {
        MarkdownParser.SetParserOptions(new RendererOptions());
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("![Landscape](./images/some-image.png)", null).Select(si => si.String);
        Assert.Equal(new[] { "Landscape" }, strings);
    }

    [Fact]
    public void ImageAltSkip()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            SkipImageAlt = true,
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("![Landscape](./images/some-image.png)", null).Select(si => si.String);
        Assert.Equal(new string[] { }, strings);
    }

    [Fact]
    public void ImageAltSkipWithText()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            SkipImageAlt = true,
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("![Landscape](./images/some-image.png) with text", null).Select(si => si.String);
        Assert.Equal(new string[] { "with text" }, strings);
    }

    [Fact]
    public void ImageAndText()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("![Landscape](./images/some-image.png) Beautiful", null).Select(si => si.String).Distinct();
        Assert.Equal(new[] { "Landscape", "Beautiful" }, strings);
    }

    [Fact]
    public void TextImageText()
    {
        MarkdownParser.SetParserOptions(new RendererOptions());
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("The following image ![Landscape](./images/some-image.png) is beautiful", null).Select(si => si.String).Distinct();
        Assert.Equal(new[] { "The following image ![Landscape](./images/some-image.png) is beautiful", "Landscape" }, strings);
    }

    [Fact]
    public void Link()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("[Google](https://www.google.com)", null).Select(si => si.String);
        Assert.Equal(new[] { "Google" }, strings);
    }

    [Fact]
    public void CustomAttributes()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableCustomAttributes = true,
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings("[Google](https://www.google.com) {.css-class}", null).Select(si => si.String).Distinct();
        Assert.Equal(new[] { "Google" }, strings);
    }

    [Fact]
    public void IgnorePattern()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            IgnorePatterns = new string[] { "<!--.*-->" },
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"Some text followed by

<!-- a comment -->", null).Select(si => si.String);
        Assert.Equal(new[] { "Some text followed by" }, strings);
    }

    [Fact]
    public void IgnorePatternUnderscore()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            IgnorePatterns = new string[] { "<!--\\s*_.*-->" },
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"Some text followed by

<!-- a comment -->

<!-- _ ignored -->", null).Select(si => si.String);
        Assert.Equal(new[] { "Some text followed by", "<!-- a comment -->" }, strings);
    }

    [Fact]
    public void Html()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"Some text followed by an html break:
        
<br>

with some text after.", null).Select(si => si.String);
        Assert.Equal(new[] { "Some text followed by an html break:", "<br>", "with some text after." }, strings);
    }

    [Fact]
    public void HtmlParse()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ParseHtml = true,
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"Some text followed by an html break:
        
<br>

with some text after.", null).Select(si => si.String);
        Assert.Equal(new[] { "Some text followed by an html break:", "with some text after." }, strings);
    }

    [Fact]
    public void ParseHtmlComments()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ParseHtml = true,
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"Some text followed by

<!-- a comment -->

<!-- _ not ignored -->", null).Select(si => si.String);
        Assert.Equal(new[] { "Some text followed by", "a comment", "_ not ignored" }, strings);
    }

    [Fact]
    public void ParseHtmlCommentsIgnore()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            IgnorePatterns = new string[] { "<!--\\s*_.*-->" },
            ParseHtml = true,
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"Some text followed by

<!-- a comment -->

<!-- _ ignored -->", null).Select(si => si.String);
        Assert.Equal(new[] { "Some text followed by", "a comment" }, strings);
    }

    [Fact]
    public void CodeInline()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"`a = b`", null).Select(si => si.String);
        Assert.Equal(new string[] { }, strings);
    }

    [Fact]
    public void CodeInlineNonTrimmed()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"   `a = b`
", null).Select(si => si.String);
        Assert.Equal(new string[] { }, strings);
    }

    [Fact]
    public void CodeInlineWithTextStart()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"Example: `a = b`", null).Select(si => si.String);
        Assert.Equal(new[] { "Example: `a = b`" }, strings);
    }

    [Fact]
    public void CodeInlineWithTextEnd()
    {
        MarkdownParser.SetParserOptions(new RendererOptions());
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"`a = b` is a good example", null).Select(si => si.String);
        Assert.Equal(new[] { "`a = b` is a good example" }, strings);
    }

    [Fact]
    public void CodeBlock()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"```language
a = b
```", null).Select(si => si.String);
        Assert.Equal(new string[] { }, strings);
    }


    [Fact]
    public void CodeBlockSurroundedByText()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"See the following example:
```language
a = b
```
This is a simple assign.", null).Select(si => si.String);
        Assert.Equal(new[] { "See the following example:", "This is a simple assign." }, strings);
    }

    [Fact]
    public void TaskWithLink()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableGitHubFlavoredMarkdownTaskLists = true,
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"- [ ] <https://github.com/octo-org/octo-repo/issues/740>
", null).Select(si => si.String);
        Assert.Equal(new string[] { }, strings);
    }

    [Fact]
    public void Quote()
    {
        MarkdownParser.SetParserOptions(new RendererOptions());
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"- An item

    > with
    two lines
", null).Select(si => si.String);
        Assert.Equal(new[] { "An item", "with\ntwo lines" }, strings);
    }

    [Fact]
    public void QuoteTwo()
    {
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(@"- An item

    > with
    > two lines
", null).Select(si => si.String);
        Assert.Equal(new[] { "An item", "with\ntwo lines" }, strings);
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
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String);
        Assert.Equal(new[] { "Column A", "Column B", "A1", "B1" }, strings);
    }

    [Fact]
    public void OnlyPattern()
    {
        string md = @"
Some text

<!--
text inside a comment
with multiple lines
-->

More text";

        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            OnlyPatterns = new[] { "<!--.*-->" },
            ParseHtml = true
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String);
        Assert.Equal(new[] { "text inside a comment\nwith multiple lines" }, strings);
    }

    [Fact]
    public void OnlyPatternNoStrings()
    {
        string md = @"
# Heading

- [ ] A task
    <!-- _do-not-translate -->

    > A quote
";
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            IgnorePatterns = new[] { "<!--\\s*_.*-->" },
            OnlyPatterns = new[] { "<!--.*-->" },
            ParseHtml = true
        });
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String);
        Assert.Equal(new string[] { }, strings);
    }

    [Fact]
    public void ListWithIndentedText()
    {
        string md = @"1. Item 1

    ![alt](./image.png)

    ![text](./image2.png)

    First
    Second
    Third

1. Item 2
";
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "Item 1", "alt", "text", "First\nSecond\nThird", "Item 2" }, strings);
    }

    [Fact]
    public void SingleString()
    {
        string md = @"- Item

    > One
";
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "Item", "One" }, strings);
    }

    [Fact]
    public void TwoStrings()
    {
        string md = @"- Item

    > One
    > Two
";
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "Item", "One\nTwo" }, strings);
    }


    [Fact]
    public void TableEmptyCell()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            ParseHtml = true,
        });
        string md = @"<table>
<thead>
<tr>
<th>Name</th>
<th>Job Role</th>
</tr>
</thead>
<tbody>
<tr>
<td>John</td>
<td></td>
</tr>
</tbody>
</table>";
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "Name", "Job Role", "John" }, strings);
    }

    [Fact]
    public void BracketsNoCustomAttributes()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableCustomAttributes = false
        });
        string md = @"[First].{Second}";
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "[First].{Second}" }, strings);
    }

    [Fact]
    public void BracketsCustomAttributes()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableCustomAttributes = true
        });
        string md = @"[First].{Second}";
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "[First]." }, strings);
    }

    [Fact]
    public void BracketsEscapeCustomAttributes()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableCustomAttributes = true
        });
        string md = @"[First].\{Second\}";
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { @"[First].\{Second\}" }, strings);
    }

    [Fact]
    public void BracketsEscapeNoCustomAttributes()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableCustomAttributes = false
        });
        string md = @"[First].\{Second\}";
        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { @"[First].\{Second\}" }, strings);
    }

    [Fact]
    public void MultipleBrackets()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableCustomAttributes = true
        });
        string md = @"\<First\> and \<First\>.[Second]

\{First\} and \{First\}.\<Second\>

\{First\} and \{First\}.[Second]

\<First\> and \<First\>.\<Second\>[First].\{Second\}";

        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] {
            @"\<First\> and \<First\>.[Second]",
            @"\{First\} and \{First\}.\<Second\>",
            @"\{First\} and \{First\}.[Second]",
            @"\<First\> and \<First\>.\<Second\>[First].\{Second\}" }, strings);
    }


    [Fact]
    public void MultipleStrings()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            EnableCustomAttributes = true
        });
        string md = @"The first. *The second!";

        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] {
            "The first. *The second!" }, strings);
    }

    [Fact]
    public void MultipleLiteralsSeparate()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            KeepLiteralsTogether = false,
        });
        string md = @"  [Label](https://www.example.com) and text.  ";

        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "Label", "and text." }, strings);
    }


    [Fact]
    public void MultipleLiteralsTogether()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            KeepLiteralsTogether = true,
        });
        string md = @"  [Label](https://www.example.com) and text.  ";

        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "[Label](https://www.example.com) and text.", "Label" }, strings);
    }

    [Fact]
    public void MultipleLiteralsTogetherImage()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            KeepLiteralsTogether = true,
        });
        string md = @"![](image.png) Text ";

        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "Text" }, strings);
    }

    [Fact]
    public void MultipleLiteralsTogetherBold()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            KeepLiteralsTogether = true,
        });
        string md = @"**Bold text**";

        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "Bold text" }, strings);
    }

    [Fact]
    public void MultipleLiteralsTogetherBullet()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            KeepLiteralsTogether = true,
        });
        string md = @"* ![](image.png) Text ";

        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "Text" }, strings);
    }

    [Fact]
    public void MultipleLiteralsTogetherBulletLink()
    {
        MarkdownParser.SetParserOptions(new RendererOptions()
        {
            KeepLiteralsTogether = true,
        });
        string md = @"* [Label](www.example.com)";

        IEnumerable<string> strings = MarkdownParser.ExtractStrings(md, null).Select(si => si.String).Distinct();
        Assert.Equal(new string[] { "Label" }, strings);
    }

}