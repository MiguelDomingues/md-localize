using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using static MarkdownLocalize.Markdown.TranslateRenderer;

namespace MarkdownLocalize.Markdown;
public class MarkdownParser
{
    public static MarkdownPipeline? _markdownPipeline { get; private set; }

    static RendererOptions options = new RendererOptions();


    public static void SetParserOptions(RendererOptions newOptions)
    {
        _markdownPipeline = null;
        options = newOptions;
    }

    public static IEnumerable<StringInfo> ExtractStrings(string markdown, string fileName)
    {
        using (var writer = new StringWriter())
        {
            var renderer = new ExtractStringsRenderer(writer, markdown, fileName, options);
            var document = Markdig.Markdown.Parse(markdown, GetPipeline());
            renderer.Render(document);
            return renderer.ExtractedStrings;
        }

    }

    public static string Echo(string markdown)
    {
        using (var writer = new StringWriter())
        {
            var renderer = new EchoRenderer(writer, markdown, options);
            var document = Markdig.Markdown.Parse(markdown, GetPipeline());
            renderer.Render(document);
            return writer.ToString();
        }

    }

    public static string Translate(string markdown, Func<StringInfo, string> func, string fileName, out TranslationInfo tInfo)
    {
        using (var writer = new StringWriter())
        {
            var renderer = new TranslateRenderer(writer, markdown, func, fileName, options);
            var document = Markdig.Markdown.Parse(markdown, GetPipeline());
            renderer.Render(document);
            tInfo = renderer.Info;
            return writer.ToString();
        }

    }

    private static MarkdownPipeline GetPipeline()
    {
        if (_markdownPipeline == null)
        {
            MarkdownPipelineBuilder builder = new MarkdownPipelineBuilder()
                .UsePreciseSourceLocation();

            if (options != null && options.EnablePipeTables)
                builder.UsePipeTables();
            if (options != null && options.EnableFrontMatter)
                builder = builder.UseYamlFrontMatter();
            if (options != null && options.EnableGitHubFlavoredMarkdownTaskLists)
                builder = builder.UseTaskLists();
            if (options != null && options.EnableCustomAttributes)
                builder.UseGenericAttributes(); // Must be last as it is one parser that is modifying other parsers

            builder = builder.EnableTrackTrivia();

            _markdownPipeline = builder.Build();

        }

        return _markdownPipeline;
    }

    internal static ElementType? ToElementType(LeafBlock lb)
    {
        switch (lb)
        {
            case HeadingBlock h when h.Level == 1:
                return ElementType.HEADING_1;
            case HeadingBlock h when h.Level == 2:
                return ElementType.HEADING_2;
            case HeadingBlock h when h.Level == 3:
                return ElementType.HEADING_3;
            case HeadingBlock h when h.Level == 4:
                return ElementType.HEADING_4;
            case HeadingBlock h when h.Level == 5:
                return ElementType.HEADING_5;
            case HeadingBlock h when h.Level == 6:
                return ElementType.HEADING_6;
            case ParagraphBlock:
                return ElementType.TEXT;
            case CodeBlock:
                return ElementType.CODE;
            case ThematicBreakBlock:
                return ElementType.THEMATIC_BREAK;
        }
        Console.Error.WriteLine("Unable to convert element fo type " + lb.GetType());
        return null;
    }

}
