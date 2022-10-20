using Google.Api.Gax.ResourceNames;
using Google.Apis.Services;
using Google.Cloud.Translate.V3;

namespace Google;
public class GoogleTranslate
{
    private const string CREDENTIALS_VAR = "GOOGLE_APPLICATION_CREDENTIALS";
    private static TranslationServiceClient TRANSLATION_CLIENT;
    private static string PROJECT_ID;

    public static string Translate(string s, string targetLocale)
    {
        CheckCredentials();
        TranslateTextRequest request = new TranslateTextRequest
        {
            SourceLanguageCode = "en-US",
            Contents = { s },
            TargetLanguageCode = targetLocale,
            Parent = new ProjectName(PROJECT_ID).ToString()
        };
        TranslateTextResponse response = TRANSLATION_CLIENT.TranslateText(request);
        // response.Translations will have one entry, because request.Contents has one entry.
        Translation translation = response.Translations[0];

        return translation.TranslatedText;
    }

    private static void CheckCredentials()
    {
        string? path = Environment.GetEnvironmentVariable(CREDENTIALS_VAR);
        if (path == null || !File.Exists(path))
            throw new Exception("Missing Google credentials.");
        if (PROJECT_ID == null || PROJECT_ID == "")
            throw new Exception("Missing Project ID.");
    }

    public static void InitCredentials(string path, string projectId)
    {
        Environment.SetEnvironmentVariable(CREDENTIALS_VAR, path);
        TRANSLATION_CLIENT = TranslationServiceClient.Create();
        PROJECT_ID = projectId;
        CheckCredentials();
    }
}
