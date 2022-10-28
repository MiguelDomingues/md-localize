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


        private int OnExecute()
        {
            InitMarkdownParserOptions();
            if (GoogleTranslateCredentials != null)
                Google.GoogleTranslate.InitCredentials(GoogleTranslateCredentials, GoogleProjectId);
            if (File.GetAttributes(Input).HasFlag(FileAttributes.Directory))
            {
                DoDirectory(Input, Output);
            }
            else
                DoFile(Input, Output);
            return 0;
        }

        private void DoDirectory(string input, string output)
        {
            foreach (string f in Directory.GetFiles(input, "*.md"))
            {
                string filename = Path.GetFileName(f);
                string outputFile = Path.Combine(output, filename);
                DoFile(f, outputFile);
            }

            foreach (string d in Directory.GetDirectories(input))
            {
                Log($"Scanning {Path.GetRelativePath(Directory.GetCurrentDirectory(), d)}...");
                string dirname = Path.GetFileName(d);
                string outputD = Path.Combine(output, dirname);
                DoDirectory(d, outputD);
            }
        }

        private void DoFile(string input, string output)
        {
            switch (Action)
            {
                case ACTION_GENERATE_POT:
                    Log($"Generating .pot from {Path.GetRelativePath(Directory.GetCurrentDirectory(), input)}...");
                    GeneratePOT(input, this.POTFile);
                    break;
                case ACTION_TRANSLATE:
                    Log($"Translating {Path.GetRelativePath(Directory.GetCurrentDirectory(), input)}...");
                    Translate(input, output, POTFile);
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
            string translatedMarkdown = POT.Translate(catalog, md, inputMarkdown, out info);
            Log(string.Format(TRANSLATION_INFO, info.TranslatedCount, info.TotalCount));
            int ratio = (int)(info.TranslatedCount * 1.0 / info.TotalCount * 100);
            if (ratio >= MinRatio)
                WriteToOutput(translatedMarkdown, outputMarkdown);
            else
            {
                Console.Error.WriteLine("Skipping write file. Translation ratio is {0}%, below target of {1}%.", ratio, MinRatio);
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
            catalog.Language = Locale;
            foreach (IPOEntry e in catalog.Values)
            {
                switch (e)
                {
                    case POSingularEntry s:
                        Log($"Translating {s.Key.Id}");
                        s.Translation = Google.GoogleTranslate.Translate(s.Key.Id, Locale);
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
                Console.Error.Write(message);
            else
                Console.WriteLine(message);
        }

        private void GeneratePOT(string inputMarkdown, string outputPOT)
        {
            string md = File.ReadAllText(inputMarkdown);
            IEnumerable<StringInfo> strings = Markdown.MarkdownParser.ExtractStrings(md, inputMarkdown);
            Log($"Found {strings.Count()} strings.");
            string pot = File.Exists(outputPOT) ? File.ReadAllText(outputPOT) : "";
            if (pot == "")
            {
                pot = POT.Generate(strings, TranslatorComments);
                WriteToOutput(pot, outputPOT);
            }
            else
            {
                var catalog = POT.Load(pot);
                string newPOT = POT.Append(catalog, strings, TranslatorComments);
                WriteToOutput(newPOT, outputPOT);
            }
        }

        private void InitMarkdownParserOptions()
        {
            string relativePathOutput = null;
            if (Input != null && Output != null && Input != "" && Output != "")
            {
                relativePathOutput = Path.GetRelativePath(Input, Output);
            }

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
                ImageRelativePath = UpdateImageRelativePaths ? relativePathOutput : null,
                LinkRelativePath = UpdateLinksRelativePaths ? relativePathOutput : null,
            });
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