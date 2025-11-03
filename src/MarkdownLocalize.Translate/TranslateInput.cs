using System.Text.Json.Serialization;

namespace MarkdownLocalize;


struct TranslateInput
{
    [JsonPropertyName("source")]
    public string Source { get; set; }
}

