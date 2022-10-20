using Markdig.Renderers;
using Markdig.Syntax;

namespace MarkdownLocalize.Markdown
{
    public abstract partial class TransformRenderer
    {
        class LeafBlockRenderer : MarkdownObjectRenderer<TransformRenderer, LeafBlock>
        {
            protected override void Write(TransformRenderer renderer, LeafBlock obj)
            {
                ElementType? type = MarkdownParser.ToElementType(obj);
                renderer.PushElementType(type);

                renderer.WriteLeafInline(obj);

                renderer.PopElementType();
            }
        }

    }
}