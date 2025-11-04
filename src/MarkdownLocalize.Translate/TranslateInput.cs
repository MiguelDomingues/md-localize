using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MarkdownLocalize;


struct TranslateInput
{
    [JsonProperty(PropertyName = "source")]
    public string Source { get; set; }
}

