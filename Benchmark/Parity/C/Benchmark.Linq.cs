using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class C
    {
        [Benchmark]
        public int C1a_Linq() =>
            _set
            .Count();

        [Benchmark]
        public int C1b_Linq() =>
            _nonGeneric
            .Count();

        [Benchmark]
        public int C2a_Linq() =>
            _set
            .TryGetNonEnumeratedCount(out var count) ? count : -1;

        [Benchmark]
        public int C2b_Linq() =>
            _nonGeneric
            .TryGetNonEnumeratedCount(out var count) ? count : -1;

        [Benchmark]
        public bool C3a_Linq() =>
            _set
            .Any();

        [Benchmark]
        public bool C3b_Linq() =>
            _iterator
            .Any();

        [Benchmark]
        public int C4_Linq() =>
            _collection
            .First();

        [Benchmark]
        public int C5_Linq() =>
            _collection
            .Last();

        [Benchmark]
        public int C6_Linq() =>
            _collection
            .Last(_predicate);

        [Benchmark]
        public int C7_Linq() =>
            _collection
            .ElementAt(Length / 2);

        [Benchmark]
        public int C8a_Linq() =>
            _collection
            .ElementAt(^10);

        [Benchmark]
        public int C8b_Linq() =>
            _set
            .ElementAt(^10);

        [Benchmark]
        public int C9_Linq() =>
            _single
            .Single();

        [Benchmark]
        public bool C10a_Linq() =>
            _set
            .Contains(_present);

        [Benchmark]
        public bool C10b_Linq() =>
            _collection
            .Contains(_present);

        [Benchmark]
        public bool C10c_Linq() =>
            _words
            .Contains(_wordAsked);

        [Benchmark]
        public bool C11_Linq() =>
            _collection
            .Contains(_present, EqualityComparer<int>.Default);

        [Benchmark]
        public int[] C12a_Linq() =>
            _set
            .ToArray();

        [Benchmark]
        public int[] C12b_Linq() =>
            _collection
            .ToArray();

        [Benchmark]
        public List<int> C13a_Linq() =>
            _set
            .ToList();

        [Benchmark]
        public List<int> C13b_Linq() =>
            _collection
            .ToList();

        [Benchmark]
        public int C14a_Linq() =>
            _set
            .ToDictionary(_keySelector)
            .Count;

        [Benchmark]
        public int C14b_Linq() =>
            _emptyCollection
            .ToDictionary(_keySelector)
            .Count;

        [Benchmark]
        public bool C16a_Linq() =>
            _array
            .SequenceEqual(_arrayCopy);

        [Benchmark]
        public bool C16b_Linq() =>
            _listA
            .SequenceEqual(_listB);

        [Benchmark]
        public bool C17a_Linq() =>
            _set
            .SequenceEqual(_setOther);

        [Benchmark]
        public bool C17b_Linq() =>
            _collection
            .SequenceEqual(_collectionCopy);

        [Benchmark]
        public bool C17c_Linq() =>
            _iterator
            .Select(_keySelector)
            .SequenceEqual(_iterator.Select(_keySelector));

        [Benchmark]
        public int[] C19a_Linq() =>
            _collection
            .TakeLast(10)
            .ToArray();

        [Benchmark]
        public int C19b_Linq() =>
            _set
            .SkipLast(10)
            .Count();

        [Benchmark]
        public int C20_Linq() =>
            _array
            .Chunk(100)
            .Sum(c => c.Length);
    }
}
