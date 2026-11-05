using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class C
    {
        [Benchmark]
        public int C1a_ZLinq() =>
            _set
            .AsValueEnumerable()
            .Count();

        [Benchmark]
        public int C1b_ZLinq() =>
            _nonGeneric
            .AsValueEnumerable()
            .Count();

        [Benchmark]
        public int C2a_ZLinq() =>
            _set
            .AsValueEnumerable()
            .TryGetNonEnumeratedCount(out var count) ? count : -1;

        [Benchmark]
        public int C2b_ZLinq() =>
            _nonGeneric
            .AsValueEnumerable()
            .TryGetNonEnumeratedCount(out var count) ? count : -1;

        [Benchmark]
        public bool C3a_ZLinq() =>
            _set
            .AsValueEnumerable()
            .Any();

        [Benchmark]
        public bool C3b_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Any();

        [Benchmark]
        public int C4_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .First();

        [Benchmark]
        public int C5_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .Last();

        [Benchmark]
        public int C6_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .Last(_predicate);

        [Benchmark]
        public int C7_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .ElementAt(Length / 2);

        [Benchmark]
        public int C8a_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .ElementAt(^10);

        [Benchmark]
        public int C8b_ZLinq() =>
            _set
            .AsValueEnumerable()
            .ElementAt(^10);

        [Benchmark]
        public int C9_ZLinq() =>
            _single
            .AsValueEnumerable()
            .Single();

        [Benchmark]
        public bool C10a_ZLinq() =>
            _set
            .AsValueEnumerable()
            .Contains(_present);

        [Benchmark]
        public bool C10b_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .Contains(_present);

        [Benchmark]
        public bool C10c_ZLinq() =>
            _words
            .AsValueEnumerable()
            .Contains(_wordAsked);

        [Benchmark]
        public bool C11_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .Contains(_present, EqualityComparer<int>.Default);

        [Benchmark]
        public int[] C12a_ZLinq() =>
            _set
            .AsValueEnumerable()
            .ToArray();

        [Benchmark]
        public int[] C12b_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .ToArray();

        [Benchmark]
        public List<int> C13a_ZLinq() =>
            _set
            .AsValueEnumerable()
            .ToList();

        [Benchmark]
        public List<int> C13b_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .ToList();

        [Benchmark]
        public int C14a_ZLinq() =>
            _set
            .AsValueEnumerable()
            .ToDictionary(_keySelector)
            .Count;

        [Benchmark]
        public int C14b_ZLinq() =>
            _emptyCollection
            .AsValueEnumerable()
            .ToDictionary(_keySelector)
            .Count;

        [Benchmark]
        public bool C16a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .SequenceEqual(_arrayCopy);

        [Benchmark]
        public bool C16b_ZLinq() =>
            _listA
            .AsValueEnumerable()
            .SequenceEqual(_listB);

        [Benchmark]
        public bool C17a_ZLinq() =>
            _set
            .AsValueEnumerable()
            .SequenceEqual(_setOther);

        [Benchmark]
        public bool C17b_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .SequenceEqual(_collectionCopy);

        [Benchmark]
        public bool C17c_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Select(_keySelector)
            .SequenceEqual(_iterator.AsValueEnumerable().Select(_keySelector));

        [Benchmark]
        public int[] C19a_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .TakeLast(10)
            .ToArray();

        [Benchmark]
        public int C19b_ZLinq() =>
            _set
            .AsValueEnumerable()
            .SkipLast(10)
            .Count();

        [Benchmark]
        public int C20_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Chunk(100)
            .Sum(c => c.Length);
    }
}
