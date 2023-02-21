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
        private const string ACTION_GOOGLE_TRANSLATE = "google-translate";
        public const string TRANSLATION_INFO = "Translated {0} out of {1} strings.";

        public static int Main(string[] args)
        {
            return CommandLineApplication.Execute<Program>(args);
        }

        [Option("--action|-a", "The action to perform.", CommandOptionType.SingleValue)]
        [AllowedValues(ACTION_GENERATE_POT, ACTION_TRANSLATE, ACTION_GOOGLE_TRANSLATE, IgnoreCase = true)]
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

        [Option("--google-translate-credentials|-gtc", "Path to Google translate .json credentials file.", CommandOptionType.SingleValue)]
        public string GoogleTranslateCredentials { get; } = null;

        [Option("--google-project-id|-gid", "Google Cloud console Project ID.", CommandOptionType.SingleValue)]
        public string GoogleProjectId { get; } = null;

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

        private int OnExecute()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            InitMarkdownParserOptions();
            if (GoogleTranslateCredentials != null)
                Google.GoogleTranslate.InitCredentials(GoogleTranslateCredentials, GoogleProjectId);
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
            foreach (string f in Directory.GetFiles(input, "*.md"))
            {
                string filename = Path.GetFileName(f);
                string outputFile = Path.Combine(output, filename);
                if (this.POTFile == "" || this.POTFile == null)
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

                string poFileDir = Path.GetDirectoryName(poFile);
                string poFileExt = Path.GetExtension(poFile);
                poFile = Path.Combine(poFileDir, Path.GetFileNameWithoutExtension(poFile) + FileSuffix + poFileExt);
            }
            UpdateRelativePaths(input, output);
            switch (Action)
            {
                case ACTION_GENERATE_POT:
                    Log($"Generating .pot from {Path.GetRelativePath(Directory.GetCurrentDirectory(), input)}...");
                    GeneratePOT(input, poFile);
                    break;
                case ACTION_TRANSLATE:
                    if (File.Exists(poFile))
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
                case ACTION_GOOGLE_TRANSLATE:
                    Log($"Google translating {Path.GetRelativePath(Directory.GetCurrentDirectory(), input)} to {Locale}");
                    GoogleTranslate(input, output);
                    break;
                default:
                    throw new NotImplementedException("Action: " + EnumUtils.GetEnumMemberAttrValue(Action));
            }
        }

        private void Translate(string inputMarkdown, string outputMarkdown, string inputPO)
        {
            string md = File.ReadAllText(inputMarkdown);
            string po = File.ReadAllText(inputPO);
            var catalog = POT.Load(po);
            TranslationInfo info;
            string relativeToSource = PathUtils.GetRelativePath(outputMarkdown, inputMarkdown, true);
            string translatedMarkdown = POT.Translate(catalog, md, inputMarkdown, relativeToSource, KeepSourceStrings, out info);
            Log(string.Format(TRANSLATION_INFO, info.TranslatedCount, info.TotalCount));
            int ratio = info.TotalCount > 0 ? (int)(info.TranslatedCount * 1.0 / info.TotalCount * 100) : 0;
            if (info.TotalCount > 0)
            {
                if (ratio >= MinRatio)
                    WriteToOutput(translatedMarkdown, outputMarkdown);
                else
                    Console.Error.WriteLine("Skipping write file. Translation ratio is {0}%, below target of {1}%.", ratio, MinRatio);
            }
            else
            {
                Console.Error.WriteLine("Skipping write file. Nothing to translate.");
            }
            if (info.MissingStrings.Count() > 0)
            {
                Console.Error.WriteLine("Missing translations:");
                foreach (string s in info.MissingStrings)
                    Console.Error.WriteLine("  {0}", s);
            }
        }

        private void GoogleTranslate(string inputPOT, string outputPO)
        {
            string pot = File.ReadAllText(inputPOT);
            var catalog = POT.Load(pot);
            var outputCatalog = File.Exists(outputPO) ? POT.Load(File.ReadAllText(outputPO)) : null;
            catalog.Language = Locale;
            foreach (IPOEntry e in catalog.Values)
            {
                switch (e)
                {
                    case POSingularEntry s:
                        if (outputCatalog != null && !String.IsNullOrEmpty(outputCatalog.GetTranslation(s.Key)))
                        {
                            s.Translation = outputCatalog.GetTranslation(s.Key);
                            Log($"Reusing translation {s.Key.Id} / " + s.Translation);
                        }
                        else
                        {
                            Log($"Translating {s.Key.Id}");
                            s.Translation = Google.GoogleTranslate.Translate(s.Key.Id, Locale);
                        }
                        break;
                    default:
                        throw new Exception("Unable to translate " + e.GetType());
                }
            }
            WriteToOutput(POT.Write(catalog), outputPO);
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