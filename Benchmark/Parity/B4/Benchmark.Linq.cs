using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class B4
    {
        [Benchmark]
        public int B26a_Linq() =>
            Enumerable
            .Range(_start, Length)
            .Count();

        [Benchmark]
        public bool B26b_Linq() =>
            Enumerable
            .Range(_start, Length)
            .Contains(_start + Length - 10);

        [Benchmark]
        public int B26c_Linq() =>
            Enumerable
            .Range(_start, Length)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B26d_Linq() =>
            Enumerable
            .Range(_start, Length)
            .First();

        [Benchmark]
        public int B26e_Linq() =>
            Enumerable
            .Range(_start, Length)
            .Last();

        [Benchmark]
        public int[] B27a_Linq() =>
            Enumerable
            .Range(_start, Length)
            .ToArray();

        [Benchmark]
        public List<int> B27b_Linq() =>
            Enumerable
            .Range(_start, Length)
            .ToList();

        [Benchmark]
        public List<int> B28_Linq() =>
            new List<int>(Enumerable.Range(_start, Length));

        [Benchmark]
        public int B29a_Linq() =>
            Enumerable
            .Repeat(_element, Length)
            .Count();

        [Benchmark]
        public bool B29b_Linq() =>
            Enumerable
            .Repeat(_element, Length)
            .Contains(_absent);

        [Benchmark]
        public int B29c_Linq() =>
            Enumerable
            .Repeat(_element, Length)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B29d_Linq() =>
            Enumerable
            .Repeat(_element, Length)
            .First();

        [Benchmark]
        public int B29e_Linq() =>
            Enumerable
            .Repeat(_element, Length)
            .Last();

        [Benchmark]
        public int[] B30a_Linq() =>
            Enumerable
            .Repeat(_element, Length)
            .ToArray();

        [Benchmark]
        public List<int> B30b_Linq() =>
            Enumerable
            .Repeat(_element, Length)
            .ToList();

        [Benchmark]
        public string[] B30c_Linq() =>
            Enumerable
            .Repeat(_nullString, Length)
            .ToArray();

        [Benchmark]
        public List<string> B30d_Linq() =>
            Enumerable
            .Repeat(_nullString, Length)
            .ToList();

        [Benchmark]
        public List<int> B31_Linq() =>
            new List<int>(Enumerable.Repeat(_element, Length));

        [Benchmark]
        public int B32a_Linq() =>
            Enumerable
            .Range(_start, Length)
            .Select(_selector)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B32b_Linq() =>
            Enumerable
            .Range(_start, Length)
            .Select(_selector)
            .First();

        [Benchmark]
        public int B32c_Linq() =>
            Enumerable
            .Range(_start, Length)
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B32d_Linq() =>
            Enumerable
            .Range(_start, Length)
            .Select(_selector)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;
    }
}
