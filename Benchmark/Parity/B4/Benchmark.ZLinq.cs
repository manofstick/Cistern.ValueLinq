using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B4
    {
        [Benchmark]
        public int B26a_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .Count();

        [Benchmark]
        public bool B26b_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .Contains(_start + Length - 10);

        [Benchmark]
        public int B26c_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B26d_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .First();

        [Benchmark]
        public int B26e_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .Last();

        [Benchmark]
        public int[] B27a_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .ToArray();

        [Benchmark]
        public List<int> B27b_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .ToList();

        // B28: ZLinq has no IEnumerable<T> view of a ValueEnumerable to hand List<T>'s constructor
        // (the constructor taking one is the entry)

        [Benchmark]
        public int B29a_ZLinq() =>
            ValueEnumerable
            .Repeat(_element, Length)
            .Count();

        [Benchmark]
        public bool B29b_ZLinq() =>
            ValueEnumerable
            .Repeat(_element, Length)
            .Contains(_absent);

        [Benchmark]
        public int B29c_ZLinq() =>
            ValueEnumerable
            .Repeat(_element, Length)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B29d_ZLinq() =>
            ValueEnumerable
            .Repeat(_element, Length)
            .First();

        [Benchmark]
        public int B29e_ZLinq() =>
            ValueEnumerable
            .Repeat(_element, Length)
            .Last();

        [Benchmark]
        public int[] B30a_ZLinq() =>
            ValueEnumerable
            .Repeat(_element, Length)
            .ToArray();

        [Benchmark]
        public List<int> B30b_ZLinq() =>
            ValueEnumerable
            .Repeat(_element, Length)
            .ToList();

        [Benchmark]
        public string[] B30c_ZLinq() =>
            ValueEnumerable
            .Repeat(_nullString, Length)
            .ToArray();

        [Benchmark]
        public List<string> B30d_ZLinq() =>
            ValueEnumerable
            .Repeat(_nullString, Length)
            .ToList();

        // B31: ZLinq has no IEnumerable<T> view of a ValueEnumerable to hand List<T>'s constructor
        // (the constructor taking one is the entry)

        [Benchmark]
        public int B32a_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .Select(_selector)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B32b_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .Select(_selector)
            .First();

        [Benchmark]
        public int B32c_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B32d_ZLinq() =>
            ValueEnumerable
            .Range(_start, Length)
            .Select(_selector)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;
    }
}
