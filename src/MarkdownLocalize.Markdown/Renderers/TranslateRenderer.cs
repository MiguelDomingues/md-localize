namespace MarkdownLocalize.Markdown
{
    public partial class TranslateRenderer : TransformRenderer
    {
        private readonly Func<StringInfo, string> _translateFunction;


        public TranslationInfo Info = new TranslationInfo
        {
            TranslatedCount = 0,
            TotalCount = 0,
            MissingStrings = new HashSet<string>(),
        };

        public TranslateRenderer(TextWriter writer, string originalMarkdown, Func<StringInfo, string> func, string fileName, RendererOptions opts, string pathToSource) : base(writer, originalMarkdown, fileName, opts, pathToSource)
        {
            this._translateFunction = func;
        }

        protected override string Transform(string s, int index, bool isMarkdown)
        {
            Info.TotalCount++;

            StringInfo si = new StringInfo
            {
                String = s,
                Context = GetElementType(),
                ReferenceLine = GetLinePosition(index),
                ReferenceFile = this.FileName,
                IsMarkdown = isMarkdown,
            };

            string translated = _translateFunction(si);
            if (translated != null && translated != "")
                Info.TranslatedCount++;
            else
                Info.MissingStrings.Add(s);

            return translated;
        }


    }
}