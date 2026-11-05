using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B11
    {
        System.Linq.IGrouping<int, int> _zlinqGrouping;
        System.Linq.ILookup<int, int> _zlinqLookup;

        void InitZLinq()
        {
            var lookup = _array.AsValueEnumerable().ToLookup(x => x % 10);
            _zlinqLookup = lookup;
            _zlinqGrouping = (System.Linq.IGrouping<int, int>)lookup[3];
        }

        [Benchmark]
        public int B78a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .DefaultIfEmpty(-1)
            .First();

        [Benchmark]
        public int B78b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .DefaultIfEmpty(-1)
            .Last();

        [Benchmark]
        public int B78c_ZLinq() =>
            _list
            .AsValueEnumerable()
            .DefaultIfEmpty(-1)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B78d_ZLinq() =>
            _list
            .AsValueEnumerable()
            .DefaultIfEmpty(-1)
            .Count();

        [Benchmark]
        public int[] B78e_ZLinq() =>
            _list
            .AsValueEnumerable()
            .DefaultIfEmpty(-1)
            .ToArray();

        [Benchmark]
        public List<int> B78f_ZLinq() =>
            _list
            .AsValueEnumerable()
            .DefaultIfEmpty(-1)
            .ToList();

        [Benchmark]
        public bool B79a_ZLinq() =>
            _set
            .AsValueEnumerable()
            .DefaultIfEmpty(-1)
            .Contains(_present);

        [Benchmark]
        public bool B79b_ZLinq() =>
            _empty
            .AsValueEnumerable()
            .DefaultIfEmpty(-1)
            .Contains(-1);

        [Benchmark]
        public int B80a_ZLinq() =>
            _arrayList
            .AsValueEnumerable()
            .Cast<int>()
            .Count();

        [Benchmark]
        public int[] B80b_ZLinq() =>
            _arrayList
            .AsValueEnumerable()
            .Cast<int>()
            .ToArray();

        [Benchmark]
        public List<int> B80c_ZLinq() =>
            _arrayList
            .AsValueEnumerable()
            .Cast<int>()
            .ToList();

        [Benchmark]
        public int B80d_ZLinq() =>
            _arrayList
            .AsValueEnumerable()
            .Cast<int>()
            .TryGetNonEnumeratedCount(out var count) ? count : -1;

        [Benchmark]
        public int B81a_ZLinq() =>
            _jagged
            .AsValueEnumerable()
            .SelectMany(x => x)
            .Count();

        [Benchmark]
        public bool B81b_ZLinq() =>
            _jagged
            .AsValueEnumerable()
            .SelectMany(x => x)
            .Contains(_presentInJagged);

        [Benchmark]
        public int[] B81c_ZLinq() =>
            _jagged
            .AsValueEnumerable()
            .SelectMany(x => x)
            .ToArray();

        [Benchmark]
        public List<int> B81d_ZLinq() =>
            _jagged
            .AsValueEnumerable()
            .SelectMany(x => x)
            .ToList();

        [Benchmark]
        public int B82a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .GroupBy(_keySelector)
            .Count();

        [Benchmark]
        public int B82b_ZLinq() =>
            _array
            .AsValueEnumerable()
            .GroupBy(_keySelector, _resultSelector)
            .Count();

        [Benchmark]
        public int B82c_ZLinq() =>
            _array
            .AsValueEnumerable()
            .GroupBy(_keySelector)
            .ToArray()
            .Length;

        [Benchmark]
        public int B82d_ZLinq() =>
            _array
            .AsValueEnumerable()
            .GroupBy(_keySelector)
            .ToList()
            .Count;

        [Benchmark]
        public int B82e_ZLinq() =>
            _array
            .AsValueEnumerable()
            .ToLookup(_keySelector)
            .Count;

        [Benchmark]
        public bool B82f_ZLinq() =>
            _zlinqGrouping
            .AsValueEnumerable()
            .Contains(_inGroup);

        [Benchmark]
        public int B82g_ZLinq() =>
            _zlinqGrouping
            .AsValueEnumerable()
            .Count();

        [Benchmark]
        public int B82h_ZLinq() =>
            _zlinqGrouping
            .AsValueEnumerable()
            .ElementAt(500);

        [Benchmark]
        public int[] B82i_ZLinq() =>
            _zlinqGrouping
            .AsValueEnumerable()
            .ToArray();

        [Benchmark]
        public int B82j_ZLinq() =>
            _zlinqLookup
            .AsValueEnumerable()
            .Count();

        [Benchmark]
        public int B82k_ZLinq() =>
            _zlinqLookup
            .AsValueEnumerable()
            .ToList()
            .Count;

        [Benchmark]
        public int B82l_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Join(_empty, _keySelector, _keySelector, (a, b) => a + b)
            .Count();

        [Benchmark]
        public int B82m_ZLinq() =>
            _empty
            .AsValueEnumerable()
            .Join(_array, _keySelector, _keySelector, (a, b) => a + b)
            .Count();

        [Benchmark]
        public int B82n_ZLinq() =>
            _empty
            .AsValueEnumerable()
            .GroupJoin(_array, _keySelector, _keySelector, (a, g) => a)
            .Count();

        [Benchmark]
        public int B82o_ZLinq() =>
            _empty
            .AsValueEnumerable()
            .LeftJoin(_array, _keySelector, _keySelector, (a, b) => a + b)
            .Count();

        [Benchmark]
        public int B82p_ZLinq() =>
            _array
            .AsValueEnumerable()
            .RightJoin(_empty, _keySelector, _keySelector, (a, b) => b)
            .Count();

        // B82q: ZLinq has no FullJoin
        // B82r: ZLinq has no FullJoin
    }
}
