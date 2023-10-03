using Markdig.Extensions.TaskLists;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Linq;

namespace MarkdownLocalize.Markdown
{
    public abstract partial class TransformRenderer
    {
        class ContainerInlineRenderer : MarkdownObjectRenderer<TransformRenderer, ContainerInline>
        {

            protected override void Write(TransformRenderer renderer, ContainerInline obj)
            {

                if (obj.Count() == 1 && obj.FirstChild is LinkInline)
                {
                    // If only one child and is image or url
                    LinkInline l = (LinkInline)obj.FirstChild;
                    if (l.IsImage && !renderer.Options.SkipImageAlt)
                        Write(renderer, l);
                    else if (!l.IsImage)
                        Write(renderer, l);
                    return;
                }

                if (obj.Count() == 1 && obj.FirstChild is CodeInline)
                {
                    renderer.MoveTo(obj.FirstChild.Span.End);
                    return;
                }

                IEnumerable<Inline> skipChilds = obj.Where(c =>
                    c is AutolinkInline
                    || c is TaskList
                    || (c is LiteralInline && ((LiteralInline)c).Content.ToString().Trim() == ""));
                if (skipChilds.Count() == obj.Count())
                {
                    // If all childs are of these types, These are not extracted.
                    return;
                }

                // Move cursor to start of the obj element
                renderer.MoveTo(obj.Span.Start);

                if (renderer.Options.KeepLiteralsTogether)
                    ProcessChildsTogether(renderer, obj);
                else
                    ProcessChildsSeparate(renderer, obj);

                ExtractLabelsFromLinkInline(renderer, obj);

                //if (!obj.LastChild.Span.IsEmpty)
                //    renderer.MoveTo(obj.LastChild.Span.End + 1);
            }

            private void ProcessChildsSeparate(TransformRenderer renderer, ContainerInline obj)
            {
                // Let's skip childs from start
                IEnumerable<Inline> childs = obj;
                while (childs.Count() > 0 && SkipChild(childs.First()))
                {
                    ProcessChild(renderer, childs.First());
                    childs = childs.Skip(1);
                }
                if (childs.Count() > 0)
                {
                    var firstChildTransform = childs.First();

                    IEnumerable<Inline> childsEnd = new List<Inline>();

                    // Let's skip childs from end
                    while (childs.Count() > 0 && SkipChild(childs.Last()))
                    {
                        childsEnd = childsEnd.Append(childs.Last());
                        childs = childs.SkipLast(1);
                    }
                    // Childs are in reverse order
                    childsEnd = childsEnd.Reverse().ToList();

                    var lastChildTransform = childs.Last();

                    if (childs.Count() == 1)
                    {
                        switch (childs.First())
                        {
                            case EmphasisInline ei: // If only emphasis, then use childs of emphasis
                                childs = ei;
                                break;
                            case CodeInline: // If only code inline, ignore
                            case AutolinkInline: // if only auto link, ignore
                                childs = childs.Skip(1);
                                break;
                        }
                    }
                    if (childs.Count() > 0)
                    {
                        if (renderer.ProcessRawLinesIndependent)
                        {
                            renderer.WriteMultiple(childs, renderer.LastWrittenIndex);
                        }
                        else if (childs.Count() > 1 && childs.All(c => c is LiteralInline || c is LineBreakInline))
                        {
                            renderer.WriteMultiple(childs, renderer.LastWrittenIndex);
                        }
                        else
                        {
                            renderer.MoveTo(childs.First().Span.Start);
                            int length = childs.Last().Span.End + 1 - childs.First().Span.Start;

                            var objMarkdown = renderer.TakeNext(length);

                            renderer.WriteRaw(objMarkdown, childs.First().Span.Start);
                        }
                    }

                    foreach (Inline i in childsEnd)
                    {
                        ProcessChild(renderer, i);
                    }
                }
            }

            private bool SkipChildTogether(Inline i)
            {
                switch (i)
                {
                    case LineBreakInline:
                    case LinkInline l when l.IsImage:
                    case LiteralInline li when li.Content.ToString().Trim() == "":
                        return true;
                }
                return false;
            }

            private void ProcessChildsTogether(TransformRenderer renderer, ContainerInline obj)
            {
                // Let's skip childs from start
                IEnumerable<Inline> childs = obj;
                while (childs.Count() > 0 && SkipChildTogether(childs.First()))
                {
                    ProcessChild(renderer, childs.First());
                    childs = childs.Skip(1);
                }

                if (childs.Count() > 0)
                {
                    IEnumerable<Inline> childsEnd = new List<Inline>();

                    // Let's skip childs from end
                    while (childs.Count() > 0 && SkipChildTogether(childs.Last()))
                    {
                        childsEnd = childsEnd.Append(childs.Last());
                        childs = childs.SkipLast(1);
                    }
                    // Childs are in reverse order
                    childsEnd = childsEnd.Reverse().ToList();

                    if (childs.Count() == 1)
                    {
                        if (SkipChild(childs.First()))
                        {
                            ProcessChild(renderer, childs.First());
                            childs = childs.Skip(1);
                        }
                        else
                        {
                            switch (childs.First())
                            {
                                case EmphasisInline ei: // If only emphasis, then use childs of emphasis
                                    childs = ei;
                                    break;
                                case CodeInline: // If only code inline, ignore
                                case AutolinkInline: // if only auto link, ignore
                                    childs = childs.Skip(1);
                                    break;
                            }
                        }
                    }
                    if (childs.Count() > 0)
                    {
                        renderer.WriteMultipleTogether(childs, renderer.LastWrittenIndex);
                    }

                    foreach (Inline i in childsEnd)
                    {
                        ProcessChild(renderer, i);
                    }
                }
            }

            private static void ExtractLabelsFromLinkInline(TransformRenderer renderer, ContainerInline obj)
            {
                // Extract labels from urls
                IEnumerable<LinkInline> links = obj.Where(c => c is LinkInline).Cast<LinkInline>().Where(l => l.FirstChild != null);
                foreach (LinkInline l in links)
                {
                    int start = l.FirstChild.Span.Start;
                    var length = l.LastChild.Span.End - start + 1;
                    string altMarkdown = renderer.OriginalMarkdown.Substring(start, length);
                    if (l.IsImage && !renderer.Options.SkipImageAlt)
                    {
                        renderer.PushElementType(ElementType.IMAGE_ALT);
                        renderer.CheckTransform(altMarkdown, start, true); // Capture alt text
                    }
                    else if (!l.IsImage)
                    {
                        renderer.PushElementType(ElementType.LINK_LABEL);
                        renderer.CheckTransform(altMarkdown, start, true);
                    }
                    renderer.PopElementType();
                }

                //if (!obj.LastChild.Span.IsEmpty)
                //    renderer.MoveTo(obj.LastChild.Span.End + 1);
            }

            private void ProcessChild(TransformRenderer renderer, Inline child)
            {
                if (child is LinkInline l)
                {
                    if (l.IsImage && !renderer.Options.SkipImageAlt)
                    {

                        renderer.PushElementType(ElementType.IMAGE_ALT);
                        Write(renderer, child);
                        renderer.PopElementType();
                    }
                    else if (!l.IsImage)
                    {
                        renderer.PushElementType(ElementType.LINK_LABEL);
                        Write(renderer, child);
                        renderer.PopElementType();
                    }
                    renderer.MoveTo(l.Span.End + 1);

                }
            }

            private static bool SkipChild(Inline elem)
            {
                switch (elem)
                {
                    case TaskList:
                    case LinkInline:
                    case LineBreakInline:
                    case LiteralInline li when li.Content.ToString().Trim() == "":
                        return true;
                    default:
                        return false;
                }
            }

        }

    }
}