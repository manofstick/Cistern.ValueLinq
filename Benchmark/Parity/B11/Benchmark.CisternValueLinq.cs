using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B11
    {
        System.Linq.IGrouping<int, int> _cisternGrouping;
        System.Linq.ILookup<int, int> _cisternLookup;

        void InitCistern()
        {
            var lookup = _array.ToLookup(x => x % 10);
            _cisternLookup = lookup;
            _cisternGrouping = (System.Linq.IGrouping<int, int>)lookup[3];
        }

        [Benchmark]
        public int B78a_CisternValueLinq() =>
            _list
            .DefaultIfEmpty(-1)
            .First();

        [Benchmark]
        public int B78b_CisternValueLinq() =>
            _list
            .DefaultIfEmpty(-1)
            .Last();

        [Benchmark]
        public int B78c_CisternValueLinq() =>
            _list
            .DefaultIfEmpty(-1)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B78d_CisternValueLinq() =>
            _list
            .DefaultIfEmpty(-1)
            .Count();

        [Benchmark]
        public int[] B78e_CisternValueLinq() =>
            _list
            .DefaultIfEmpty(-1)
            .ToArray();

        [Benchmark]
        public List<int> B78f_CisternValueLinq() =>
            _list
            .DefaultIfEmpty(-1)
            .ToList();

        [Benchmark]
        public bool B79a_CisternValueLinq() =>
            _set
            .DefaultIfEmpty(-1)
            .Contains(_present);

        [Benchmark]
        public bool B79b_CisternValueLinq() =>
            _empty
            .DefaultIfEmpty(-1)
            .Contains(-1);

        [Benchmark]
        public int B80a_CisternValueLinq() =>
            _arrayList
            .Cast<int>()
            .Count();

        [Benchmark]
        public int[] B80b_CisternValueLinq() =>
            _arrayList
            .Cast<int>()
            .ToArray();

        [Benchmark]
        public List<int> B80c_CisternValueLinq() =>
            _arrayList
            .Cast<int>()
            .ToList();

        [Benchmark]
        public int B80d_CisternValueLinq() =>
            _arrayList
            .Cast<int>()
            .TryGetNonEnumeratedCount(out var count) ? count : -1;

        [Benchmark]
        public int B81a_CisternValueLinq() =>
            _jagged
            .SelectMany(x => x)
            .Count();

        [Benchmark]
        public bool B81b_CisternValueLinq() =>
            _jagged
            .SelectMany(x => x)
            .Contains(_presentInJagged);

        [Benchmark]
        public int[] B81c_CisternValueLinq() =>
            _jagged
            .SelectMany(x => x)
            .ToArray();

        [Benchmark]
        public List<int> B81d_CisternValueLinq() =>
            _jagged
            .SelectMany(x => x)
            .ToList();

        [Benchmark]
        public int B82a_CisternValueLinq() =>
            _array
            .GroupBy(_keySelector)
            .Count();

        [Benchmark]
        public int B82b_CisternValueLinq() =>
            _array
            .GroupBy(_keySelector, _resultSelector)
            .Count();

        [Benchmark]
        public int B82c_CisternValueLinq() =>
            _array
            .GroupBy(_keySelector)
            .ToArray()
            .Length;

        [Benchmark]
        public int B82d_CisternValueLinq() =>
            _array
            .GroupBy(_keySelector)
            .ToList()
            .Count;

        [Benchmark]
        public int B82e_CisternValueLinq() =>
            _array
            .ToLookup(_keySelector)
            .Count;

        [Benchmark]
        public bool B82f_CisternValueLinq() =>
            _cisternGrouping
            .Contains(_inGroup);

        [Benchmark]
        public int B82g_CisternValueLinq() =>
            _cisternGrouping
            .Count();

        [Benchmark]
        public int B82h_CisternValueLinq() =>
            _cisternGrouping
            .ElementAt(500);

        [Benchmark]
        public int[] B82i_CisternValueLinq() =>
            _cisternGrouping
            .ToArray();

        [Benchmark]
        public int B82j_CisternValueLinq() =>
            _cisternLookup
            .Count();

        [Benchmark]
        public int B82k_CisternValueLinq() =>
            _cisternLookup
            .ToList()
            .Count;

        [Benchmark]
        public int B82l_CisternValueLinq() =>
            _array
            .Join(_empty, _keySelector, _keySelector, (a, b) => a + b)
            .Count();

        [Benchmark]
        public int B82m_CisternValueLinq() =>
            _empty
            .Join(_array, _keySelector, _keySelector, (a, b) => a + b)
            .Count();

        [Benchmark]
        public int B82n_CisternValueLinq() =>
            _empty
            .GroupJoin(_array, _keySelector, _keySelector, (a, g) => a)
            .Count();

        [Benchmark]
        public int B82o_CisternValueLinq() =>
            _empty
            .LeftJoin(_array, _keySelector, _keySelector, (a, b) => a + b)
            .Count();

        [Benchmark]
        public int B82p_CisternValueLinq() =>
            _array
            .RightJoin(_empty, _keySelector, _keySelector, (a, b) => b)
            .Count();

        [Benchmark]
        public int B82q_CisternValueLinq() =>
            _emptyArray
            .FullJoin(_array, _keySelector, _keySelector, (a, b) => b)
            .Count();

        [Benchmark]
        public int B82r_CisternValueLinq() =>
            _array
            .FullJoin(_array, x => x, x => x, (a, b) => a + b)
            .Count();
    }
}
