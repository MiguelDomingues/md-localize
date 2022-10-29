using MarkdownLocalize.CLI;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace MarkdownLocalize.Tests;

public class CLITest
{

    private void HelperCompareOutput(string expected, string actual)
    {
        var lines = actual.Split(Environment.NewLine, StringSplitOptions.TrimEntries);
        var expectedLines = expected.Split(Environment.NewLine, StringSplitOptions.TrimEntries);

        Assert.Equal(expectedLines.Count(), lines.Count());

        for (int i = 0; i < lines.Count(); i++)
            Assert.Equal(expectedLines[i], lines[i]);
    }

    [Fact]
    public void CLIVersion()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);

        int exitCode = MarkdownLocalize.CLI.Program.Main(new[] { "--version " });
        Assert.Equal(1, exitCode);

        var sb = writer.GetStringBuilder();
        var lines = sb.ToString().Split(Environment.NewLine, StringSplitOptions.TrimEntries);
        Assert.Equal("", lines[1]);
        Assert.Equal(2, lines.Count());
    }

    [Theory]
    [InlineData("resources/tasks.md", "resources/tasks.pot")]
    [InlineData("resources/repeated-strings.md", "resources/repeated-strings.pot")]
    public void CLIGeneratePOT(string md, string pot)
    {
        var writer = new StringWriter();
        Console.SetOut(writer);

        int exitCode = MarkdownLocalize.CLI.Program.Main(new[] { "--input", md, "--action", "generate-pot", "--gfm-task-lists" });
        Assert.Equal(0, exitCode);

        HelperCompareOutput(File.ReadAllText(pot), writer.GetStringBuilder().ToString());
    }

    [Theory]
    [InlineData("resources/tasks.md", "resources/tasks.pt-PT.po", "resources/tasks.pt-PT.md")]
    public void CLITranslate(string md, string po, string expectedMD)
    {
        var writer = new StringWriter();
        Console.SetOut(writer);

        int exitCode = MarkdownLocalize.CLI.Program.Main(new[] { "--input", md, "--action", "translate", "-po", po, "--gfm-task-lists" });
        Assert.Equal(0, exitCode);

        HelperCompareOutput(File.ReadAllText(expectedMD), writer.GetStringBuilder().ToString());
    }

    [Fact]
    public void CLIAppendPOT()
    {
        string tempPot = Path.GetTempFileName();

        int exitCode = MarkdownLocalize.CLI.Program.Main(new[] { "--input", "resources/tasks.md", "--action", "generate-pot", "--po-file", tempPot, "--gfm-task-lists" });
        Assert.Equal(0, exitCode);

        HelperCompareOutput(File.ReadAllText(tempPot), File.ReadAllText("resources/tasks.pot"));
    }

    [Fact]
    public void CLIPOTImage()
    {
        string tempPot = Path.GetTempFileName();

        int exitCode = MarkdownLocalize.CLI.Program.Main(new[] { "--input", "resources/image-with-text.md", "--action", "generate-pot", "--po-file", tempPot, "--custom-attributes" });
        Assert.Equal(0, exitCode);

        HelperCompareOutput(File.ReadAllText("resources/image-with-text.pot"), File.ReadAllText(tempPot));
        File.Delete(tempPot);
    }

    [Fact]
    public void CLIAppendPOTImage()
    {
        string tempPot = Path.GetTempFileName();

        int exitCode = MarkdownLocalize.CLI.Program.Main(new[] { "--input", "resources/image-with-text.md", "--action", "generate-pot", "--po-file", tempPot, "--custom-attributes" });
        Assert.Equal(0, exitCode);

        HelperCompareOutput(File.ReadAllText(tempPot), File.ReadAllText("resources/image-with-text.pot"));

        exitCode = MarkdownLocalize.CLI.Program.Main(new[] { "--input", "resources/image-with-text.md", "--action", "generate-pot", "--po-file", tempPot, "--custom-attributes" });
        Assert.Equal(0, exitCode);

        HelperCompareOutput(File.ReadAllText("resources/image-with-text.pot"), File.ReadAllText(tempPot));
        File.Delete(tempPot);
    }

    [Theory]
    [InlineData("resources/front-matter.md", "resources/front-matter-with-comments.pot")]
    public void CLIGeneratePOTExtraComments(string md, string pot)
    {
        var writer = new StringWriter();
        Console.SetOut(writer);

        int exitCode = MarkdownLocalize.CLI.Program.Main(new[] { "--input", md, "--action", "generate-pot", "--gfm-front-matter", "--gfm-front-matter-exclude", "theme", "--markdown-translator-comment", "an extra comment" });
        Assert.Equal(0, exitCode);

        HelperCompareOutput(File.ReadAllText(pot), writer.GetStringBuilder().ToString());
    }

    [Fact]
    public void PathRewrite()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);

        int exitCode = MarkdownLocalize.CLI.Program.Main(new[] { "--input", "resources/front-matter.md", "--action", "translate", "--output", "some-file.md", "--po-file", "resources/front-matter-with-comments.pot", "--gfm-front-matter", "--gfm-front-matter-exclude", "theme", "--markdown-translator-comment", "an extra comment" });
        Assert.Equal(0, exitCode);

        HelperCompareOutput("aaa", writer.GetStringBuilder().ToString());
    }

}
