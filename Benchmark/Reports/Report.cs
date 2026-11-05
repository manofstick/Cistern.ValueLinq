#:property ImplicitUsings=disable
// Runs the Parity or Toub benchmark suite into a new run directory and writes its report there, or rewrites a report
// from a run directory:
//
//   dotnet run Report.cs run parity|toub <run directory> [<extra benchmark arguments>...]
//   dotnet run Report.cs parity <run directory> [<output .html>]
//   dotnet run Report.cs toub <run directory> [<output .html>]
//   dotnet run Report.cs progress <run directory>
//
// A run takes the Release build of the benchmark project, so build that first, and passes on its environment: the exe
// needs DOTNET_ROOT to name an SDK with the runtime it targets. Extra arguments are added after the suite's own; a
// --filter among them replaces the suite's, so a run can take part of it. The run directory must not exist yet; it gets
//
//   artifacts/results/*-report.csv   BenchmarkDotNet's results, one file per class
//   run.log                          the benchmark exe's console output
//   command.txt                      the command line the run was started with
//   finished.txt                     its exit code and end time, written once it has ended
//   report.html                      the report, unless another file is named
//
// A report can be written while the run is going; "progress" prints only how far it has got. Anything in the results a
// report does not recognise stops it with a message naming it, rather than leaving it out. The Parity report has one
// row per entry and parameter set, Cistern against System.Linq, ZLinq and the hand-coded loop, its variants listed
// beside it; the Toub report one row per entry, Cistern and its variants against the same three, with the labels in
// ToubLabels.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

try
{
    int exitCode =
        args switch
        {
            ["run", var suite, var runDirectory, .. var extra] => Runner.Run(suite, runDirectory, extra),
            ["parity", var runDirectory] => ParityReport.Write(runDirectory, null),
            ["parity", var runDirectory, var output] => ParityReport.Write(runDirectory, output),
            ["toub", var runDirectory] => ToubReport.Write(runDirectory, null),
            ["toub", var runDirectory, var output] => ToubReport.Write(runDirectory, output),
            ["progress", var runDirectory] => progress(runDirectory),
            _ => throw new Failure("usage: Report.cs run parity|toub <run directory> [<extra benchmark arguments>...] | parity <run directory> [<output .html>] | toub <run directory> [<output .html>] | progress <run directory>"),
        };

    return exitCode;
}
catch (Failure failure)
{
    Console.Error.WriteLine($"error: {failure.Message}");

    return 1;
}

static int progress(string runDirectory)
{
    string progress =
        Results.Progress(runDirectory);
    Console.WriteLine(progress);

    return 0;
}

sealed class Failure(string message)
    : Exception(message);

static class Runner
{
    static readonly Dictionary<string, (string Filter, string[] Arguments, Func<string, string?, int> Report)> Suites = new()
    {
        ["parity"] = ("Cistern.Benchmarks.Parity.*", ["--job", "short"], ParityReport.Write),
        ["toub"] = ("Cistern.Benchmarks.Toub.*", ["--job", "short", "--iterationCount", "20", "--warmupCount", "5"], ToubReport.Write),
    };

    public static int Run(string suite, string runDirectory, string[] extra)
    {
        if (!Suites.TryGetValue(suite, out var chosen))
            throw new Failure($"usage: Report.cs run {string.Join("|", Suites.Keys)} <run directory> [<extra benchmark arguments>...]");

        runDirectory =
            Path.GetFullPath(runDirectory);
        if (Path.Exists(runDirectory))
            throw new Failure($"{runDirectory} exists; a run goes into a new directory");

        string scriptDirectory =
            ScriptDirectory();
        string benchmark =
            Path.GetDirectoryName(scriptDirectory)!;
        string exeName =
            OperatingSystem.IsWindows()
                ? "Cistern.Benchmarks.exe"
                : "Cistern.Benchmarks";
        string exe =
            Path.Combine(benchmark, "bin", "Release", "net11.0", exeName);
        if (!File.Exists(exe))
            throw new Failure($"{exe} not found; build the benchmark project in Release first");

        bool filtered =
            extra.Any(argument => argument is "--filter" or "-f");
        string[] filter =
            filtered
                ? []
                : ["--filter", chosen.Filter];
        var arguments =
            filter
            .Concat(chosen.Arguments)
            .Concat(extra)
            .ToList();

        Directory.CreateDirectory(runDirectory);

        string commandPath =
            Path.Combine(runDirectory, "command.txt");
        var commandArguments =
            arguments.Prepend(exeName);
        string command =
            CommandLine(commandArguments) + "\n";
        File.WriteAllText(commandPath, command);

        string artifacts =
            Path.Combine(runDirectory, "artifacts");
        arguments.Add("--artifacts");
        arguments.Add(artifacts);

        // BenchmarkDotNet finds the benchmark project by searching the tree above its working directory.
        string logPath =
            Path.Combine(runDirectory, "run.log");
        int code =
            Execute(exe, arguments, benchmark, logPath);

        var now = DateTimeOffset.Now;
        string sign =
            now.Offset < TimeSpan.Zero
                ? "-"
                : "+";
        string hoursMinutes =
            now.Offset.ToString("hhmm");
        string finishedPath =
            Path.Combine(runDirectory, "finished.txt");
        string finished = $"exit={code} {now:yyyy-MM-dd HH:mm:ss} {sign}{hoursMinutes}\n";
        File.WriteAllText(finishedPath, finished);

        chosen.Report(runDirectory, null);

        return code;
    }

    static int Execute(string exe, List<string> arguments, string workingDirectory, string logPath)
    {
        var info =
            new ProcessStartInfo(exe)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
        foreach (var argument in arguments)
            info.ArgumentList.Add(argument);

        var encoding =
            new UTF8Encoding(false);
        using var log =
            new StreamWriter(logPath, false, encoding);
        using var process =
            new Process { StartInfo = info };
        process.OutputDataReceived += append;
        process.ErrorDataReceived += append;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return process.ExitCode;

        void append(object sender, DataReceivedEventArgs received)
        {
            if (received.Data is not string line)
                return;

            lock (log)
                log.WriteLine(line);
        }
    }

    static string CommandLine(IEnumerable<string> arguments)
    {
        var quotedArguments =
            arguments.Select(quoted);
        string line =
            string.Join(" ", quotedArguments);

        return line;

        static string quoted(string argument)
        {
            bool bare =
                argument.Length > 0
                && !argument.Any(character => character is ' ' or '\t' or '"');
            string quotedArgument =
                bare
                    ? argument
                    : "\"" + argument.Replace("\"", "\\\"") + "\"";

            return quotedArgument;
        }
    }

    static string ScriptDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
}

// One CSV row: methods are named <entry>_<library>, and a class's parameter columns ([Params]) sit between
// BenchmarkDotNet's last job column, WarmupCount, and Mean.
sealed record Row(string Class, string Entry, string Library, (string Name, string Value)[] Parameters, double? Mean, double? Alloc, string Runtime);

// The rows of one entry and parameter set, keyed by library.
sealed class Case(string className, string entry, (string Name, string Value)[] parameters)
{
    public string Class { get; } = className;
    public string Entry { get; } = entry;
    public (string Name, string Value)[] Parameters { get; } = parameters;
    public Dictionary<string, (double? Mean, double? Alloc)> Libraries { get; } = new();

    public (double? Mean, double? Alloc) this[string library] => Libraries.GetValueOrDefault(library);

    public string Value(string parameterName)
    {
        var found =
            Parameters.FirstOrDefault(parameter => parameter.Name == parameterName);
        string value = found.Value ?? "";

        return value;
    }

    // What a parameter set sorts and groups by: the values compared as text, as BenchmarkDotNet wrote them.
    public string Key { get; } =
        string.Join("\u0001", parameters.Select(parameter => parameter.Name + "\u0002" + parameter.Value));
}

// What the reports share: reading a run directory, the HTML pieces every report uses.
static class Results
{
    static readonly Dictionary<string, double> TimeUnits = new()
    {
        ["ns"] = 1,
        ["us"] = 1e3,
        ["μs"] = 1e3,
        ["µs"] = 1e3,
        ["ms"] = 1e6,
        ["s"] = 1e9,
    };
    static readonly Dictionary<string, double> ByteUnits = new()
    {
        ["B"] = 1,
        ["KB"] = 1024,
        ["MB"] = 1024 * 1024,
        ["GB"] = 1024 * 1024 * 1024,
    };
    static readonly Regex Quantity = new(@"^([\d,\.]+)\s*(\S+)");
    static readonly Regex Remained = new(@"// \*\* Remained (\d+) \(([\d\.]+)%\) benchmark\(s\) to run\. Estimated finish ([^*]+)\*\*");
    static readonly Regex ParityLine = new(@"^PARITY [^\r\n]*", RegexOptions.Multiline);
    static readonly Regex MismatchLine = new(@"^[^\r\n]*MISMATCH[^\r\n]*", RegexOptions.Multiline);

    public static readonly string[] Bands = ["lose2", "lose", "level", "win"];

    public static string DefaultOutput(string runDirectory) => Path.Combine(runDirectory, "report.html");

    public static string Escape(string text)
    {
        string escaped =
            text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;");

        return escaped;
    }

    // A number as the page's script reads it back: the shortest round-trip form, with ".0" on an integral value.
    static string Sortable(double value)
    {
        string text =
            value
            .ToString("R", CultureInfo.InvariantCulture)
            .Replace("E", "e");
        bool fractional =
            text.Contains('.')
            || text.Contains('e');
        string sortable =
            fractional
                ? text
                : text + ".0";

        return sortable;
    }

    static string ResultsDirectory(string runDirectory) => Path.Combine(runDirectory, "artifacts", "results");

    static string? ReadText(string runDirectory, string fileName)
    {
        string path =
            Path.Combine(runDirectory, fileName);
        if (!File.Exists(path))
            return null;

        string text =
            File.ReadAllText(path);

        return text;
    }

    static string OneLine(string text)
    {
        string line =
            text
            .Trim()
            .Replace("\n", " ");

        return line;
    }

    static double? TimeNs(string text) => Scaled(text, TimeUnits);

    static double? BytesOf(string text)
    {
        if (text is "" or "-")
            return 0;

        double? bytes =
            Scaled(text, ByteUnits);

        return bytes;
    }

    static double? Scaled(string text, Dictionary<string, double> units)
    {
        var match =
            Quantity.Match(text);
        if (!match.Success
            || !units.TryGetValue(match.Groups[2].Value, out double scale))
        {
            return null;
        }

        string digits =
            match.Groups[1].Value.Replace(",", "");
        double scaled =
            double.Parse(digits) * scale;

        return scaled;
    }

    // Stops the report on any name in the list, naming them all.
    public static void FailIfAny(IEnumerable<string> names, string message)
    {
        var sorted =
            names
            .Distinct()
            .Order(StringComparer.Ordinal);
        string text =
            string.Join(", ", sorted);
        if (text != "")
            throw new Failure($"{message}: {text}");
    }

    public static List<Row> Load(string runDirectory, string classNamespace)
    {
        string directory =
            ResultsDirectory(runDirectory);
        if (!Directory.Exists(directory))
            throw new Failure($"no *-report.csv under {directory}");

        var paths =
            Directory
            .GetFiles(directory, "*-report.csv")
            .Order(StringComparer.Ordinal)
            .ToList();
        if (paths is [])
            throw new Failure($"no *-report.csv under {directory}");

        string prefix = classNamespace + ".";
        var rows =
            new List<Row>();
        foreach (var path in paths)
        {
            string fileName =
                Path.GetFileName(path);
            string name = fileName[..^"-report.csv".Length];
            if (!name.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            string className = name[prefix.Length..];
            var lines =
                ReadCsv(path);
            var header = lines[0];
            int warmupCount =
                Array.IndexOf(header, "WarmupCount");
            int mean =
                Array.IndexOf(header, "Mean");
            if (warmupCount < 0
                || mean < 0)
            {
                throw new Failure($"{path}: no WarmupCount or Mean column, so its parameter columns cannot be found");
            }

            var parameterNames = header[(warmupCount + 1)..mean];
            foreach (var values in lines.Skip(1))
            {
                var fields =
                    header
                    .Zip(values)
                    .ToDictionary(
                        pair => pair.First,
                        pair => pair.Second
                    );
                string method = fields["Method"];
                int underscore =
                    method.IndexOf('_');
                if (underscore <= 0
                    || underscore == method.Length - 1)
                {
                    throw new Failure($"{path}: method {method} is not named <entry>_<library>");
                }

                if (!fields.TryGetValue("Allocated", out string? allocated))
                    throw new Failure($"{path}: no Allocated column");

                string entry = method[..underscore];
                string library = method[(underscore + 1)..];
                var parameters =
                    parameterNames
                    .Select(parameterName => (parameterName, fields[parameterName]))
                    .ToArray();
                double? time =
                    TimeNs(fields["Mean"]);
                double? alloc =
                    BytesOf(allocated);
                string runtime =
                    fields.GetValueOrDefault("Runtime", "");

                var row =
                    new Row(className, entry, library, parameters, time, alloc, runtime);
                rows.Add(row);
            }
        }

        if (rows is [])
            throw new Failure($"no results for {classNamespace}.* under {directory}");

        return rows;
    }

    static List<string[]> ReadCsv(string path)
    {
        string text =
            File.ReadAllText(path);
        var lines =
            new List<string[]>();
        var fields =
            new List<string>();
        var field =
            new StringBuilder();
        bool quoted = false;
        for (int i = 0; i < text.Length; ++i)
        {
            char character = text[i];
            if (quoted)
            {
                if (character != '"')
                    field.Append(character);
                else if (i + 1 < text.Length
                    && text[i + 1] == '"')
                {
                    field.Append('"');
                    ++i;
                }
                else
                    quoted = false;
            }
            else if (character == '"')
                quoted = true;
            else if (character == ',')
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else if (character == '\n')
            {
                fields.Add(field.ToString());
                field.Clear();
                lines.Add(fields.ToArray());
                fields.Clear();
            }
            else if (character != '\r')
                field.Append(character);
        }

        if (field.Length > 0
            || fields.Count > 0)
        {
            fields.Add(field.ToString());
            lines.Add(fields.ToArray());
        }

        return lines;
    }

    public static List<string> ParameterNames(List<Row> rows)
    {
        var names =
            rows
            .SelectMany(row => row.Parameters)
            .Select(parameter => parameter.Name)
            .Distinct()
            .ToList();

        return names;
    }

    public static List<Case> Cases(List<Row> rows)
    {
        var cases =
            new Dictionary<(string, string, string), Case>();
        foreach (var result in rows)
        {
            var candidate =
                new Case(result.Class, result.Entry, result.Parameters);
            var key = (candidate.Class, candidate.Entry, candidate.Key);
            if (!cases.TryGetValue(key, out var existing))
            {
                existing = candidate;
                cases.Add(key, existing);
            }

            existing.Libraries[result.Library] = (result.Mean, result.Alloc);
        }

        var sorted =
            cases.Values
            .OrderBy(entry => entry.Class, StringComparer.Ordinal)
            .ThenBy(entry => entry.Entry, StringComparer.Ordinal)
            .ThenBy(entry => entry.Key, StringComparer.Ordinal)
            .ToList();

        return sorted;
    }

    // The run as recorded: the command it was started with, the runtimes the results name, and how it ended.
    public static string RunDescription(string runDirectory, List<Row> rows)
    {
        string? command =
            ReadText(runDirectory, "command.txt");
        string commandPart =
            command switch
            {
                null => "Command not recorded",
                { } recorded => $"Command: <code>{Escape(recorded.Trim())}</code>",
            };

        var runtimes =
            rows
            .Select(row => row.Runtime)
            .Where(runtime => runtime != "")
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToList();
        string runtimePart =
            runtimes switch
            {
                [] => "runtime not recorded",
                [_, ..] named => $"runtime {Escape(string.Join(", ", named))}",
            };

        string? finished =
            ReadText(runDirectory, "finished.txt");
        string finishedPart =
            finished switch
            {
                null => "not finished",
                { } ended => Escape("finished: " + OneLine(ended)),
            };

        string[] parts = [commandPart, runtimePart, finishedPart];
        string description =
            string.Join(". ", parts) + ".";

        return description;
    }

    public static string Progress(string runDirectory)
    {
        string? finished =
            ReadText(runDirectory, "finished.txt");
        if (finished is not null)
            return "finished: " + OneLine(finished);

        string? log =
            ReadText(runDirectory, "run.log");
        if (log is null)
            return "run.log not found";

        var remained =
            Remained.Matches(log);
        if (remained is [])
            return "running, no estimate yet (sanity checks or first case)";

        var last = remained[^1];
        string progress = $"{last.Groups[1].Value} cases remaining ({last.Groups[2].Value}% left), BenchmarkDotNet estimates finish {last.Groups[3].Value.Trim()}";

        return progress;
    }

    static string SanitySections(string runDirectory)
    {
        string log =
            ReadText(runDirectory, "run.log")
            ?? "";
        var parity =
            ParityLine
            .Matches(log)
            .Select(match => match.Value)
            .Distinct()
            .Order(StringComparer.Ordinal);
        var mismatch =
            MismatchLine
            .Matches(log)
            .Select(match => match.Value)
            .Distinct()
            .Order(StringComparer.Ordinal);
        string sections = $"""
            <h2>Sanity check: user code called more often by Cistern</h2>
            <pre>{lines(parity)}</pre>
            <h2>Sanity check mismatches</h2>
            <pre>{lines(mismatch)}</pre>
            """;

        return sections;

        static string lines(IEnumerable<string> matched)
        {
            string joined =
                string.Join("\n", matched);
            string text =
                Escape(joined);
            string shown =
                text == ""
                    ? "none"
                    : text;

            return shown;
        }
    }

    public static string Heads(IEnumerable<string> columns)
    {
        var cells =
            columns.Select(column => $"<th>{Escape(column)}</th>");
        string heads =
            string.Concat(cells);

        return heads;
    }

    public static string Text(string value, string cssClass = "")
    {
        string display =
            Escape(value);
        string cell =
            Td(value, display, cssClass);

        return cell;
    }

    public static string Cell(string? value, string display, string cssClass = "")
    {
        string escaped =
            Escape(display);
        string cell =
            Td(value ?? "", escaped, cssClass);

        return cell;
    }

    public static string Cell(double? value, string display, string cssClass = "")
    {
        string sort =
            value switch
            {
                null => "",
                double number => Sortable(number),
            };
        string escaped =
            Escape(display);
        string cell =
            Td(sort, escaped, cssClass);

        return cell;
    }

    static string Td(string sort, string display, string cssClass)
    {
        string classAttribute =
            cssClass == ""
                ? ""
                : $" class=\"{cssClass}\"";
        string escaped =
            Escape(sort);
        string cell = $"<td data-v=\"{escaped}\"{classAttribute}>{display}</td>";

        return cell;
    }

    public static string Verdict(double? ratio) =>
        ratio switch
        {
            null => "",
            >= 2 => "lose2",
            >= 1.1 => "lose",
            <= 0.9 => "win",
            _ => "level",
        };

    public static Dictionary<string, int> NewCounts()
    {
        var counts =
            Bands.ToDictionary(
                band => band,
                _ => 0
            );

        return counts;
    }

    const string Style = """
        body { font: 13px system-ui, sans-serif; margin: 16px; background: #fff; color: #111; }
        table { border-collapse: collapse; }
        th, td { border: 1px solid #ccc; padding: 3px 8px; text-align: right; white-space: nowrap; }
        th { cursor: pointer; background: #eee; position: sticky; top: 0; }
        th.asc::after { content: " ▲"; } th.desc::after { content: " ▼"; }
        .lose2 { background: #f6c6c6; } .lose { background: #fbe5c8; } .win { background: #cdeccd; }
        input { margin: 8px 0; width: 300px; }
        pre { background: #f5f5f5; padding: 8px; }
        """;

    const string Script = """
        const table = document.getElementById('t');
        const tbody = table.tBodies[0];
        table.querySelectorAll('th').forEach((th, i) => th.addEventListener('click', () => {
          const asc = !th.classList.contains('asc');
          table.querySelectorAll('th').forEach(h => h.classList.remove('asc', 'desc'));
          th.classList.add(asc ? 'asc' : 'desc');
          const rows = Array.from(tbody.rows);
          rows.sort((a, b) => {
            const x = a.cells[i].dataset.v, y = b.cells[i].dataset.v;
            const nx = parseFloat(x), ny = parseFloat(y);
            let c;
            if (x === '' && y === '') c = 0; else if (x === '') c = 1; else if (y === '') c = -1;
            else if (!isNaN(nx) && !isNaN(ny)) c = nx - ny;
            else c = x.localeCompare(y, undefined, { numeric: true });
            return asc ? c : -c;
          });
          rows.forEach(r => tbody.appendChild(r));
        }));
        document.getElementById('filter').addEventListener('input', e => {
          const q = e.target.value.toLowerCase();
          Array.from(tbody.rows).forEach(r => r.style.display = r.textContent.toLowerCase().includes(q) ? '' : 'none');
        });
        """;

    // The page around a report's table: its title, the style and the extra style rules, the paragraphs before the
    // table, its heads and its rows, then the sanity sections and the sorting script.
    public static void WritePage(string path, string runDirectory, string title, string extraStyle, string before, string heads, List<string> body)
    {
        string rows =
            string.Join("\n", body);
        string sanity =
            SanitySections(runDirectory);
        string page = $$"""
            <!doctype html>
            <html><head><meta charset="utf-8"><title>{{title}}</title>
            <style>
            {{Style}}
            {{extraStyle}}
            </style></head><body>
            {{before}}
            <input id="filter" placeholder="filter rows (any text)">
            <table id="t"><thead><tr>
            {{heads}}
            </tr></thead><tbody>
            {{rows}}
            </tbody></table>
            {{sanity}}
            <script>
            {{Script}}
            </script>
            </body></html>

            """;
        string text =
            page.ReplaceLineEndings("\n");
        var encoding =
            new UTF8Encoding(false);
        File.WriteAllText(path, text, encoding);
    }
}

static class ParityReport
{
    const string Cistern = "CisternValueLinq";
    // the versions Cistern is compared against, as (method suffix, column label)
    static readonly (string Suffix, string Label)[] Others = [("Linq", "System.Linq"), ("ZLinq", "ZLinq"), ("Handcoded", "Hand-coded")];
    static readonly (string Unit, double Scale)[] TimeUnits = [("s", 1e9), ("ms", 1e6), ("µs", 1e3)];

    static string FormatTime(double? maybeNanoseconds)
    {
        if (maybeNanoseconds is not double nanoseconds)
            return "";

        foreach (var (unit, scale) in TimeUnits)
        {
            if (nanoseconds >= scale)
            {
                string scaled = $"{nanoseconds / scale:N2} {unit}";

                return scaled;
            }
        }

        string formatted = $"{nanoseconds:N2} ns";

        return formatted;
    }

    static string FormatBytes(double? maybeBytes)
    {
        if (maybeBytes is not double byteCount)
            return "";

        string formatted =
            byteCount >= 1024
                ? $"{byteCount / 1024:N1} KB"
                : $"{byteCount:N0} B";

        return formatted;
    }

    static double? TimeRatio(double? cistern, double? other)
    {
        double? ratio =
            (cistern, other) switch
            {
                (> 0, > 0) => cistern / other,
                _ => null,
            };

        return ratio;
    }

    static double? AllocRatio(double? cistern, double? other)
    {
        double? ratio =
            (cistern, other) switch
            {
                (double mine, double theirs and > 0) => mine / theirs,
                (0, double) => 1.0,
                (double, double) => double.PositiveInfinity,
                _ => null,
            };

        return ratio;
    }

    public static int Write(string runDirectory, string? output)
    {
        string path =
            output
            ?? Results.DefaultOutput(runDirectory);
        var rows =
            Results.Load(runDirectory, "Cistern.Benchmarks.Parity");
        var known =
            Others
            .Select(other => other.Suffix)
            .ToHashSet();
        var unknown =
            rows
            .Where(row => !known.Contains(row.Library) && !row.Library.StartsWith(Cistern, StringComparison.Ordinal))
            .Select(row => $"{row.Class}.{row.Entry}_{row.Library}");
        Results.FailIfAny(unknown, "methods for a library this report has no column for");

        var parameterNames =
            Results.ParameterNames(rows);
        var cases =
            Results.Cases(rows);
        var counts =
            Others.ToDictionary(
                other => other.Suffix,
                _ => Results.NewCounts()
            );
        var body =
            new List<string>();
        foreach (var row in cases)
        {
            var (cisternTime, cisternAlloc) = row[Cistern];
            var cells =
                new StringBuilder("<tr>");
            append(Results.Text(row.Class), Results.Text(row.Entry));
            foreach (var parameterName in parameterNames)
                append(Results.Text(row.Value(parameterName)));
            append(time(cisternTime));
            var variants =
                row.Libraries
                .Where(library =>
                    library.Key.StartsWith(Cistern, StringComparison.Ordinal)
                    && library.Key != Cistern)
                .OrderBy(library => library.Key, StringComparer.Ordinal)
                .Select(library => $"{library.Key[Cistern.Length..]} {FormatTime(library.Value.Mean)}");
            append(Results.Text(string.Join(", ", variants), "variants"));
            foreach (var (suffix, _) in Others)
            {
                double? otherTime = row[suffix].Mean;
                double? vsOther =
                    TimeRatio(cisternTime, otherTime);
                append(time(otherTime), ratio(vsOther, counts[suffix]));
            }

            append(bytes(cisternAlloc));
            foreach (var (suffix, _) in Others)
            {
                double? otherAlloc = row[suffix].Alloc;
                double? vsOther =
                    AllocRatio(cisternAlloc, otherAlloc);
                append(bytes(otherAlloc), ratio(vsOther));
            }

            append("</tr>");
            var html = $"{cells}";
            body.Add(html);

            void append(params ReadOnlySpan<string> columns)
            {
                foreach (var column in columns)
                    cells.Append(column);
            }
        }

        var summaries =
            Others.Select(summaryOf);
        string summary =
            string.Join("; ", summaries);
        var timeHeadings =
            Others.SelectMany(other => new[] { other.Label, $"÷ {other.Label}" });
        var allocHeadings =
            Others.SelectMany(other => new[] { $"{other.Label} alloc", $"÷ {other.Label} alloc" });
        string[] timeColumns = ["Class", "Entry", .. parameterNames, "Cistern", "Cistern variants", .. timeHeadings];
        string[] allocColumns = ["Cistern alloc", .. allocHeadings];
        string heads =
            Results.Heads(timeColumns) + "\n" + Results.Heads(allocColumns);
        int left = 2 + parameterNames.Count;

        string extraStyle = $$"""
            td:nth-child(-n+{{left}}), th:nth-child(-n+{{left}}), td.variants { text-align: left; }
            td.variants { white-space: normal; min-width: 120px; font-size: 12px; color: #444; }
            """;
        string description =
            Results.RunDescription(runDirectory, rows);
        string progress =
            Results.Progress(runDirectory);
        string before = $"""
            <h1>Benchmark/Parity rerun</h1>
            <p>Generated {DateTime.Now:yyyy-MM-dd HH:mm}. {Results.Escape(progress)}.</p>
            <p>{description} Out of process, one process per case. Each ratio is Cistern ÷ the named version,
            for time and for allocation (below 1 is Cistern faster or smaller). Colours: red ≥ 2×, orange ≥ 1.1×, green ≤ 0.9×.
            An empty cell is a version with no method for that entry (ZLinq where its API does not cover it). "Cistern variants"
            are extra Cistern methods an entry carries (an option off by default, such as SIMDOptions.Fastest), not counted in
            the ratios. Entries: {body.Count}.
            Time {Results.Escape(summary)}.</p>
            """;
        Results.WritePage(path, runDirectory, "Parity rerun", extraStyle, before, heads, body);
        Console.WriteLine($"{path}: {body.Count} entries, {rows.Count} rows");

        return 0;

        string summaryOf((string Suffix, string Label) other)
        {
            var bands = counts[other.Suffix];
            string text = $"vs {other.Label}: ≥2× {bands["lose2"]}, 1.1–2× {bands["lose"]}, level {bands["level"]}, Cistern ahead {bands["win"]}";

            return text;
        }

        static string time(double? maybeNanoseconds)
        {
            string formatted =
                FormatTime(maybeNanoseconds);
            string cell =
                Results.Cell(maybeNanoseconds, formatted);

            return cell;
        }

        static string bytes(double? alloc)
        {
            string formatted =
                FormatBytes(alloc);
            string cell =
                Results.Cell(alloc, formatted);

            return cell;
        }

        static string ratio(double? value, Dictionary<string, int>? band = null)
        {
            if (value is not double number)
                return Results.Cell((double?)null, "");

            if (double.IsPositiveInfinity(number))
                return Results.Cell(1e18, "∞", "lose2");

            string verdict =
                Results.Verdict(number);
            if (band is not null)
                band[verdict]++;

            string cell =
                Results.Cell(number, $"{number:F2}×", verdict);

            return cell;
        }
    }
}

static class ToubReport
{
    const string Cistern = "CisternValueLinq";
    static readonly string[] Others = ["Linq", "ZLinq", "Handcoded"];
    static readonly (string Unit, double Scale)[] TimeUnits = [("s", 1e9), ("ms", 1e6), ("µs", 1e3), ("ns", 1.0)];
    static readonly (string Unit, double Scale)[] ByteUnits = [("MB", 1024.0 * 1024), ("KB", 1024.0), ("B", 1.0)];
    static readonly Regex ClassName = new(@"^Net(Core)?(\d+)$");

    sealed class LabelStats
    {
        public int Entries;
        public Dictionary<string, int> Post = Results.NewCounts();
        public Dictionary<string, int> Best = Results.NewCounts();
    }

    // NetCore20 is .NET Core 2.0, Net5 is .NET 5: ordered by release.
    static double ClassOrder(string className)
    {
        var match =
            ClassName.Match(className);
        if (!match.Success)
            throw new Failure($"class {className} is not named Net<version> or NetCore<version>, so it cannot be placed by release");

        int version =
            int.Parse(match.Groups[2].Value);
        double order =
            match.Groups[1].Success
                ? version / 10.0
                : version;

        return order;
    }

    // The unit of the smallest non-zero value in the row, so every value in it is shown in one unit without losing
    // the smallest one's digits.
    static (string Unit, double Scale) RowUnit(IEnumerable<double?> values, (string Unit, double Scale)[] units)
    {
        var present =
            values
            .OfType<double>()
            .Where(value => value != 0)
            .ToList();
        double smallest =
            present is []
                ? 0
                : present.Min();
        foreach (var unit in units)
            if (smallest >= unit.Scale)
                return unit;

        return units[^1];
    }

    static string FormatTime(double? maybeNanoseconds, (string Unit, double Scale) unit)
    {
        if (maybeNanoseconds is not double nanoseconds)
            return "";

        string formatted = $"{nanoseconds / unit.Scale:N2} {unit.Unit}";

        return formatted;
    }

    static string FormatBytes(double? maybeBytes, (string Unit, double Scale) unit)
    {
        if (maybeBytes is not double byteCount)
            return "";

        if (byteCount == 0)
            return $"0 {unit.Unit}";

        string digits =
            unit.Unit == "B"
                ? "N0"
                : "N1";
        string scaled =
            (byteCount / unit.Scale).ToString(digits);
        string formatted = $"{scaled} {unit.Unit}";

        return formatted;
    }

    static void CheckLabels(List<Row> rows)
    {
        var unknown =
            rows
            .Where(row =>
                !row.Library.StartsWith(Cistern, StringComparison.Ordinal)
                && !Others.Contains(row.Library))
            .Select(row => $"{row.Class}.{row.Entry}_{row.Library}");
        Results.FailIfAny(unknown, "methods for a library this report has no column for");

        var entries =
            rows
            .Select(row => (row.Class, row.Entry))
            .ToHashSet();
        var missing =
            entries
            .Where(entry => !ToubLabels.Types.ContainsKey(entry))
            .Select(entry => $"{entry.Class}.{entry.Entry}");
        Results.FailIfAny(missing, "entries with no line in ToubLabels");

        var stale =
            ToubLabels.Types.Keys
            .Where(entry => !entries.Contains(entry))
            .Select(entry => $"{entry.Class}.{entry.Entry}");
        Results.FailIfAny(stale, "ToubLabels lines for entries the run has no results for");

        var defined =
            ToubLabels.Labels
            .Select(label => label.Label)
            .ToHashSet();
        var bad =
            ToubLabels.Types.Values
            .SelectMany(type => type.Labels)
            .Where(label => !defined.Contains(label));
        Results.FailIfAny(bad, "labels used in ToubLabels with no definition in Labels");
    }

    public static int Write(string runDirectory, string? output)
    {
        string path =
            output
            ?? Results.DefaultOutput(runDirectory);
        var rows =
            Results.Load(runDirectory, "Cistern.Benchmarks.Toub");
        CheckLabels(rows);

        var parameterNames =
            Results.ParameterNames(rows);
        var cases =
            Results
            .Cases(rows)
            .OrderBy(entry => ClassOrder(entry.Class))
            .ThenBy(entry => entry.Entry, StringComparer.Ordinal)
            .ThenBy(entry => entry.Key, StringComparer.Ordinal)
            .ToList();
        (string Name, string Label)[] versions =
        [
            ("Cistern", "Cistern vs System.Linq"),
            ("Best", "Best Cistern vs System.Linq"),
            ("ZLinq", "Best Cistern vs ZLinq"),
            ("Handcoded", "Best Cistern vs Hand-coded"),
        ];
        var counts =
            versions.ToDictionary(
                version => version.Name,
                _ => Results.NewCounts()
            );
        var labelStats =
            ToubLabels.Labels.ToDictionary(
                label => label.Label,
                _ => new LabelStats()
            );
        var body =
            new List<string>();
        foreach (var row in cases)
        {
            var cistern = row[Cistern];
            var variants =
                row.Libraries
                .Where(library =>
                    library.Key.StartsWith(Cistern, StringComparison.Ordinal)
                    && library.Key != Cistern)
                .Select(library => (Name: library.Key[Cistern.Length..], Value: library.Value))
                .OrderBy(variant => variant.Name, StringComparer.Ordinal)
                .ToList();
            var present =
                variants
                .Prepend((Name: "post form", Value: cistern))
                .Where(candidate => candidate.Value.Mean is not null)
                .ToList();
            if (present is [])
                throw new Failure($"{row.Class}.{row.Entry} has no Cistern result");

            var (bestName, best) =
                present.MinBy(candidate => candidate.Value.Mean);
            var means =
                row.Libraries.Values.Select(library => library.Mean);
            var timeUnit =
                RowUnit(means, TimeUnits);
            var allocs =
                row.Libraries.Values.Select(library => library.Alloc);
            var byteUnit =
                RowUnit(allocs, ByteUnits);
            var variantTexts =
                variants.Select(variantText);
            string variantsText =
                string.Join(", ", variantTexts);
            var (labels, test) = ToubLabels.Types[(row.Class, row.Entry)];
            var linq = row["Linq"];
            foreach (var label in labels)
            {
                var stats = labelStats[label];
                stats.Entries++;
                count(stats.Post, cistern.Mean);
                count(stats.Best, best.Mean);
            }

            string labelText =
                string.Join(", ", labels);
            var zlinq = row["ZLinq"];
            var handcoded = row["Handcoded"];
            string bestTime =
                FormatTime(best.Mean, timeUnit);
            string bestSuffix =
                bestName == "post form"
                    ? ""
                    : $" ({bestName})";
            string bestText = bestTime + bestSuffix;

            var cells =
                new StringBuilder("<tr>");
            append(Results.Text(row.Class), Results.Text(row.Entry));
            foreach (var parameterName in parameterNames)
                append(Results.Text(row.Value(parameterName)));
            append(Results.Text(labelText), Results.Text(test, "test"));
            append(time(linq.Mean));
            append(time(cistern.Mean), ratio(cistern.Mean, linq.Mean, counts["Cistern"]));
            append(Results.Text(variantsText, "variants"));
            append(Results.Cell(best.Mean, bestText), ratio(best.Mean, linq.Mean, counts["Best"]));
            append(time(zlinq.Mean), ratio(best.Mean, zlinq.Mean, counts["ZLinq"]));
            append(time(handcoded.Mean), ratio(best.Mean, handcoded.Mean, counts["Handcoded"]));
            append(bytes(linq.Alloc));
            append(bytes(cistern.Alloc), ratio(cistern.Alloc, linq.Alloc));
            append(bytes(best.Alloc), ratio(best.Alloc, linq.Alloc));
            append(bytes(zlinq.Alloc), ratio(best.Alloc, zlinq.Alloc));
            append(bytes(handcoded.Alloc), ratio(best.Alloc, handcoded.Alloc));
            append("</tr>");
            var html = $"{cells}";
            body.Add(html);

            void append(params ReadOnlySpan<string> columns)
            {
                foreach (var column in columns)
                    cells.Append(column);
            }

            string time(double? maybeNanoseconds)
            {
                string formatted =
                    FormatTime(maybeNanoseconds, timeUnit);
                string cell =
                    Results.Cell(maybeNanoseconds, formatted);

                return cell;
            }

            string bytes(double? alloc)
            {
                string formatted =
                    FormatBytes(alloc, byteUnit);
                string cell =
                    Results.Cell(alloc, formatted);

                return cell;
            }

            string variantText((string Name, (double? Mean, double? Alloc) Value) variant)
            {
                string formatted =
                    FormatTime(variant.Value.Mean, timeUnit);
                string text = $"{variant.Name} {formatted}";

                return text;
            }

            void count(Dictionary<string, int> band, double? mine)
            {
                double? vsLinq =
                    linq.Mean switch
                    {
                        > 0 => mine / linq.Mean,
                        _ => null,
                    };
                string verdict =
                    Results.Verdict(vsLinq);
                if (verdict != "")
                    band[verdict]++;
            }

            static string ratio(double? mine, double? theirs, Dictionary<string, int>? band = null)
            {
                if (mine is not double numerator
                    || theirs is not double denominator)
                {
                    return Results.Cell((double?)null, "");
                }

                double value =
                    (numerator, denominator) switch
                    {
                        (_, not 0) => numerator / denominator,
                        (0, _) => 1.0,
                        _ => double.PositiveInfinity,
                    };
                string verdict =
                    Results.Verdict(value);
                if (band is not null
                    && verdict != "")
                {
                    band[verdict]++;
                }

                bool infinite =
                    double.IsPositiveInfinity(value);
                string text =
                    infinite
                        ? "∞"
                        : value is > 0 and < 0.1
                            ? $"{value:g2}×"
                            : $"{value:F2}×";
                double sort =
                    infinite
                        ? 1e18
                        : value;
                string cell =
                    Results.Cell(sort, text, verdict);

                return cell;
            }
        }

        var summaries =
            versions.Select(summaryOf);
        string summary =
            string.Join("; ", summaries);
        var labelRows =
            labelStats.Select(labelRow);
        string labelRowsText =
            string.Join("\n", labelRows);
        string bandHeads = "<th>≥2×</th><th>1.1–2×</th><th>level</th><th>ahead</th>";
        string labelTable = $"""
            <table class="labels"><thead>
            <tr><th rowspan="2">Label</th><th rowspan="2">Entries</th><th colspan="4">Cistern ÷ System.Linq</th>
            <th colspan="4">Best Cistern ÷ System.Linq</th></tr>
            <tr>{bandHeads}{bandHeads}</tr></thead><tbody>
            {labelRowsText}
            </tbody></table>
            """;
        var definitionList =
            ToubLabels.Labels.Select(definitionOf);
        string definitions =
            string.Join(" ", definitionList);

        string[] columns =
        [
            "Class",
            "Entry",
            .. parameterNames,
            "Labels",
            "Test",
            "System.Linq",
            "Cistern",
            "÷ System.Linq",
            "Cistern variants",
            "Best Cistern",
            "÷ System.Linq",
            "ZLinq",
            "Best Cistern ÷",
            "Hand-coded",
            "Best Cistern ÷",
            "System.Linq alloc",
            "Cistern alloc",
            "÷ System.Linq",
            "Best Cistern alloc",
            "÷ System.Linq",
            "ZLinq alloc",
            "Best Cistern ÷",
            "Hand-coded alloc",
            "Best Cistern ÷",
        ];
        string heads =
            Results.Heads(columns);
        int left = 4 + parameterNames.Count;

        string extraStyle = $$"""
            td:nth-child(-n+{{left}}), th:nth-child(-n+{{left}}), td.variants { text-align: left; }
            table.labels { margin: 8px 0 12px; } table.labels th { cursor: default; position: static; text-align: center; }
            table.labels td:first-child { text-align: left; } table.labels td:nth-child(n+2) { text-align: right; }
            td.variants { white-space: normal; min-width: 160px; font-size: 12px; color: #444; }
            td.test { white-space: normal; min-width: 260px; font-size: 12px; }
            """;
        string description =
            Results.RunDescription(runDirectory, rows);
        string before = $"""
            <h1>Benchmark/Toub</h1>
            <p>Generated {DateTime.Now:yyyy-MM-dd HH:mm} from {Results.Escape(runDirectory)}.</p>
            <p>{description} Out of process, one process per case.
            Every ratio is Cistern ÷ another version, for time and for allocation, so above 1 is always Cistern slower or larger:
            "÷ System.Linq" is the Cistern version to its left ÷ System.Linq; "Best Cistern ÷" is Best Cistern ÷ the version to
            its left (ZLinq or Hand-coded).
            "Cistern" is the row written as the post writes the query; "Cistern variants" are the extra Cistern rows an entry
            carries (Typed: the source reached as its own type rather than <code>IEnumerable&lt;T&gt;</code>; Inline: the chain
            built in the method, as ZLinq's is; any other: the form named by the entry), and "Best Cistern" is the fastest of all
            of them (its allocation is that row's). Colours: red ≥ 2×, orange ≥ 1.1×, green ≤ 0.9×. An empty cell is a version
            with no method for that entry. Entries: {body.Count}.</p>
            <p>Time. {Results.Escape(summary)}.</p>
            <p>Labels say what each entry exercises, read from the query; an entry carries every label that applies, so the
            counts below do not add up to the entry total. {definitions}</p>
            {labelTable}
            """;
        Results.WritePage(path, runDirectory, "Toub benchmarks", extraStyle, before, heads, body);
        Console.WriteLine($"{path}: {body.Count} entries");

        return 0;

        string summaryOf((string Name, string Label) version)
        {
            var bands = counts[version.Name];
            string text = $"{version.Label}: Cistern ≥2× behind {bands["lose2"]}, 1.1–2× behind {bands["lose"]}, level {bands["level"]}, ahead {bands["win"]}";

            return text;
        }

        static string labelRow(KeyValuePair<string, LabelStats> stat)
        {
            string label =
                Results.Escape(stat.Key);
            Dictionary<string, int>[] forms = [stat.Value.Post, stat.Value.Best];
            var bandCells =
                forms.SelectMany(form => Results.Bands.Select(band => $"<td class=\"{band}\">{form[band]}</td>"));
            string bands =
                string.Concat(bandCells);
            string html = $"<tr><td>{label}</td><td>{stat.Value.Entries}</td>{bands}</tr>";

            return html;
        }

        static string definitionOf((string Label, string Definition) label)
        {
            string name =
                Results.Escape(label.Label);
            string definition =
                Results.Escape(label.Definition);
            string text = $"<b>{name}</b>: {definition}.";

            return text;
        }
    }
}

// What each Benchmark/Toub entry exercises: its labels and a one-line description of the query, keyed by class and
// entry, the entry being a method's name before its "_<library>". The labels are read from the query, an entry taking
// every one that applies. A new entry needs a line here, or the report stops naming it; a line whose entry no longer
// exists also stops it.
static class ToubLabels
{
    const string Simd = "SIMD";
    const string Algo = "Algorithm";
    const string Size = "Sizing";
    const string Sort = "Sort";
    const string Empty = "Empty";
    const string New = "New operator";
    const string Const = "Constant-factor";
    const string Via = "Via IEnumerable";

    // Each label's definition, shown in the report's header, in the order the report lists them.
    public static readonly (string Label, string Definition)[] Labels =
    [
        (Simd, "a vectorised loop over a primitive span (Sum, Min, Max, SequenceEqual, Contains, a vectorised fill)"),
        (Algo, "work skipped by answering the query differently (Contains without sorting or shuffling, OrderBy().Last() "
             + "without a full sort, TakeLast reading the tail, a deep Concat flattened, First or ElementAt answered "
             + "without building a set)"),
        (Size, "ToArray, ToList, ToDictionary or AddRange sizing its result"),
        (Sort, "sort internals"),
        (Empty, "an operator over an empty source, measured for allocation"),
        (New, "an operator added in that release (Zip3, Take(Range), LeftJoin, FullJoin) or the composition it replaces"),
        (Const, "the same work done with less overhead"),
        (Via, "the source or chain is reached through IEnumerable<T>, so its real type has to be recovered behind the "
            + "interface (the Typed variant, where an entry has one, is the same query without it)"),
    ];

    public static readonly Dictionary<(string Class, string Entry), (string[] Labels, string Test)> Types = new()
    {
        [("NetCore20", "N01")] = ([Algo, Via], "foreach over Range(0, 10) concatenated 10,000 times through IEnumerable<int>"),
        [("NetCore20", "N02")] = ([Algo, Via], "Range.Reverse() as IEnumerable<int>, OrderBy().Skip(4).First() over 10M"),
        [("NetCore20", "N03")] = ([Size, Via], "IEnumerable<int> over int[10M], Select().ToList()"),
        [("NetCore20", "N04")] = ([Size, Via], "Range(0, 100M) as IEnumerable<int>, ToArray()"),
        [("NetCore30", "N01")] = ([Algo, Via], "IEnumerable<int> over array Select, TakeLast(10).Sum()"),
        [("NetCore30", "N02")] = ([Size], "Range(0, 100).Select().ToArray()"),
        [("NetCore30", "N03")] = ([Empty], "Empty<int>().Take(10).Select().ToArray()"),
        [("Net5", "N01")] = ([Sort], "foreach over OrderBy(i => i) of random int[1000]"),
        [("Net5", "N02")] = ([Algo, Via], "IEnumerable<int> over List<int> of 100, SkipLast(5).Sum()"),
        [("Net6", "N01")] = ([Const, Via], "IEnumerable<string> over string[100k], Distinct().Count()"),
        [("Net6", "N02")] = ([Simd, Via], "SequenceEqual of two int[1M] as IEnumerable<int>"),
        [("Net6", "N03a")] = ([Const], "TimeSpan[1M].Max()"),
        [("Net6", "N03b")] = ([Const], "TimeSpan[1M].Min()"),
        [("Net6", "N04a")] = ([New, Via], "foreach over Zip(e2).Zip(e3, selector) of three Ranges as IEnumerable<int>"),
        [("Net6", "N04b")] = ([New, Via], "foreach over three-way Zip(e2, e3) of Ranges as IEnumerable<int>"),
        [("Net6", "N05a")] = ([Const], "foreach over 10k iterator, Skip(1000).Take(10)"),
        [("Net6", "N05b")] = ([New], "foreach over 10k iterator, Take(1000..1010)"),
        [("Net7", "N01a")] = ([Simd, Via], "IEnumerable<int> over int[4], Min()"),
        [("Net7", "N01b")] = ([Simd, Via], "IEnumerable<int> over int[1024], Min()"),
        [("Net7", "N02a")] = ([Simd, Via], "IEnumerable<int> over int[4], Max()"),
        [("Net7", "N02b")] = ([Simd, Via], "IEnumerable<int> over int[1024], Max()"),
        [("Net7", "N03a")] = ([Simd, Via], "IEnumerable<float> over float[10k], Sum()"),
        [("Net7", "N03b")] = ([Simd, Via], "IEnumerable<float> over float[10k], Average()"),
        [("Net7", "N03c")] = ([Simd, Via], "IEnumerable<float> over float[10k], Min()"),
        [("Net7", "N03d")] = ([Simd, Via], "IEnumerable<float> over float[10k], Max()"),
        [("Net7", "N04a")] = ([Sort], "foreach over reversed int[1024], OrderBy(x => x)"),
        [("Net7", "N04b")] = ([Sort], "foreach over reversed int[1024], Order()"),
        [("Net8", "N01a")] = ([Size, Via], "IEnumerable<int> over int[1024], Select().ToList()"),
        [("Net8", "N01b")] = ([Simd, Size], "Repeat((byte)'a', 1024).ToList()"),
        [("Net8", "N01c")] = ([Size], "Range(0, 1024).Select().ToList()"),
        [("Net8", "N02")] = ([Simd, Size], "Range(0, 16384).ToList()"),
        [("Net8", "N03")] = ([Size, Via], "List<byte>.AddRange(Repeat((byte)'a', 1024))"),
        [("Net8", "N04")] = ([Simd], "byte[4096].Max()"),
        [("Net8", "N05")] = ([Simd, Via], "IEnumerable<int> over int[1024], Sum()"),
        [("Net8", "N06")] = ([Sort, Via], "foreach over Range(0, 1000).Reverse() as IEnumerable<int>, Order()"),
        [("Net8", "N07")] = ([Sort], "foreach over reversed int[1M], OrderBy(i => i * 2)"),
        [("Net8", "N08")] = ([Const, Via], "Any() over a string, a List<int> and an int[]"),
        [("Net9", "N01a")] = ([Algo, Via], "array.Distinct().First() handed out as IEnumerable<int>"),
        [("Net9", "N01b")] = ([Algo, Via], "array.Append(42).Select().Last() handed out as IEnumerable<int>"),
        [("Net9", "N01c")] = ([Algo, Via], "Range.Reverse().Count() handed out as IEnumerable<int>"),
        [("Net9", "N01d")] = ([Algo, Via], "list.DefaultIfEmpty().Select().ElementAt(999) handed out as IEnumerable<int>"),
        [("Net9", "N01e")] = ([Algo, Via], "list.Skip(500).Take(100).ElementAt(99) handed out as IEnumerable<int>"),
        [("Net9", "N01f")] = ([Algo, Via], "Range.Union(Range).First() handed out as IEnumerable<int>"),
        [("Net9", "N02a")] = ([Const, Via], "Where().Select().Any() handed out as IEnumerable<int>, over an iterator"),
        [("Net9", "N02b")] = ([Const, Via], "Where().Select().Any() handed out as IEnumerable<int>, over an array"),
        [("Net9", "N03a")] = ([Empty], "Chunk over empty string[], returned as object"),
        [("Net9", "N03b")] = ([Empty], "Distinct over empty string[], returned as object"),
        [("Net9", "N03c")] = ([Empty], "GroupJoin over empty string[], returned as object"),
        [("Net9", "N03d")] = ([Empty], "Join over empty string[], returned as object"),
        [("Net9", "N03e")] = ([Empty], "ToLookup over empty string[], returned as object"),
        [("Net9", "N03f")] = ([Empty], "Reverse over empty string[], returned as object"),
        [("Net9", "N03g")] = ([Empty], "indexed Select over empty string[], returned as object"),
        [("Net9", "N03h")] = ([Empty], "SelectMany over empty string[], returned as object"),
        [("Net9", "N03i")] = ([Empty], "SkipWhile over empty string[], returned as object"),
        [("Net9", "N03j")] = ([Empty], "TakeWhile over empty string[], returned as object"),
        [("Net9", "N03k")] = ([Empty], "indexed Where over empty string[], returned as object"),
        [("Net9", "N04")] = ([Simd], "int[1000].DefaultIfEmpty().Average()"),
        [("Net9", "N05")] = ([Const], "foreach over int[1000].Chunk(10), summing chunk lengths"),
        [("Net9", "N06a")] = ([Const, Via], "List<int> as IEnumerable<int>, Any(predicate)"),
        [("Net9", "N06b")] = ([Const, Via], "List<int> as IEnumerable<int>, All(predicate)"),
        [("Net9", "N06c")] = ([Const, Via], "List<int> as IEnumerable<int>, Count(predicate)"),
        [("Net9", "N06d")] = ([Const, Via], "List<int> as IEnumerable<int>, First(predicate)"),
        [("Net9", "N06e")] = ([Const, Via], "List<int> as IEnumerable<int>, Single(predicate)"),
        [("Net9", "N07")] = ([Simd, Via], "SequenceEqual of int[10k] and a List<int> copy, as IEnumerable<int>"),
        [("Net9", "N08a")] = ([Size], "ToArray of a 1-element string iterator"),
        [("Net9", "N08b")] = ([Size], "ToArray of an 8-element string iterator"),
        [("Net9", "N08c")] = ([Size], "ToArray of a 500-element string iterator"),
        [("Net9", "N09a")] = ([Size], "ToList of an 8-element iterator through Select"),
        [("Net9", "N09b")] = ([Size], "ToList of an 8-element iterator through Where().Select()"),
        [("Net9", "N10")] = ([Sort, Size], "OrderBy().ToArray() over a 1-element iterator"),
        [("Net9", "N11")] = ([Size], "ToDictionary over a 10k-element Select iterator"),
        [("Net9", "N12")] = ([Const], "Int128[1000].Max()"),
        [("Net9", "N13")] = ([Simd], "int[10k].Max()"),
        [("Net9", "N14a")] = ([Algo], "shuffled List<int> of 16k, OrderBy(x => x).Last()"),
        [("Net9", "N14b")] = ([Algo], "shuffled List<int> of 16k, Order().Last()"),
        [("Net9", "N15")] = ([Sort, Via], "foreach over Order() of 100 DayOfWeek as IEnumerable<DayOfWeek>"),
        [("Net9", "N16")] = ([Sort, Via], "foreach over OrderBy(i => i) of int[1000] as IEnumerable<int>"),
        [("Net9", "N17")] = ([Const], "foreach over a ToLookup of 80 strings, groupings and items"),
        [("Net10", "N01a")] = ([Algo, Via], "IEnumerable<int> over int[1000], Append(100).Contains(999)"),
        [("Net10", "N01b")] = ([Algo, Via], "IEnumerable<int> over int[1000], Concat(source).Contains(999)"),
        [("Net10", "N01c")] = ([Algo, Via], "IEnumerable<int> over int[1000], DefaultIfEmpty(42).Contains(999)"),
        [("Net10", "N01d")] = ([Algo, Via], "IEnumerable<int> over int[1000], Distinct().Contains(999)"),
        [("Net10", "N01e")] = ([Algo, Via], "IEnumerable<int> over int[1000], OrderBy(x => x).Contains(999)"),
        [("Net10", "N01f")] = ([Algo, Via], "IEnumerable<int> over int[1000], Reverse().Contains(999)"),
        [("Net10", "N01g")] = ([Algo, Via], "IEnumerable<int> over int[1000], Union(source).Contains(999)"),
        [("Net10", "N01h")] = ([Algo, Via], "IEnumerable<int> over int[1000], SelectMany(x => source).Contains(999)"),
        [("Net10", "N01i")] = ([Algo, Via], "IEnumerable<int> over int[1000], Where(x => true).Select(x => x).Contains(999)"),
        [("Net10", "N02")] = ([Size, Via], "List<short> cleared and filled from Sequence<short>(42, 1042, 1)"),
        [("Net10", "N03a")] = ([Algo, Via], "List<int> of 1000 as IEnumerable<int>, Shuffle().Take(10).ToList()"),
        [("Net10", "N03b")] = ([Algo, Via], "List<int> of 1000 as IEnumerable<int>, the post's manual shuffle then Take(10).ToList()"),
        [("Net10", "N04a")] = ([Algo, Via], "List<int> of 1000 as IEnumerable<int>, Shuffle().Take(10).Contains(2000)"),
        [("Net10", "N04b")] = ([Algo, Via], "List<int> of 1000 as IEnumerable<int>, the post's manual shuffle then Take(10).Contains(2000)"),
        [("Net10", "N05a")] = ([New], "LeftJoin of two Sequences, Count()"),
        [("Net10", "N05b")] = ([New], "GroupJoin/SelectMany/DefaultIfEmpty left join of two Sequences, Count()"),
        [("Net10", "N06")] = ([Size, Via], "IEnumerable<string> over string[1000], Skip(200).Take(200).ToList()"),
        [("Net10", "N07")] = ([Const, Via], "ReadOnlyCollection<int> of 1000, Skip(100).Take(800).Sum()"),
        [("Net10", "N08")] = ([Simd, Via], "IEnumerable<int> over int[1M], Contains(int.MaxValue, EqualityComparer<int>.Default)"),
        [("Net11", "N01")] = ([Algo], "int[5].Append(6).Skip(10).LastOrDefault()"),
        [("Net11", "N02")] = ([Simd], "int[32].Sum()"),
        [("Net11", "N03a")] = ([Simd], "byte[16].Max()"),
        [("Net11", "N03b")] = ([Simd], "byte[64].Max()"),
        [("Net11", "N04a")] = ([New], "FullJoin of two 10k tuple lists, Count()"),
        [("Net11", "N04b")] = ([New], "GroupJoin/SelectMany/Concat full join of two 10k tuple lists, Count()"),
    };
}
