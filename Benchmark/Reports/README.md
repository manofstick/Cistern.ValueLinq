# Benchmark reports

`Report.cs` runs the Parity or Toub suite and writes it up as a sortable HTML table, Cistern against System.Linq,
ZLinq and the hand-coded loop. It is a file-based program (`dotnet run Report.cs`), not part of the benchmark
project's build. Run it through the net11 launcher, so the benchmark exe it starts inherits `DOTNET_ROOT`, after
building the benchmark project in Release:

```
dotnet11.sh run Benchmark/Reports/Report.cs run parity <run directory>
dotnet11.sh run Benchmark/Reports/Report.cs run toub <run directory> [<extra benchmark arguments>...]
dotnet11.sh run Benchmark/Reports/Report.cs parity <run directory> [<output .html>]
dotnet11.sh run Benchmark/Reports/Report.cs toub <run directory> [<output .html>]
dotnet11.sh run Benchmark/Reports/Report.cs progress <run directory>
```

`run` takes a new directory, puts BenchmarkDotNet's artifacts, the console log, the command line and the end time
in it, and writes `report.html` there; the other verbs read such a directory, while the run is going if need be. A
`--filter` among the extra arguments replaces the suite's, to run part of it.

A report stops, naming what it met, on anything it has no place for: a method not named `<entry>_<library>`, a
library it has no column for, or a Toub entry with no line in the script's `ToubLabels` (each entry's labels and
description) or a line with no entry.
