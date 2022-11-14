
using Karambolo.PO;
using MarkdownLocalize.Markdown;
using static MarkdownLocalize.Markdown.TranslateRenderer;

namespace MarkdownLocalize.Tests;

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
        string md = POT.Translate(catalog, originalMarkdown, null, null, false, out info);

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
        string md = POT.Translate(catalog, originalMarkdown, null, null, false, out info);

        Assert.Equal(translatedMarkdown, md);
        Assert.Equal(expectedTotalCount, info.TotalCount);
        Assert.Equal(expectedTranslatedCount, info.TranslatedCount);
    }

    [Fact]
    public void TranslateKeepSource()
    {
        var catalog = POT.Load(ReadPO("headings.pt-PT.po"));
        TranslationInfo info;
        string md = POT.Translate(catalog, "# Heading\n\n##New Heading", null, null, true, out info);

        Assert.Equal("# Título\n\n##New Heading", md);
        Assert.Equal(2, info.TotalCount);
        Assert.Equal(2, info.TranslatedCount);
    }
}