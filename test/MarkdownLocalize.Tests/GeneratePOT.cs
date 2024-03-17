using Karambolo.PO;
using MarkdownLocalize.Markdown;

namespace MarkdownLocalize.Tests;

[Collection("MDLocalize Tests")]
public class GeneratePOT
{
    [Fact]
    public void GeneratePOTSingle()
    {
        string po = POT.Generate(new[] { new StringInfo { String = "Heading" } }, new string[] { });
        string expected = $@"msgid """"
msgstr """"
""POT-Creation-Date: 1987-05-13 20:45+0000\n""
""Project-Id-Version: Markdown POT\n""
""X-Generator: Markdown POT\n""
""Content-Transfer-Encoding: 8bit\n""
""Content-Type: text/plain; charset=UTF-8\n""
""Language: en_US\n""

msgid ""Heading""
msgstr """"
";
        Assert.Equal(expected, po);
    }

    [Fact]
    public void GeneratePOTMultiple()
    {
        string po = POT.Generate(new[] {
            new StringInfo { String = "Heading"},
            new StringInfo { String = "Another Heading" }
        }, new string[] { });
        string expected = $@"msgid """"
msgstr """"
""POT-Creation-Date: 1987-05-13 20:45+0000\n""
""Project-Id-Version: Markdown POT\n""
""X-Generator: Markdown POT\n""
""Content-Transfer-Encoding: 8bit\n""
""Content-Type: text/plain; charset=UTF-8\n""
""Language: en_US\n""

msgid ""Heading""
msgstr """"

msgid ""Another Heading""
msgstr """"
";
        Assert.Equal(expected, po);
    }

    [Fact]
    public void AppendPOTExisting()
    {
        string po = POT.Generate(new[] {
            new StringInfo { String ="Heading"},
            new StringInfo { String = "Another Heading"}
        }, new string[] { });
        POCatalog simple = POT.Load(po);
        string finalPO = POT.Append(simple, new[] { new StringInfo { String = "Heading" } }, new string[] { });

        string expected = $@"msgid """"
msgstr """"
""POT-Creation-Date: 1987-05-13 20:45+0000\n""
""Project-Id-Version: Markdown POT\n""
""X-Generator: Markdown POT\n""
""Content-Transfer-Encoding: 8bit\n""
""Content-Type: text/plain; charset=UTF-8\n""
""Language: en_US\n""

msgid ""Heading""
msgstr """"

msgid ""Another Heading""
msgstr """"
";
        Assert.Equal(expected, finalPO);
    }


    [Fact]
    public void AppendPOTNew()
    {
        string po = POT.Generate(new[] {
            new StringInfo { String ="Heading"},
            new StringInfo { String = "Another Heading"}
        }, new string[] { });
        POCatalog simple = POT.Load(po);
        string finalPO = POT.Append(simple, new[] { new StringInfo { String = "Third Heading" } }, new string[] { });

        string expected = $@"msgid """"
msgstr """"
""POT-Creation-Date: 1987-05-13 20:45+0000\n""
""Project-Id-Version: Markdown POT\n""
""X-Generator: Markdown POT\n""
""Content-Transfer-Encoding: 8bit\n""
""Content-Type: text/plain; charset=UTF-8\n""
""Language: en_US\n""

msgid ""Heading""
msgstr """"

msgid ""Another Heading""
msgstr """"

msgid ""Third Heading""
msgstr """"
";
        Assert.Equal(expected, finalPO);
    }

    [Fact]
    public void GeneratePOTMarkdownComments()
    {
        string markdown = @"# Heading
        
Some text with **bold**.
";
        IEnumerable<StringInfo> strings = Markdown.MarkdownParser.ExtractStrings(markdown, "./file.md");
        string po = POT.Generate(strings, new string[] { ". This is markdown (extracted comment).", "Markdown content below." });
        string expected = $@"msgid """"
msgstr """"
""POT-Creation-Date: 1987-05-13 20:45+0000\n""
""Project-Id-Version: Markdown POT\n""
""X-Generator: Markdown POT\n""
""Content-Transfer-Encoding: 8bit\n""
""Content-Type: text/plain; charset=UTF-8\n""
""Language: en_US\n""

#  Markdown content below.
#. This is markdown (extracted comment).
#: ./file.md:1
msgctxt ""Heading (level 1)""
msgid ""Heading""
msgstr """"

#  Markdown content below.
#. This is markdown (extracted comment).
#: ./file.md:3
msgctxt ""Text""
msgid ""Some text with **bold**.""
msgstr """"
";
        Assert.Equal(expected, po);
    }
}