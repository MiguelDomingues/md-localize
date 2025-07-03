using McMaster.Extensions.CommandLineUtils;
using MarkdownLocalize.Utils;
using System.ComponentModel.DataAnnotations;
using MarkdownLocalize.Markdown;
using System.IO;
using System;
using System.Collections.Generic;
using static MarkdownLocalize.Markdown.TranslateRenderer;
using System.Linq;
using Karambolo.PO;

namespace MarkdownLocalize.CLI
{
    public class Program
    {
        private const string ACTION_GENERATE_POT = "generate-pot";
        private const string ACTION_TRANSLATE = "translate";
        public const string TRANSLATION_INFO = "Translated {0} out of {1} strings.";

        public static int Main(string[] args)
        {
            return CommandLineApplication.Execute<Program>(args);
        }

        [Option("--action|-a", "The action to perform.", CommandOptionType.SingleValue)]
        [McMaster.Extensions.CommandLineUtils.AllowedValues(ACTION_GENERATE_POT, ACTION_TRANSLATE, IgnoreCase = true)]
        public string Action { get; }

        [Option("--input|-i", "Input file/directory.", CommandOptionType.SingleValue)]
        [Required]
        [FileOrDirectoryExists]
        public string Input { get; }

        [Option("--output|-o", "Output file/directory.", CommandOptionType.SingleValue)]
        public string Output { get; } = "";

        [Option("--po-file|-po", ".po/.pot file ", CommandOptionType.SingleValue)]
        public string POTFile { get; }

        [Option("--po-dir|-pod", "The directory to create the .po/.pot files. Directory structure is kept.", CommandOptionType.SingleValue)]
        public string POTDirectory { get; }

        [Option("--locale|-l", "Locale.", CommandOptionType.SingleValue)]
        public string Locale { get; }

        [Option("--gfm-task-lists", "Enable GitHub Flavored Markdown task lists.", CommandOptionType.NoValue)]
        public bool GitHubFlavoredMarkdownTaskLists { get; }

        [Option("--gfm-tables", "Enable GitHub Flavored tables.", CommandOptionType.NoValue)]
        public bool GitHubFlavoredMarkdownTables { get; }

        [Option("--gfm-front-matter", "Enable GitHub Flavored Markdown Front Matter.", CommandOptionType.NoValue)]
        public bool GitHubFlavoredMarkdownFrontMatter { get; }

        [Option("--gfm-front-matter-exclude|-fme", "Keys to ignore in Front Matter.", CommandOptionType.MultipleValue)]
        public string[] FrontMatterExclude { get; } = new string[] { };

        [Option("--ignore-image-alt", "Ignore image alt texts.", CommandOptionType.NoValue)]
        public bool IgnoreImageAlt { get; } = false;

        [Option("--custom-attributes", "Enable custom attributes (e.g. {.css-class}).", CommandOptionType.NoValue)]
        public bool CustomAttributes { get; } = false;

        [Option("--definition-lists", "Enable definition lists.", CommandOptionType.NoValue)]
        public bool EnableDefinitionLists { get; } = false;

        [Option("--ignore-pattern", "Regex patterns to ignore literals.", CommandOptionType.MultipleValue)]
        public string[] IgnorePatterns { get; } = new string[] { };

        [Option("--include-only-pattern", "Only matching literals with the regex provided will be considered.", CommandOptionType.MultipleValue)]
        public string[] OnlyPatterns { get; } = new string[] { };

        [Option("--parse-html", "Parse HTML found in markdown.", CommandOptionType.NoValue)]
        public bool ParseHtml { get; } = false;

        [Option("--min-ratio", "Min ratio to write translated file.", CommandOptionType.SingleValue)]
        [Range(0, 100)]
        public int MinRatio { get; } = 0;

        [Option("--markdown-translator-comment|-mtc", "Extra translator comment to add to .pot.", CommandOptionType.MultipleValue)]
        public string[] TranslatorComments { get; } = new string[] { };

        [Option("--update-image-relative-paths", "Update images relative paths to refer original files.", CommandOptionType.NoValue)]
        public bool UpdateImageRelativePaths { get; } = false;

        [Option("--update-links-relative-paths", "Update links relative paths to refer original files.", CommandOptionType.NoValue)]
        public bool UpdateLinksRelativePaths { get; } = false;

        [Option("--add-front-matter-source", "Add a new key (specified as parameter) to the front matter with the relative path to the source file.", CommandOptionType.SingleValue)]
        public string FrontMatterSourceKey { get; } = null;

        [Option("--update-front-matter-locale", "Update locale in front matter.", CommandOptionType.NoValue)]
        public bool UpdateFrontMatterLocale { get; } = false;

        [Option("--add-front-matter-key", "Add key:value to front-matter.", CommandOptionType.MultipleValue)]
        public List<string> AddFrontMatter { get; } = null;

        [Option("--keep-source-strings", "Keep source strings for non-translated strings.", CommandOptionType.NoValue)]
        public bool KeepSourceStrings { get; } = false;

        [Option("--file-suffix", "Suffix to add to output files.", CommandOptionType.SingleValue)]
        public string FileSuffix { get; } = "";

        [Option("--append-pot", "Append to existing .pot file if it exists.", CommandOptionType.NoValue)]
        public bool AppendPot { get; } = false;

        [Option("--ignore-missing-po", "Write output if .po file is missing.", CommandOptionType.NoValue)]
        public bool IgnoreMissingPO { get; } = false;

        [Option("--keep-literals-together", "Keep multiple literals within the same block as a single string.", CommandOptionType.NoValue)]
        public bool KeepLiteralsTogether { get; } = false;

        [Option("--keep-html-together", "HTML tags to keep within text when extracting strings", CommandOptionType.MultipleValue)]
        public string[] KeepHTMLTagsTogether { get; } = Array.Empty<string>();

        [Option("--unescape-entities", "HTML entities to be unescaped/decoded", CommandOptionType.MultipleValue)]
        public string[] UnescapeEntities { get; } = Array.Empty<string>();

        [Option("--trim-translations", "Trim translations retrieved from PO file", CommandOptionType.NoValue)]
        public bool TrimTranslations { get; } = false;

        [Option("--use-br-inside-tables", "New lines are replaced by a <br/> tag when used inside tables.", CommandOptionType.NoValue)]
        public bool UseBRInsideTables { get; } = false;

        [Option("--use-br-inside-headings", "New lines are replaced by a <br/> tag when used inside headings.", CommandOptionType.NoValue)]
        public bool UseBRInsideHeadings { get; } = false;

        private int OnExecute()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            InitMarkdownParserOptions();
            if (File.GetAttributes(Input).HasFlag(FileAttributes.Directory))
            {
                DoDirectory(Input, Output, this.POTDirectory);
            }
            else
                DoFile(Input, Output, this.POTFile);
            return 0;
        }

        private void DoDirectory(string input, string output, string poDirectory)
        {
            string searchPattern = "*.md";
            foreach (string f in Directory.GetFiles(input, searchPattern))
            {
                string filename = Path.GetFileName(f);
                string outputFile = Path.Combine(output, filename);
                if ((this.POTFile == "" || this.POTFile == null) && poDirectory != null)
                {
                    string extension = this.Action == ACTION_GENERATE_POT ? ".pot" : ".po";
                    string poFile = Path.Combine(poDirectory, Path.ChangeExtension(filename, extension));
                    DoFile(f, outputFile, poFile);
                }
                else
                    DoFile(f, outputFile, this.POTFile);
            }

            foreach (string d in Directory.GetDirectories(input))
            {
                Log($"Scanning {Path.GetRelativePath(Directory.GetCurrentDirectory(), d)}...");
                string dirname = Path.GetFileName(d);
                string outputD = Path.Combine(output, dirname);
                string poD = poDirectory != null ? Path.Combine(poDirectory, dirname) : null;
                DoDirectory(d, outputD, poD);
            }
        }

        private void DoFile(string input, string output, string poFile)
        {
            if (!String.IsNullOrEmpty(FileSuffix))
            {
                if (this.Action != ACTION_TRANSLATE)
                {
                    string outputDir = Path.GetDirectoryName(output);
                    string outputExt = Path.GetExtension(output);
                    output = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(output) + FileSuffix + outputExt);
                }
                if (Path.GetExtension(output) == ".pot")
                    output = Path.ChangeExtension(output, ".po");

                if (poFile != null)
                {
                    string poFileDir = Path.GetDirectoryName(poFile);
                    string poFileExt = Path.GetExtension(poFile);
                    poFile = Path.Combine(poFileDir, Path.GetFileNameWithoutExtension(poFile) + FileSuffix + poFileExt);
                }
            }
            UpdateRelativePaths(input, output);
            switch (Action)
            {
                case ACTION_GENERATE_POT:
                    Log($"Generating .pot from {Path.GetRelativePath(Directory.GetCurrentDirectory(), input)}...");
                    GeneratePOT(input, poFile);
                    break;
                case ACTION_TRANSLATE:
                    if (File.Exists(poFile) || IgnoreMissingPO)
                    {
                        Log($"Translating {Path.GetRelativePath(Directory.GetCurrentDirectory(), input)}...");
                        Translate(input, output, poFile);
                    }
                    else
                    {
                        string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), poFile);
                        Log("Missing " + relativePath + " file. Skipping...");
                    }

                    break;
                default:
                    throw new NotImplementedException("Action: " + EnumUtils.GetEnumMemberAttrValue(Action));
            }
        }

        private void Translate(string inputMarkdown, string outputMarkdown, string inputPO)
        {
            string md = File.ReadAllText(inputMarkdown);
            POCatalog catalog;
            if (!File.Exists(inputPO) && IgnoreMissingPO)
            {
                catalog = new POCatalog();
            }
            else
            {
                string po = File.ReadAllText(inputPO);
                catalog = POT.Load(po);
            }
            TranslationInfo info;
            string relativeToSource = PathUtils.GetRelativePath(outputMarkdown, inputMarkdown, true);
            string translatedMarkdown = POT.Translate(catalog, md, inputMarkdown, relativeToSource, KeepSourceStrings, TrimTranslations, UnescapeEntities, out info);
            Log(string.Format(TRANSLATION_INFO, info.TranslatedCount, info.TotalCount));
            int ratio = info.TotalCount > 0 ? (int)(info.TranslatedCount * 1.0 / info.TotalCount * 100) : 0;
            if (ratio >= MinRatio)
                WriteToOutput(translatedMarkdown, outputMarkdown);
            else
                Console.Error.WriteLine("Skipping write file. Translation ratio is {0}%, below target of {1}%.", ratio, MinRatio);
            if (info.MissingStrings.Count() > 0)
            {
                Console.Error.WriteLine("Missing translations:");
                foreach (string s in info.MissingStrings)
                    Console.Error.WriteLine("  {0}", s);
            }
        }

        private void Log(string message)
        {
            if (this.Output == null || this.Output == "")
                Console.Error.WriteLine(message);
            else
                Console.WriteLine(message);
        }

        private void GeneratePOT(string inputMarkdown, string outputPOT)
        {
            string md = File.ReadAllText(inputMarkdown);
            IEnumerable<StringInfo> strings = Markdown.MarkdownParser.ExtractStrings(md, inputMarkdown);
            Log($"Found {strings.Count()} strings.");
            string pot = AppendPot && File.Exists(outputPOT) ? File.ReadAllText(outputPOT) : "";
            if (pot == "")
            {
                if (strings.Count() > 0)
                {
                    pot = POT.Generate(strings, TranslatorComments);
                    WriteToOutput(pot, outputPOT);
                }
            }
            else
            {
                var catalog = POT.Load(pot);
                string newPOT = POT.Append(catalog, strings, TranslatorComments);
                if (catalog.Count > 0)
                    WriteToOutput(newPOT, outputPOT);
            }
        }

        private void UpdateRelativePaths(string input, string output)
        {
            string relativePathOutput = null;
            if (input != null && output != null && input != "" && output != "")
            {
                relativePathOutput = PathUtils.GetRelativePath(output, input, false);
            }
            MarkdownParser.Options.ImageRelativePath = UpdateImageRelativePaths ? relativePathOutput : null;
            MarkdownParser.Options.LinkRelativePath = UpdateLinksRelativePaths ? relativePathOutput : null;

        }
        private void InitMarkdownParserOptions()
        {

            MarkdownParser.SetParserOptions(new RendererOptions()
            {
                EnableGitHubFlavoredMarkdownTaskLists = GitHubFlavoredMarkdownTaskLists,
                EnableFrontMatter = GitHubFlavoredMarkdownFrontMatter,
                EnablePipeTables = GitHubFlavoredMarkdownTables,
                FrontMatterExclude = FrontMatterExclude,
                SkipImageAlt = IgnoreImageAlt,
                EnableCustomAttributes = CustomAttributes,
                IgnorePatterns = IgnorePatterns,
                OnlyPatterns = OnlyPatterns,
                ParseHtml = ParseHtml,
                FrontMatterSourceKey = FrontMatterSourceKey,
                UpdateFrontMatterLocale = UpdateFrontMatterLocale,
                AddFrontMatterKeys = ParseFrontMatterKeys(),
                KeepLiteralsTogether = KeepLiteralsTogether,
                EnableDefinitionLists = EnableDefinitionLists,
                KeepHtmlTagsTogether = KeepHTMLTagsTogether,
                ReplaceNewLineInsideTable = UseBRInsideTables,
                ReplaceNewLineInsideHeading = UseBRInsideHeadings,
            });
        }

        private Dictionary<string, string> ParseFrontMatterKeys()
        {
            Dictionary<string, string> keys = new Dictionary<string, string>();
            if (AddFrontMatter != null)
            {
                foreach (string s in AddFrontMatter)
                {
                    string[] split = s.Split(':');
                    if (split.Length != 2)
                        throw new Exception("Invalid option: " + s);
                    keys.Add(split[0], split[1]);
                }
            }
            return keys;
        }

        private void WriteToOutput(string pot, string outputFile)
        {
            if (outputFile == null || outputFile == "")
                Console.Write(pot);
            else
            {
                string path = Path.GetDirectoryName(outputFile);
                if (path != "")
                    Directory.CreateDirectory(path);
                Log($"Writing to {outputFile}...");
                File.WriteAllText(outputFile, pot);
            }
        }
    }
}