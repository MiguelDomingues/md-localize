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
                string trimStart = html != html.TrimStart() ? html.Substring(0, html.Length - html.TrimStart().Length) : "";
                string trimEnd = html != html.TrimEnd() ? html.Substring(html.TrimEnd().Length) : "";
                html = html.Substring(trimStart.Length, html.Length - trimEnd.Length);
                renderer.Write(trimStart);

                if (!renderer.ShouldTransform(html))
                {
                    renderer.Write(html);
                }
                else if (renderer.Options.ParseHtml)
                {
                    IDocument doc = GetDocument(html, out int positionOffset);

                    if (doc.Body.Descendants().Count() == 0)
                    {
                        // The HTML is probably mal-formed. We keep it as is
                        renderer.Write(html);
                    }
                    else
                    {
                        if (renderer.Options.ImageRelativePath != null)
                            html = UpdateImageRelativePaths(renderer.Options.ImageRelativePath, html);

                        if (renderer.Options.KeepHTMLTagsTogetherEnabled())
                        {
                            if (doc.Body.Descendants<IText>().Count() > 0)
                                ProcessHTMLTogether(renderer, doc.Body, html, obj.Span.Start);
                            else
                                renderer.Write(html);
                        }
                        else
                            ProcessHTMLIndependent(renderer, doc, html, obj.Span.Start);
                    }
                }
                else
                {
                    renderer.WriteRaw(html, obj.Span.Start);
                }
                renderer.Write(trimEnd);
                renderer.PopElementType();
            }

            private void ProcessHTMLTogether(TransformRenderer renderer, INode parent, string html, int startOffset)
            {
                foreach (var node in parent.ChildNodes)
                {
                    IEnumerable<string> allTags = node.Descendants<IHtmlElement>().Select(c => c.TagName);
                    bool allTagsSkip = renderer.Options.CheckKeepHTMLTagsTogether(allTags);

                    if (node is IHtmlElement nodeHtml)
                    {
                        renderer.PushElementType(NodeToElementType(node));
                        string text = nodeHtml.InnerHtml;

                        int trimStartIndex = nodeHtml.OuterHtml.IndexOf(text.TrimStart());
                        string trimmedStart = nodeHtml.OuterHtml.Substring(0, trimStartIndex);

                        var trimmedHtml = text.Trim();

                        int trimEndIndex = text.TrimEnd().Length;
                        string trimmedEnd = nodeHtml.OuterHtml.Substring(trimStartIndex + trimmedHtml.Length);

                        if (allTagsSkip && node.ChildNodes.Length > 1)
                        {
                            if (trimmedHtml != "" && renderer.ShouldTransform(nodeHtml.OuterHtml))
                            {
                                string newHtml = trimmedStart.Trim() + renderer.Transform(trimmedHtml, startOffset + trimStartIndex, false) + trimmedEnd.Trim();
                                renderer.Write(newHtml);
                            }
                            else
                                renderer.Write(html);
                        }
                        else
                        {
                            if (trimmedHtml != "")
                            {
                                renderer.Write(trimmedStart.Trim());
                                ProcessHTMLTogether(renderer, node, trimmedHtml, startOffset);
                                renderer.Write(trimmedEnd.Trim());
                            }
                            else
                            {
                                renderer.Write(nodeHtml.OuterHtml);
                            }
                        }
                        renderer.PopElementType();
                    }
                    else if (node is IText)
                    {
                        renderer.PushElementType(ElementType.TEXT);
                        string text = node.TextContent;

                        int trimStartIndex = text.Length - text.TrimStart().Length;
                        string trimmedStart = text.Substring(0, trimStartIndex);

                        var trimmedHtml = text.Trim();

                        int trimEndIndex = text.TrimEnd().Length;
                        string trimmedEnd = trimmedHtml.Length != 0 ? text.Substring(trimEndIndex) : "";

                        if (trimmedHtml != "" && renderer.ShouldTransform(trimmedHtml))
                        {
                            node.TextContent = trimmedStart + renderer.Transform(trimmedHtml, startOffset + trimStartIndex, false) + trimmedEnd;
                        }
                        renderer.Write(node.TextContent);
                        renderer.PopElementType();
                    }
                    else
                    {
                        renderer.PushElementType(ElementType.HTML_RAW);
                        string newHtml = renderer.Transform(html, 0, false);
                        renderer.Write(newHtml);
                        renderer.PopElementType();
                    }
                }
            }

            private void ProcessHTMLIndependent(TransformRenderer renderer, IDocument doc, string html, int startOffset)
            {
                string parsedHTML = doc.Body.InnerHtml;

                int extractedStrings = 0;
                foreach (var node in doc.Body.Descendants())
                {
                    if (!node.HasChildNodes && !String.IsNullOrEmpty(node.TextContent))
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
                            node.TextContent = trimmedStart + renderer.Transform(trimmedHtml, startOffset + trimStartIndex, false) + trimmedEnd;
                            extractedStrings++;
                        }
                        renderer.PopElementType();
                    }
                }

                if (extractedStrings > 0 || renderer.Options.ImageRelativePath != null)
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

            private string UpdateImageRelativePaths(string relativePath, string html)
            {
                IDocument doc = GetDocument(html, out _);
                string parsedHTML = doc.Body.InnerHtml;

                int count = 0;

                foreach (var node in doc.Body.Descendants())
                {
                    if (node is IHtmlImageElement img)
                    {
                        string originalSrc = img.GetAttribute("src") ?? "";
                        string newSrc = Path.Combine(relativePath, originalSrc);
                        img.SetAttribute("src", newSrc);
                        count++;
                    }
                }

                if (count == 0)
                    return html;

                return doc.Body.InnerHtml;
            }

            private static IDocument GetDocument(string html, out int positionOffset)
            {
                var parser = new HtmlParser(new HtmlParserOptions
                {
                    IsKeepingSourceReferences = true
                });
                IDocument doc = parser.ParseDocument($"<html><body>{html}</body></html>");
                positionOffset = "<html><body>".Length;
                return doc;
            }

            private ElementType? NodeToElementType(INode node)
            {
                switch (node)
                {
                    case IText:
                    case IHtmlParagraphElement:
                    case IHtmlDivElement:
                        return ElementType.TEXT;
                    case IHtmlHeadingElement h when h.LocalName == "h1":
                        return ElementType.HEADING_1;
                    case IHtmlHeadingElement h when h.LocalName == "h2":
                        return ElementType.HEADING_2;
                    case IHtmlHeadingElement h when h.LocalName == "h3":
                        return ElementType.HEADING_3;
                    case IHtmlHeadingElement h when h.LocalName == "h4":
                        return ElementType.HEADING_4;
                    case IHtmlHeadingElement h when h.LocalName == "h5":
                        return ElementType.HEADING_5;
                    case IHtmlHeadingElement h when h.LocalName == "h6":
                        return ElementType.HEADING_6;

                    case IHtmlAnchorElement:
                        return ElementType.LINK_DEFINITION;
                    case IHtmlElement htmlCode when htmlCode.TagName.ToLowerInvariant() == TagNames.Code.ToLowerInvariant():
                        return ElementType.CODE;
                    case IHtmlElement htmlSup when htmlSup.TagName.ToLowerInvariant() == TagNames.Sup.ToLowerInvariant():
                    case IHtmlElement htmlSection when htmlSection.TagName.ToLowerInvariant() == TagNames.Section.ToLowerInvariant():
                    case IHtmlElement htmlStrong when htmlStrong.TagName.ToLowerInvariant() == TagNames.Strong.ToLowerInvariant():
                    case IHtmlElement htmlItalic when htmlItalic.TagName.ToLowerInvariant() == TagNames.I.ToLowerInvariant():
                    case IHtmlElement htmlEmphasis when htmlEmphasis.TagName.ToLowerInvariant() == TagNames.Em.ToLowerInvariant():
                    case IHtmlListItemElement:
                    case IHtmlUnorderedListElement:
                    case IHtmlPreElement:
                    case IHtmlBreakRowElement:
                        return ElementType.TEXT;
                    case IHtmlElement htmlDefinitionListElement when htmlDefinitionListElement.TagName.ToLowerInvariant() == TagNames.Dd.ToLowerInvariant():
                    case IHtmlElement htmlDefinitionList when htmlDefinitionList.TagName.ToLowerInvariant() == TagNames.Dl.ToLowerInvariant():
                        return ElementType.DEFINITION_TERM;
                    case IComment:
                        return ElementType.HTML_COMMENT;
                    case IHtmlTableElement:
                    case IHtmlTableSectionElement:
                    case IHtmlTableRowElement:
                    case IHtmlInlineFrameElement:
                    case IHtmlSpanElement:
                    case IHtmlTableDataCellElement:
                    case IHtmlTableHeaderCellElement:
                    case IHtmlElement:
                        return ElementType.HTML_RAW;
                }
                Console.Error.WriteLine("Unable to convert HTML element fo type " + node.GetType());
                return null;
            }
        }

    }
}