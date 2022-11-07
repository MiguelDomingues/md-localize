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

    public string ImageRelativePath { get; set; } = null;
    public string LinkRelativePath { get; set; } = null;
    public string FrontMatterSourceKey { get; set; }
    public bool UpdateFrontMatterLocale { get; set; }
    public string Locale { get; set; }
    public Dictionary<string, string> AddFrontMatterKeys { get; set; }
}
