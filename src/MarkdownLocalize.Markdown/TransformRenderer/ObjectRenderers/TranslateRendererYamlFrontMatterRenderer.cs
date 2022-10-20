using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using YamlDotNet.Serialization;

namespace MarkdownLocalize.Markdown
{
    public abstract partial class TransformRenderer
    {
        class YamlFrontMatterRenderer : MarkdownObjectRenderer<TransformRenderer, YamlFrontMatterBlock>
        {

            protected override void Write(TransformRenderer renderer, YamlFrontMatterBlock obj)
            {
                renderer.PushElementType(ElementType.YAML_FRONT_MATTER);
                var reader = new StringReader(String.Join(Environment.NewLine, obj.Lines));

                object yamlObject = new Deserializer().Deserialize(reader);

                object newYaml = Convert(renderer, null, yamlObject);

                string yamlText = new SerializerBuilder()
                    .WithIndentedSequences()
                    .Build()
                    .Serialize(newYaml);

                renderer.WriteLine("---");
                renderer.Write(yamlText.ReplaceLineEndings("\n"));
                renderer.Write("---");
                renderer.SkipTo(obj.Span.End + 1);

                renderer.PopElementType();
            }

            private object Convert(TransformRenderer renderer, object key, object original)
            {
                switch (original)
                {
                    case String s:
                        renderer.PushElementType(ElementType.YAML_FRONT_MATTER_KEY, key.ToString());
                        string newValue = renderer.CheckTransform(s, renderer.LastWrittenIndex, false);
                        renderer.PopElementType();
                        return newValue;
                    case List<object> l:
                        return l.Select(elem => Convert(renderer, key, elem)).ToList();
                    case Dictionary<object, object> dict:
                        return dict
                            .Select(x => new KeyValuePair<object, object>(
                                x.Key,
                                renderer.Options.FrontMatterExclude.Contains(x.Key)
                                    ? x.Value
                                    : Convert(renderer, x.Key, x.Value)

                            ))
                            .ToDictionary(
                                        item => item.Key,
                                        item => item.Value
                                    );
                    case null:
                        return null;
                    default:
                        throw new Exception("Unsupported Yaml Element Exception: " + original.GetType());
                }
            }
        }

    }
}