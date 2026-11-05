using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks
{
    // The lengths a Coverage benchmark runs over: those listed in CISTERN_LENGTHS (comma-separated) when it is set, so a
    // run can measure short sources without editing every class.
    public static class Lengths
    {
        public static IEnumerable<int> Values =>
            Environment.GetEnvironmentVariable("CISTERN_LENGTHS") is { Length: > 0 } lengths
                ? lengths.Split(',').Select(int.Parse)
                : new[] { 10, 1000, 100000 };
    }
}
