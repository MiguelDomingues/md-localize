using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Markdig.Renderers;
using Markdig.Syntax;

namespace MarkdownLocalize.Markdown
{
    public abstract partial class TransformRenderer
    {
        class HtmlBlockRenderer : MarkdownObjectRenderer<TransformRenderer, HtmlBlock>
        {
            protected override void Write(TransformRenderer renderer, HtmlBlock obj)
            {
                renderer.PushElementType(ElementType.HTML_RAW);

                renderer.MoveTo(obj.Span.Start);
                int length = obj.Span.End + 1 - obj.Span.Start;

                string html = renderer.TakeNext(length);

                if (!renderer.ShouldTransform(html))
                {
                    renderer.Write(html);
                }
                else if (renderer.Options.ParseHtml)
                {
                    using var context = BrowsingContext.New(Configuration.Default);
                    using var doc = context.OpenAsync(req => req.Content($"<html><body>{html}</body></html>")).GetAwaiter().GetResult();
                    string parsedHTML = doc.Body.InnerHtml;

                    int extractedStrings = 0;
                    foreach (var node in doc.Body.Descendents())
                    {
                        if (!node.HasChildNodes)
                        {
                            renderer.PushElementType(NodeToElementType(node));
                            string text = node.TextContent;

                            int trimStartIndex = text.Length - text.TrimStart().Length;
                            string trimmedStart = text.Substring(0, trimStartIndex);

                            var trimmedHtml = text.Trim();

                            int trimEndIndex = text.TrimEnd().Length;
                            string trimmedEnd = trimmedHtml.Length != 0 ? text.Substring(trimEndIndex) : "";

                            if (trimmedHtml != "" && renderer.ShouldTransform(parsedHTML))
                            {
                                node.TextContent = trimmedStart + renderer.Transform(trimmedHtml, trimStartIndex, false) + trimmedEnd;
                                extractedStrings++;
                            }
                            renderer.PopElementType();
                        }
                    }

                    if (extractedStrings > 0)
                    {
                        if (html.ReplaceLineEndings("\n") != parsedHTML)
                        {
                            Console.Error.WriteLine("Malformed html: " + html);
                            Console.Error.WriteLine("Expected html: " + parsedHTML);
                        }
                        renderer.Write(doc.Body.InnerHtml);
                    }
                    else
                        renderer.Write(html);

                }
                else
                {
                    renderer.WriteRaw(html, obj.Span.Start);
                }
                renderer.PopElementType();
            }

            private ElementType? NodeToElementType(INode node)
            {
                switch (node)
                {
                    case IHtmlBreakRowElement:
                        return ElementType.HTML_RAW;
                    case IComment:
                        return ElementType.HTML_COMMENT;
                    case IHtmlDivElement:
                        return ElementType.HTML_DIV;
                    case IText:
                        return ElementType.TEXT;

                }
                Console.Error.WriteLine("Unable to convert HTML element fo type " + node.GetType());
                return null;
            }
        }

    }
}