using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using MarkdownLocalize.Utils;
using static MarkdownLocalize.Markdown.TranslateRenderer;

namespace MarkdownLocalize.Markdown;
public class MarkdownParser
{
    public static MarkdownPipeline? _markdownPipeline { get; private set; }

    public static RendererOptions Options = new RendererOptions();
    private static string REGEX_IMAGE = @"(!\[[^\]]*\]\()(.*?)\s*('(?:.*[^'])')?\s*(\))";
    private static string REGEX_LINK = @"([^!]?\[[^\]]*\]\()(.*?)\s*('(?:.*[^'])')?\s*(\))";

    public static void SetParserOptions(RendererOptions newOptions)
    {
        _markdownPipeline = null;
        Options = newOptions;
    }

    public static IEnumerable<StringInfo> ExtractStrings(string markdown, string fileName)
    {
        using (var writer = new StringWriter())
        {
            var renderer = new ExtractStringsRenderer(writer, markdown, fileName, Options);
            var document = Markdig.Markdown.Parse(markdown, GetPipeline());
            renderer.Render(document);
            return renderer.ExtractedStrings;
        }

    }

    public static string Echo(string markdown)
    {
        using (var writer = new StringWriter())
        {
            var renderer = new EchoRenderer(writer, markdown, Options);
            var document = Markdig.Markdown.Parse(markdown, GetPipeline());
            renderer.Render(document);
            return writer.ToString();
        }

    }

    public static string Translate(string markdown, Func<StringInfo, string> func, string fileName, out TranslationInfo tInfo)
    {
        using (var writer = new StringWriter())
        {
            var renderer = new TranslateRenderer(writer, markdown, func, fileName, Options);
            var document = Markdig.Markdown.Parse(markdown, GetPipeline());
            renderer.Render(document);
            tInfo = renderer.Info;
            string renderedMarkdown = writer.ToString();
            if (Options.ImageRelativePath != null)
                renderedMarkdown = UpdateRelativePaths(REGEX_IMAGE, renderedMarkdown, Options.ImageRelativePath);
            if (Options.LinkRelativePath != null)
                renderedMarkdown = UpdateRelativePaths(REGEX_LINK, renderedMarkdown, Options.LinkRelativePath);

            return renderedMarkdown;
        }

    }

    private static string UpdateRelativePaths(string pattern, string original, string relativePath)
    {
        MatchEvaluator evaluator = new MatchEvaluator((Match m) =>
        {
            return MatchReplacer(relativePath, m);
        });
        try
        {
            return Regex.Replace(original, pattern, evaluator,
                                            RegexOptions.Multiline | RegexOptions.Singleline
                                            );
        }
        catch (RegexMatchTimeoutException)
        {
            throw new Exception("Unable to update relative paths.");
        }
    }

    private static string MatchReplacer(string path, Match match)
    {
        string newString = match.Groups[1].Value;

        if (match.Groups[2].Value.StartsWith("http"))
        {
            newString += match.Groups[2].Value;
        }
        else
        {
            string newPath = PathUtils.SimplifyRelativePath(Path.Combine(path, match.Groups[2].Value));
            newString += newPath;
        }

        newString += match.Groups[3].Value + match.Groups[4].Value;

        return newString;
    }

    private static MarkdownPipeline GetPipeline()
    {
        if (_markdownPipeline == null)
        {
            MarkdownPipelineBuilder builder = new MarkdownPipelineBuilder()
                .UsePreciseSourceLocation();

            if (Options != null && Options.EnablePipeTables)
                builder.UsePipeTables();
            if (Options != null && Options.EnableFrontMatter)
                builder = builder.UseYamlFrontMatter();
            if (Options != null && Options.EnableGitHubFlavoredMarkdownTaskLists)
                builder = builder.UseTaskLists();
            if (Options != null && Options.EnableCustomAttributes)
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
