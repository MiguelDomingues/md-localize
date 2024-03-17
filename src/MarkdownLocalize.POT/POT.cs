using Karambolo.PO;
using MarkdownLocalize.Markdown;
using MarkdownLocalize.Utils;
using static MarkdownLocalize.Markdown.TranslateRenderer;
using System.Linq;
using System.Web;

namespace MarkdownLocalize;
public class POT
{

    public static string Append(POCatalog catalog, IEnumerable<StringInfo> strings, string[] extraTranslatorComments)
    {
        foreach (StringInfo si in strings)
        {
            POKey key = new POKey(NormalizeLineBreaks(si.String), null, si.Context);
            IPOEntry entry;
            if (!catalog.TryGetValue(key, out entry))
            {
                entry = new POSingularEntry(key);
                catalog.Add(entry);
            }

            if (si.ReferenceFile != null && si.ReferenceLine > 0)
            {
                if (entry.Comments == null)
                    entry.Comments = new List<POComment>();

                POReferenceComment porc = null;

                IEnumerable<POReferenceComment> refs = entry.Comments.Where(c => c is POReferenceComment).Cast<POReferenceComment>();

                if (refs.Count() > 0)
                    porc = refs.First();
                else
                {
                    porc = new POReferenceComment();
                    porc.References = new List<POSourceReference>();
                    entry.Comments.Add(porc);
                }

                if (!porc.References.Any(r => r.FilePath == si.ReferenceFile && r.Line == si.ReferenceLine))
                    porc.References.Add(new POSourceReference(si.ReferenceFile, si.ReferenceLine));
            }

            if (si.IsMarkdown)
                AddExtraTranslatorComments(entry, extraTranslatorComments);
        }

        return Write(catalog);
    }

    public static string Write(POCatalog catalog)
    {
        TextWriter writer = new StringWriter();
        GetPOGenerator().Generate(writer, catalog);
        writer.Flush();
        return writer.ToString();
    }

    private static void AddExtraTranslatorComments(IPOEntry entry, string[] extraTranslatorComments)
    {
        if (entry.Comments == null)
            entry.Comments = new List<POComment>();

        IEnumerable<POTranslatorComment> translatorComments = entry.Comments.Where(c => c is POTranslatorComment).Cast<POTranslatorComment>();

        foreach (string tc in extraTranslatorComments)
        {
            if (!translatorComments.Any(c => c.Text == tc))
            {
                POComment comment = GenerateComment(tc);
                entry.Comments.Add(comment);
            }
        }
    }

    private static POComment GenerateComment(string tc)
    {
        switch (tc[0])
        {
            case '.':
                return new POExtractedComment()
                {
                    Text = tc[1..].Trim(),
                };
            default:
                return new POTranslatorComment()
                {
                    Text = tc,
                };
        }
    }

    private static string NormalizeLineBreaks(string s)
    {
        return s.ReplaceLineEndings("\n");
    }

    public static string Generate(IEnumerable<StringInfo> strings, string[] extraTranslatorComments)
    {
        var catalog = GenerateCatalog();

        return Append(catalog, strings, extraTranslatorComments);
    }

    public static POCatalog Load(string pot)
    {
        var parser = new POParser(new POParserSettings
        {
            StringDecodingOptions = new POStringDecodingOptions()
            {
                KeepKeyStringsPlatformIndependent = true,
                KeepTranslationStringsPlatformIndependent = true,
            }
        });

        TextReader reader = new StringReader(pot);
        var result = parser.Parse(reader);

        if (result.Success)
        {
            return result.Catalog;
            // process the parsed data...
        }
        else
        {
            var diagnostics = result.Diagnostics;
            // examine diagnostics, display an error, etc...
            throw new Exception("Invalid PO content.");
        }
    }

    static POGenerator GetPOGenerator()
    {
        var generator = new POGenerator(
            new POGeneratorSettings
            {
                PreserveHeadersOrder = true,
                IgnoreEncoding = true,


            });
        return generator;
    }

    static POCatalog GenerateCatalog()
    {
        var catalog = new POCatalog();

        // setting required headers
        catalog.Encoding = "UTF-8";
        catalog.PluralFormCount = 2;
        catalog.Language = "en_US";

        // setting custom headers
        catalog.Headers = new Dictionary<string, string>
        {
            { "POT-Creation-Date", "1987-05-13 20:45+0000" },
            { "Project-Id-Version", "Markdown POT" },
            { "X-Generator", "Markdown POT" },
        };

        return catalog;
    }

    public static string Translate(POCatalog catalog, string markdown, string fileName, string pathToSource, bool keepSourceStrings, bool trimTranslation, IEnumerable<string> unescapeEntities, out TranslationInfo info)
    {
        string translatedMarkdown = MarkdownParser.Translate(markdown, (si) =>
        {
            var key = new POKey(NormalizeLineBreaks(si.String), null, si.Context);
            string translation = catalog.GetTranslation(key);
            if (trimTranslation)
                translation = translation.Trim();
            if (keepSourceStrings && (translation == null || translation == ""))
            {
                translation = si.String;
            }
            if (translation != null)
            {
                if (unescapeEntities != null)
                {
                    foreach (string entity in unescapeEntities)
                    {
                        translation = translation.Replace(entity, HttpUtility.HtmlDecode(entity));
                    }
                }
                translation = translation.Replace("&quot;", "\"");

                return translation.Trim();
            }
            return "";
        }, fileName, pathToSource, catalog.Language, out info);

        return translatedMarkdown;
    }
}
