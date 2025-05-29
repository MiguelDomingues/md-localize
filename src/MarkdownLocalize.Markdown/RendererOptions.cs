using Markdig;
using Markdig.Renderers.Normalize;
using static MarkdownLocalize.Markdown.TranslateRenderer;

namespace MarkdownLocalize.Markdown;
public class RendererOptions
{
    public bool EnableFrontMatter = false;

    public bool EnableGitHubFlavoredMarkdownTaskLists = false;

    public string[] FrontMatterExclude = new string[] { };

    public bool SkipImageAlt = false;

    public bool EnableCustomAttributes = false;

    public string[] IgnorePatterns = new string[] { };
    public string[] OnlyPatterns = new string[] { };

    public bool ParseHtml = false;

    public bool EnablePipeTables { get; set; }
    public bool EnableDefinitionLists { get; set; }

    public string ImageRelativePath { get; set; } = null;
    public string LinkRelativePath { get; set; } = null;
    public string FrontMatterSourceKey { get; set; }
    public bool UpdateFrontMatterLocale { get; set; }
    public string Locale { get; set; }
    public Dictionary<string, string> AddFrontMatterKeys { get; set; }
    public bool KeepLiteralsTogether = false;
    public string[] KeepHtmlTagsTogether = new string[] { };
    public bool ReplaceNewLineInsideTable = false;
    public bool ReplaceNewLineInsideHeading = false;


    internal bool CheckKeepHTMLTagsTogether(IEnumerable<string> tags)
    {
        if (KeepHtmlTagsTogether.Length == 0)
            return false;
        IEnumerable<string> extraTags = tags.Where(t => !KeepHtmlTagsTogether.Select(t => t.ToLower()).Contains(t.ToLower()));
        return !extraTags.Any();
    }

    internal bool KeepHTMLTagsTogetherEnabled()
    {
        return KeepHtmlTagsTogether.Length > 0;
    }
}
