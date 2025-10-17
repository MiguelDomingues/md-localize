using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;
using LLama.Transformers;

namespace MarkdownLocalize;

public class Translator : IDisposable
{
    private readonly LLamaWeights Model;
    private readonly LLamaContext Context;
    private readonly StatefulExecutorBase Executor;
    private readonly InferenceParams InferenceParams;
    private readonly ChatSession Session;
    private const string PUNCTUATION_CHARS = ".?!";

    public Translator(string modelPath, string targetLanguage)
    {
        var showLLamaCppLogs = true;
        NativeLibraryConfig
           .All
           .WithLogCallback((level, message) =>
           {
               if (showLLamaCppLogs)
                   Console.WriteLine($"[llama {level}]: {message.TrimEnd('\n')}");
           });
        NativeLibraryConfig
            .All
            .WithVulkan()
            .WithAutoFallback();

        // Calling this method forces loading to occur now.
        NativeApi.llama_empty_call();
        showLLamaCppLogs = false;

        var parameters = new ModelParams(modelPath)
        {
            GpuLayerCount = 10
        };
        Model = LLamaWeights.LoadFromFile(parameters);

        Context = Model.CreateContext(parameters);
        Executor = new InteractiveExecutor(Context);
        /*var sysPrompt = "Translate the given text from English to " + targetLanguage + ". "
                 + "Do not reply with any additional text or symbols, notes or explanations. "
                 + "The output must contain only the translated text, and include the same formatting in Markdown as the original text. "
                 + "Be faithful or accurate in translation. "
                 + "Make the translation readable or intelligible. "
                 + "Be elegant or natural in translation. "
                 + "If the text cannot be translated, return the original text as is. "
                 + "Do not translate person's name.";*/
        //var sysPrompt = "You are an expert linguist, specializing in translation. You are able to capture the nuances of the languages you translate. You pay attention to masculine/feminine/plural and proper use of articles and grammar. You always provide natural sounding translations that fully preserve the meaning of the original text. You never provide explanations for your work. You always answer with the translated text and nothing else. The translated text captures the same Markdown styling as the source text. If the source text contains Markdown formatting, apply the same to the translated text.";
        var sysPrompt = $"You are a translator. Translate the given text from English to {targetLanguage}. Be faithful or accurate in translation. Make the translation readable or intelligible. Be elegant or natural in translation. Keeping the same punctuation in the translation, if no period (or similar) is at the end of the input, do not add it. If the text cannot be translated, return the original text as is. Do not translate person's name, and do not add any additional text in the translation. Also, be attentive to any terms that are capitalized, avoid translating these.";

        var chatHistory = new ChatHistory();
        chatHistory.AddMessage(AuthorRole.System, sysPrompt);

        Session = new(Executor, chatHistory);

        Session.WithHistoryTransform(new PromptTemplateTransformer(Model, withAssistant: true));

        Session.WithOutputTransform(new LLamaTransforms.KeywordTextOutputStreamTransform(
                    ["User:", "�"],
                    redundancyLength: 5));

        InferenceParams = new InferenceParams
        {
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = 0.6f
            },

            MaxTokens = -1, // keep generating tokens until the anti prompt is encountered
            AntiPrompts = ["User:"] // model specific end of turn string (or default)
        };

    }

    public string? TranslateText(string text)
    {
        string translated = Prompt(text).Result;
        if (string.IsNullOrEmpty(translated) || text.Trim() == translated.Trim())
            return null;

        // Check and fix ending punctuation
        if (!PUNCTUATION_CHARS.Contains(text[^1]) && PUNCTUATION_CHARS.Contains(translated[^1]))
            translated = translated[..^1];

        return translated;
    }

    private async Task<string> Prompt(string prompt)
    {
        string result = "";
        CancellationTokenSource cts = new CancellationTokenSource();
        int noTextCount = 0;
        await foreach (
            var text
            in Session.ChatAsync(
                new ChatHistory.Message(AuthorRole.User, prompt),
                InferenceParams, cts.Token))
        {

            if (string.IsNullOrWhiteSpace(text))
                noTextCount++;
            else
                noTextCount = 0;

            result += text;

            if (noTextCount > 50)
            {
                cts.Cancel();
                break;
            }
        }

        return result.Trim();
    }

    public void Dispose()
    {
        Context.Dispose();
        Model.Dispose();
    }

}
