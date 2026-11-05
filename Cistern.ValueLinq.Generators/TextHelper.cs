namespace Cistern.ValueLinq.Generators;

static class TextHelper
{
    // generated source is the same text on every build host, so the line ending is fixed rather than the host's
    public const string NewLine = "\r\n";

    // the templates carry the line endings of the generator's own source files, so the finished text is normalised once
    public static string WithNewLines(string text)
    {
        var normalised =
            text
            .Replace("\r\n", "\n")
            .Replace("\n", NewLine);
        return normalised;
    }
}
