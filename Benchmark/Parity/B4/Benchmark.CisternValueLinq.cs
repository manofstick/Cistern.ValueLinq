using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B4
    {
        [Benchmark]
        public int B26a_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .Count();

        [Benchmark]
        public bool B26b_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .Contains(_start + Length - 10);

        [Benchmark]
        public int B26c_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B26d_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .First();

        [Benchmark]
        public int B26e_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .Last();

        [Benchmark]
        public int[] B27a_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .ToArray();

        [Benchmark]
        public List<int> B27b_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .ToList();

        // System.Linq's Range is an IList<int>, so List's constructor copies it through CopyTo; Cistern's Range would have to
        // return a type of its own implementing ICollection<T> to match, so the entry is measured in its ToList form
        [Benchmark]
        public List<int> B28_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .ToList();

        [Benchmark]
        public int B29a_CisternValueLinq() =>
            Enumerable
            .Repeat(_element, Length)
            .Count();

        [Benchmark]
        public bool B29b_CisternValueLinq() =>
            Enumerable
            .Repeat(_element, Length)
            .Contains(_absent);

        [Benchmark]
        public int B29c_CisternValueLinq() =>
            Enumerable
            .Repeat(_element, Length)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B29d_CisternValueLinq() =>
            Enumerable
            .Repeat(_element, Length)
            .First();

        [Benchmark]
        public int B29e_CisternValueLinq() =>
            Enumerable
            .Repeat(_element, Length)
            .Last();

        [Benchmark]
        public int[] B30a_CisternValueLinq() =>
            Enumerable
            .Repeat(_element, Length)
            .ToArray();

        [Benchmark]
        public List<int> B30b_CisternValueLinq() =>
            Enumerable
            .Repeat(_element, Length)
            .ToList();

        [Benchmark]
        public string[] B30c_CisternValueLinq() =>
            Enumerable
            .Repeat(_nullString, Length)
            .ToArray();

        [Benchmark]
        public List<string> B30d_CisternValueLinq() =>
            Enumerable
            .Repeat(_nullString, Length)
            .ToList();

        // As B28: System.Linq's Repeat is an IList<T>, which Cistern's Repeat is not, so the entry is measured in its ToList form
        [Benchmark]
        public List<int> B31_CisternValueLinq() =>
            Enumerable
            .Repeat(_element, Length)
            .ToList();

        [Benchmark]
        public int B32a_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .Select(_selector)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B32b_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .Select(_selector)
            .First();

        [Benchmark]
        public int B32c_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B32d_CisternValueLinq() =>
            Enumerable
            .Range(_start, Length)
            .Select(_selector)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;
    }
}
