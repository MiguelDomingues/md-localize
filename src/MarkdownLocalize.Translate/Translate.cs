
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Sampling;

namespace MarkdownLocalize;

public class Translator : IDisposable
{
    private const string INSTRUCTION_PREFIX = "\n\n### Instruction:\n\n";

    private readonly LLamaWeights Model;
    private readonly LLamaContext Context;
    private readonly StatefulExecutorBase Executor;
    private readonly InferenceParams InferenceParams;

    public Translator(string modelPath, string targetLanguage)
    {
        var parameters = new ModelParams(modelPath)
        {
            ContextSize = 2048,
            GpuLayerCount = 32,
            UseMemoryLock = false,
            UseMemorymap = true,
            BatchSize = 2048,
            Threads = 4,

        };
        Model = LLamaWeights.LoadFromFile(parameters);
        Context = Model.CreateContext(parameters);

        var sysPrompt = "Translate the given text from English to " + targetLanguage + ". "
                 + "Do not reply with any additional text or symbols, notes or explanations. "
                 + "The output must contain only the translated text, and include the same formatting in Markdown as the original text. "
                 + "Be faithful or accurate in translation. "
                 + "Make the translation readable or intelligible. "
                 + "Be elegant or natural in translation. "
                 + "If the text cannot be translated, return the original text as is. "
                 + "Do not translate person's name.";
        Executor = new InstructExecutor(Context, instructionPrefix: INSTRUCTION_PREFIX);

        InferenceParams = new InferenceParams
        {
            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Temperature = 0.0f,
                TopK = 50,
                TopP = 0.95f,
                MinP = 0.05f,
                TypicalP = 1.0f,
                RepeatPenalty = 1.1f

            },
        };

        var _ = Prompt(sysPrompt, true).Result;
    }

    public string TranslateText(string text)
    {
        return Prompt(text, false).Result;
    }

    private async Task<string> Prompt(string prompt, bool ignoreOutput)
    {
        string result = "";
        CancellationTokenSource cts = new CancellationTokenSource();
        await foreach (var text in Executor.InferAsync(prompt, InferenceParams, cts.Token))
        {
            if (ignoreOutput | result.Contains("\n\n### "))
            {
                cts.Cancel();
                if (ignoreOutput)
                    return "";
                break;
            }

            result += text;
        }
        int index = result.IndexOf("\n\n### ");
        result = result.Substring(0, index).Trim();
        return result;
    }

    public void Dispose()
    {
        Context.Dispose();
        Model.Dispose();
    }

}
