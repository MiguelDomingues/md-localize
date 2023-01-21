namespace MarkdownLocalize.Markdown
{
    public partial class ExtractStringsRenderer : TransformRenderer
    {

        public IEnumerable<StringInfo> ExtractedStrings { get; set; } = new List<StringInfo>();

        public ExtractStringsRenderer(TextWriter writer, string originalMarkdown, string fileName, RendererOptions opts) : base(writer, originalMarkdown, fileName, opts, null, null)
        {
        }

        protected override string Transform(string s, int index, bool isMarkdown)
        {
            ExtractedStrings = ExtractedStrings.Append(new StringInfo
            {
                String = s,
                Context = GetElementType(),
                ReferenceLine = GetLinePosition(index),
                ReferenceFile = !String.IsNullOrEmpty(this.FileName) ? this.FileName.Replace("\\", "/") : "",
                IsMarkdown = isMarkdown,
            });
            return s;
        }
    }
}