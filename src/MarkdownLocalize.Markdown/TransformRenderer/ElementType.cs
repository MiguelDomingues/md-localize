using System.ComponentModel;

namespace MarkdownLocalize.Markdown
{
    public enum ElementType
    {
        [Description("Heading (level 1)")]
        HEADING_1,

        [Description("Heading (level 2)")]
        HEADING_2,

        [Description("Heading (level 3)")]
        HEADING_3,

        [Description("Heading (level 4)")]
        HEADING_4,

        [Description("Heading (level 5)")]
        HEADING_5,

        [Description("Heading (level 6)")]
        HEADING_6,

        [Description("Image alternative text")]
        IMAGE_ALT,

        [Description("Hyperlink label")]
        LINK_LABEL,

        [Description("Front Matter property value")]
        YAML_FRONT_MATTER,

        [Description("Front Matter property '{0}'")]
        YAML_FRONT_MATTER_KEY,

        [Description("Raw HTML")]
        HTML_RAW,

        [Description("Text")]
        TEXT,

        [Description("HTML Comment")]
        HTML_COMMENT,

        [Description("Source Code")]
        CODE,

        [Description("HTML div container.")]
        HTML_DIV,

        [Description("HTML cell.")]
        HTML_CELL,

        [Description("Thematic Break")]
        THEMATIC_BREAK,
    }
}