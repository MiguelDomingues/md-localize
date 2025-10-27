namespace MarkdownLocalize;


struct TranslateResult
{
    public string Source { get; set; }
    public string Target { get; set; }
    public bool Success { get; set; }
    public string Reason { get; set; }
}

