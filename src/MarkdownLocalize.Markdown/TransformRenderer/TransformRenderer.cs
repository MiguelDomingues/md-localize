using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using MarkdownLocalize.Utils;

namespace MarkdownLocalize.Markdown
{
    public abstract partial class TransformRenderer : TextRendererBase<TransformRenderer>
    {
        public TransformRenderer(TextWriter writer, string originalMarkdown, string fileName, RendererOptions opts, string pathToSource, string locale) : base(writer)
        {
            OriginalMarkdown = originalMarkdown;
            Options = opts;
            FileName = fileName;
            this.PathToSource = pathToSource;
            Locale = locale;

            ObjectRenderers.Add(new QuoteBlockRenderer());
            ObjectRenderers.Add(new HtmlBlockRenderer());
            ObjectRenderers.Add(new YamlFrontMatterRenderer());
            ObjectRenderers.Add(new ContainerBlockRenderer());
            ObjectRenderers.Add(new LeafBlockRenderer());
            ObjectRenderers.Add(new ContainerInlineRenderer());
        }

        public string Locale { get; }
        public string PathToSource { get; }
        private readonly string OriginalMarkdown;
        private readonly RendererOptions Options;
        private int LastWrittenIndex = 0;
        protected string FileName { get; private set; } = null;
        private bool ProcessRawLinesIndependent = false;

        private Stack<string> ContextStack = new Stack<string>();

        protected abstract string Transform(string s, int index, bool isMarkdown);

        private string CheckTransform(string s, int index, bool isMarkdown)
        {
            if (s.Trim().Length == 0)
                return s;

            if (!ShouldTransform(s))
                return s;

            return Transform(s, index, isMarkdown);
        }

        private bool ShouldTransform(string s)
        {
            if (Options.OnlyPatterns.Length > 0)
            {
                if (!Options.OnlyPatterns.Any(p => Regex.Match(s, p, RegexOptions.IgnoreCase | RegexOptions.Singleline).Success))
                {
                    return false;
                }
            }

            foreach (string pattern in Options.IgnorePatterns)
            {
                Match m = Regex.Match(s, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (m.Success)
                    return false;
            }
            return true;
        }

        private string TakeNext(int length)
        {
            if (length == 0)
                throw new Exception("Invalid length");
            string result = OriginalMarkdown.Substring(LastWrittenIndex, length);
            LastWrittenIndex += length;
            return result;
        }

        private void MoveTo(int index)
        {
            if (index == LastWrittenIndex)
                return;
            int length = index - LastWrittenIndex;
            string next = TakeNext(length);
            Write(next);
            LastWrittenIndex = index;
        }

        private void SkipTo(int index)
        {
            if (index < LastWrittenIndex)
                throw new Exception("Invalid index");
            LastWrittenIndex = index;
        }

        private void Flush()
        {
            MoveTo(OriginalMarkdown.Length);
        }

        private void WriteRaw(string raw, int index)
        {
            int trimStartIndex = raw.Length - raw.TrimStart().Length;
            string trimmedStart = raw.Substring(0, trimStartIndex);
            Write(trimmedStart);    // Trimmed whitespace from start

            var trimmedMarkdown = raw.Trim();
            if (trimmedMarkdown.Length == 0)
            {
                // Do nothing
            }
            else
            {
                var newMarkdown = CheckTransform(trimmedMarkdown, index + trimStartIndex, true);
                Write(newMarkdown);    // Replaced markdown
            }

            if (trimmedMarkdown.Length != 0)
            {
                // If all gets trimmed then do not write it again
                int trimEndIndex = raw.TrimEnd().Length;
                string trimmedEnd = raw.Substring(trimEndIndex);
                Write(trimmedEnd);     // Trimmed whitespace from start
            }
        }

        private void WriteMultiple(IEnumerable<Inline> childs, int index)
        {
            // Let's trim all lines, while saving trimmed text
            List<string> trimChildStart = new List<string>();
            List<string> trimChildEnd = new List<string>();

            string trimmedS = "";
            int lastIndex = index;
            foreach (Inline i in childs)
            {
                if (i is LineBreakInline)
                {
                    continue;
                }
                int childStartIndex = i.Span.Start;
                string markdownBefore = OriginalMarkdown.Substring(lastIndex, childStartIndex - lastIndex);
                string childMarkdown = OriginalMarkdown.Substring(childStartIndex, i.Span.Length);

                int trimStartIndex = childMarkdown.Length - childMarkdown.TrimStart().Length;
                trimChildStart.Add(markdownBefore + childMarkdown.Substring(0, trimStartIndex));

                if (childMarkdown.Trim().Length != 0)
                {
                    int trimEndIndex = childMarkdown.TrimEnd().Length;
                    trimChildEnd.Add(childMarkdown.Substring(trimEndIndex));

                    trimmedS += childMarkdown.Trim() + "\n";
                    //lastIndex += childMarkdown.Length;
                    lastIndex = i.Span.End + 1;
                }
                else
                {
                    trimmedS += "\n";
                    trimChildEnd.Add("");
                    lastIndex += childMarkdown.Length;
                }

            }

            string transformedS = CheckTransform(trimmedS.Trim(), index, true);
            // Reconstruct the text with trimmed whitespace
            IEnumerable<string> transformedLines = transformedS.ReplaceLineEndings("\n").Split("\n");

            using (var eStart = trimChildStart.GetEnumerator())
            using (var eTransform = transformedLines.GetEnumerator())
            using (var eEnd = trimChildEnd.GetEnumerator())
            {
                while (eStart.MoveNext() && eTransform.MoveNext() && eEnd.MoveNext())
                {
                    Write(eStart.Current);
                    Write(eTransform.Current);
                    Write(eEnd.Current);
                }

                if (eStart.MoveNext() || eEnd.MoveNext())
                    throw new Exception("Translation of '" + trimmedS.Trim() + "' failed. Missing lines. Check for line breaks.");
                if (eTransform.MoveNext())
                    throw new Exception("Translation of '" + trimmedS.Trim() + "' failed. Extra lines found. Check for line breaks.");
            }
            SkipTo(childs.Last().Span.End + 1);
        }


        private void PushElementType(ElementType? e)
        {
            PushElementType(e, null);
        }

        private void PushElementType(ElementType? e, string arg)
        {
            if (arg != null)
                ContextStack.Push(string.Format(EnumUtils.GetDescription(e), arg));
            else
                ContextStack.Push(EnumUtils.GetDescription(e));
        }

        private string? PopElementType()
        {
            string? e;
            ContextStack.TryPop(out e);
            return e;
        }
        protected string? GetElementType()
        {
            string? e;
            ContextStack.TryPeek(out e);
            return e;
        }

        protected int GetLinePosition(int index)
        {
            return OriginalMarkdown.Substring(0, index + 1).Split("\n").Count();
        }

    }
}