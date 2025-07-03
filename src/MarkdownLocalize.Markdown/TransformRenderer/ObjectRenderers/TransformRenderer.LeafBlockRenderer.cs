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
                bool oldReplaceNewLinesByHTML = renderer.ForceReplaceNewLinesByHTML;

                ElementType? type = MarkdownParser.ToElementType(obj);
                renderer.PushElementType(type);

                if (obj is HeadingBlock && renderer.Options.ReplaceNewLineInsideHeading)
                    renderer.ForceReplaceNewLinesByHTML = true;

                renderer.WriteLeafInline(obj);

                renderer.PopElementType();

                renderer.ForceReplaceNewLinesByHTML = oldReplaceNewLinesByHTML;
            }
        }

    }
}