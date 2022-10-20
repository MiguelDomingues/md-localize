using MarkdownLocalize.Utils;

namespace MarkdownLocalize.Markdown
{
    public struct StringInfo
    {
        public string String;
        public string Context;
        public string ReferenceFile;
        public int ReferenceLine;
        public bool IsMarkdown;
    }
}