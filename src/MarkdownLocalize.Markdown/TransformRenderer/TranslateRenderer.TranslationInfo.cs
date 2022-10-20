namespace MarkdownLocalize.Markdown
{
    public partial class TranslateRenderer
    {
        public struct TranslationInfo
        {
            private int? _translatedCount;
            private int? _totalCount;
            private HashSet<string> _missingStrings;

            public int TranslatedCount { get { return _translatedCount ?? 0; } set { _translatedCount = value; } }
            public int TotalCount { get { return _totalCount ?? 0; } set { _totalCount = value; } }
            public HashSet<string> MissingStrings { get { return _missingStrings; } set { _missingStrings = value; } }

        }
    }
}