using System.Net;

namespace MarkdownLocalize.Tests;

[Collection("Text Translation")]
public class TranslateText
{
    private const string MODEL_FILE = "gemma-3-4b-it-q4_0.gguf";
    private const string MODEL_URL = "https://huggingface.co/libretranslate/gemma3/resolve/main/gemma-3-4b-it-q4_0.gguf";

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
        using Translator t = new(modelPath, "pt_PT");
        string? translated = t.TranslateText("Hello, world!");
        Assert.NotNull(translated);
        Assert.Equal("Olá, mundo!", translated);
    }

    [Fact]
    public static void MultipleText()
    {
        string modelPath = GetModelFile();
        using Translator t = new(modelPath, "pt_PT");
        string? translated = t.TranslateText("Hello, world!");
        Assert.NotNull(translated);
        Assert.Equal("Olá, mundo!", translated);

        translated = t.TranslateText("Today the sun is shining.");
        Assert.NotNull(translated);
        Assert.Equal("Hoje o sol está a brilhar.", translated);
    }


    [Fact]
    public static void MarkdownText()
    {
        string modelPath = GetModelFile();
        using Translator t = new(modelPath, "pt_PT");

        string? translated = t.TranslateText("The **sun** is shining.");
        Assert.NotNull(translated);
        Assert.Equal("O **sol** está a brilhar.", translated);
    }

    [Fact]
    public static void QuickFox()
    {
        string modelPath = GetModelFile();
        using Translator t = new(modelPath, "pt_BR");
        string? translated = t.TranslateText("The quick brown fox jumps over the lazy dog.");
        Assert.NotNull(translated);
        Assert.Equal("A raposa marrom rápida pula sobre o cão preguiçoso.", translated);
    }

}
