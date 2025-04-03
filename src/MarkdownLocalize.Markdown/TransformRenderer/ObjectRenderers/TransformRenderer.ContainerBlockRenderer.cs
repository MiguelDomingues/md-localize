using Markdig.Extensions.Tables;
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
                bool oldReplaceNewLinesByHTML = renderer.ForceReplaceNewLinesByHTML;
                if (obj is Table && renderer.Options.EnablePipeTables && renderer.Options.ReplaceNewLineInsideTable)
                    renderer.ForceReplaceNewLinesByHTML = true;

                renderer.WriteChildren(obj);

                renderer.ForceReplaceNewLinesByHTML = oldReplaceNewLinesByHTML;

                if (obj is MarkdownDocument)
                    // Flush remaining markdown 
                    renderer.Flush();
            }
        }

    }
}