namespace MarkdownLocalize.Markdown
{
    public partial class EchoRenderer : TransformRenderer
    {

        public EchoRenderer(TextWriter writer, string originalMarkdown, RendererOptions opts) : base(writer, originalMarkdown, null, opts, null, null)
        {
        }

        protected override string Transform(string s, int index, bool isMarkdown)
        {
            return s;
        }


    }
}