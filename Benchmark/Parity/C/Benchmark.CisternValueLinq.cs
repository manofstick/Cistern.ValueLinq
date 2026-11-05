using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class C
    {
        [Benchmark]
        public int C1a_CisternValueLinq() =>
            _set
            .Count();

        [Benchmark]
        public int C1b_CisternValueLinq() =>
            _nonGeneric
            .Count();

        [Benchmark]
        public int C2a_CisternValueLinq() =>
            _set
            .TryGetNonEnumeratedCount(out var count) ? count : -1;

        [Benchmark]
        public int C2b_CisternValueLinq() =>
            _nonGeneric
            .TryGetNonEnumeratedCount(out var count) ? count : -1;

        [Benchmark]
        public bool C3a_CisternValueLinq() =>
            _set
            .Any();

        [Benchmark]
        public bool C3b_CisternValueLinq() =>
            _iterator
            .Any();

        [Benchmark]
        public int C4_CisternValueLinq() =>
            _collection
            .First();

        [Benchmark]
        public int C5_CisternValueLinq() =>
            _collection
            .Last();

        [Benchmark]
        public int C6_CisternValueLinq() =>
            _collection
            .Last(_predicate);

        [Benchmark]
        public int C7_CisternValueLinq() =>
            _collection
            .ElementAt(Length / 2);

        [Benchmark]
        public int C8a_CisternValueLinq() =>
            _collection
            .ElementAt(^10);

        [Benchmark]
        public int C8b_CisternValueLinq() =>
            _set
            .ElementAt(^10);

        [Benchmark]
        public int C9_CisternValueLinq() =>
            _single
            .Single();

        [Benchmark]
        public bool C10a_CisternValueLinq() =>
            _set
            .Contains(_present);

        [Benchmark]
        public bool C10b_CisternValueLinq() =>
            _collection
            .Contains(_present);

        [Benchmark]
        public bool C10c_CisternValueLinq() =>
            _words
            .Contains(_wordAsked);

        [Benchmark]
        public bool C11_CisternValueLinq() =>
            _collection
            .Contains(_present, EqualityComparer<int>.Default);

        [Benchmark]
        public int[] C12a_CisternValueLinq() =>
            _set
            .ToArray();

        [Benchmark]
        public int[] C12b_CisternValueLinq() =>
            _collection
            .ToArray();

        [Benchmark]
        public List<int> C13a_CisternValueLinq() =>
            _set
            .ToList();

        [Benchmark]
        public List<int> C13b_CisternValueLinq() =>
            _collection
            .ToList();

        [Benchmark]
        public int C14a_CisternValueLinq() =>
            _set
            .ToDictionary(_keySelector)
            .Count;

        [Benchmark]
        public int C14b_CisternValueLinq() =>
            _emptyCollection
            .ToDictionary(_keySelector)
            .Count;

        [Benchmark]
        public bool C16a_CisternValueLinq() =>
            _array
            .SequenceEqual(_arrayCopy);

        [Benchmark]
        public bool C16b_CisternValueLinq() =>
            _listA
            .SequenceEqual(_listB);

        [Benchmark]
        public bool C17a_CisternValueLinq() =>
            _set
            .SequenceEqual(_setOther);

        [Benchmark]
        public bool C17b_CisternValueLinq() =>
            _collection
            .SequenceEqual(_collectionCopy);

        [Benchmark]
        public bool C17c_CisternValueLinq() =>
            _iterator
            .Select(_keySelector)
            .SequenceEqual(_iterator.Select(_keySelector));

        [Benchmark]
        public int[] C19a_CisternValueLinq() =>
            _collection
            .TakeLast(10)
            .ToArray();

        [Benchmark]
        public int C19b_CisternValueLinq() =>
            _set
            .SkipLast(10)
            .Count();

        [Benchmark]
        public int C20_CisternValueLinq() =>
            _array
            .Chunk(100)
            .Sum(c => c.Length);
    }
}
