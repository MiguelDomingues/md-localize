using Markdig.Renderers;
using Markdig.Syntax;

namespace MarkdownLocalize.Markdown
{
    public abstract partial class TransformRenderer
    {
        class ContainerBlockRenderer : MarkdownObjectRenderer<TransformRenderer, ContainerBlock>
        {
            protected override void Write(TransformRenderer renderer, ContainerBlock obj)
            {
                renderer.WriteChildren(obj);

                if (obj is MarkdownDocument)
                    // Flush remaining markdown 
                    renderer.Flush();
            }
        }

    }
}