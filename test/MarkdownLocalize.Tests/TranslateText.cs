using System.Net;

namespace MarkdownLocalize.Tests;

[Collection("Text Translation")]
public class TranslateText
{
    private const string MODEL_FILE = "Meta-Llama-3.1-8B-Instruct-128k-Q4_0.gguf";
    private const string MODEL_URL = "https://huggingface.co/GPT4All-Community/Meta-Llama-3.1-8B-Instruct-128k-GGUF/resolve/main/Meta-Llama-3.1-8B-Instruct-128k-Q4_0.gguf";

    private static string GetModelFile()
    {
        lock (MODEL_FILE)
        {
            if (File.Exists(MODEL_FILE))
                return MODEL_FILE;

            using var client = new HttpClient();
            using var s = client.GetStreamAsync(MODEL_URL);
            using var fs = new FileStream(MODEL_FILE, FileMode.OpenOrCreate);
            s.Result.CopyTo(fs);
        }
        return MODEL_FILE;
    }

    [Fact]
    public static void Hello()
    {
        string modelPath = GetModelFile();
        using Translator t = new Translator(modelPath, "Portuguese (Portugal)");
        string text = t.TranslateText("Hello, world!");
        Assert.Equal("Olá, mundo!", text);
    }

    [Fact]
    public static void MultipleText()
    {
        string modelPath = GetModelFile();
        using Translator t = new Translator(modelPath, "Portuguese (Portugal)");
        string text = t.TranslateText("Hello, world!");
        Assert.Equal("Olá, mundo!", text);

        text = t.TranslateText("Today the sun is shining.");
        Assert.Equal("Hoje o sol está a brilhar.", text);
    }

    
    [Fact]
    public static void MarkdownText()
    {
        string modelPath = GetModelFile();
        using Translator t = new Translator(modelPath, "Portuguese (Portugal)");

        string text = t.TranslateText("The quick brown **fox** jumps over the _lazy_ dog.");
        Assert.Equal("O rápido **rato** pula sobre o _preguiçoso_ cão.", text);
    }

}
