using Markdig.Renderers;
using Markdig.Syntax;

namespace MarkdownLocalize.Markdown
{
    public abstract partial class TransformRenderer
    {

        class QuoteBlockRenderer : MarkdownObjectRenderer<TransformRenderer, QuoteBlock>
        {
            protected override void Write(TransformRenderer renderer, QuoteBlock obj)
            {
                bool beforeProcessRawLinesIndependent = renderer.ProcessRawLinesIndependent;
                renderer.ProcessRawLinesIndependent = false;
                renderer.WriteChildren(obj);
                renderer.ProcessRawLinesIndependent = beforeProcessRawLinesIndependent;
            }
        }

    }
}