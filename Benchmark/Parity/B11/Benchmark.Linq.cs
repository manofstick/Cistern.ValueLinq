using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class B11
    {
        System.Linq.IGrouping<int, int> _linqGrouping;
        System.Linq.ILookup<int, int> _linqLookup;

        void InitLinq()
        {
            var lookup = _array.ToLookup(x => x % 10);
            _linqLookup = lookup;
            _linqGrouping = (System.Linq.IGrouping<int, int>)lookup[3];
        }

        [Benchmark]
        public int B78a_Linq() =>
            _list
            .DefaultIfEmpty(-1)
            .First();

        [Benchmark]
        public int B78b_Linq() =>
            _list
            .DefaultIfEmpty(-1)
            .Last();

        [Benchmark]
        public int B78c_Linq() =>
            _list
            .DefaultIfEmpty(-1)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B78d_Linq() =>
            _list
            .DefaultIfEmpty(-1)
            .Count();

        [Benchmark]
        public int[] B78e_Linq() =>
            _list
            .DefaultIfEmpty(-1)
            .ToArray();

        [Benchmark]
        public List<int> B78f_Linq() =>
            _list
            .DefaultIfEmpty(-1)
            .ToList();

        [Benchmark]
        public bool B79a_Linq() =>
            _set
            .DefaultIfEmpty(-1)
            .Contains(_present);

        [Benchmark]
        public bool B79b_Linq() =>
            _empty
            .DefaultIfEmpty(-1)
            .Contains(-1);

        [Benchmark]
        public int B80a_Linq() =>
            _arrayList
            .Cast<int>()
            .Count();

        [Benchmark]
        public int[] B80b_Linq() =>
            _arrayList
            .Cast<int>()
            .ToArray();

        [Benchmark]
        public List<int> B80c_Linq() =>
            _arrayList
            .Cast<int>()
            .ToList();

        [Benchmark]
        public int B80d_Linq() =>
            _arrayList
            .Cast<int>()
            .TryGetNonEnumeratedCount(out var count) ? count : -1;

        [Benchmark]
        public int B81a_Linq() =>
            _jagged
            .SelectMany(x => x)
            .Count();

        [Benchmark]
        public bool B81b_Linq() =>
            _jagged
            .SelectMany(x => x)
            .Contains(_presentInJagged);

        [Benchmark]
        public int[] B81c_Linq() =>
            _jagged
            .SelectMany(x => x)
            .ToArray();

        [Benchmark]
        public List<int> B81d_Linq() =>
            _jagged
            .SelectMany(x => x)
            .ToList();

        [Benchmark]
        public int B82a_Linq() =>
            _array
            .GroupBy(_keySelector)
            .Count();

        [Benchmark]
        public int B82b_Linq() =>
            _array
            .GroupBy(_keySelector, _resultSelector)
            .Count();

        [Benchmark]
        public int B82c_Linq() =>
            _array
            .GroupBy(_keySelector)
            .ToArray()
            .Length;

        [Benchmark]
        public int B82d_Linq() =>
            _array
            .GroupBy(_keySelector)
            .ToList()
            .Count;

        [Benchmark]
        public int B82e_Linq() =>
            _array
            .ToLookup(_keySelector)
            .Count;

        [Benchmark]
        public bool B82f_Linq() =>
            _linqGrouping
            .Contains(_inGroup);

        [Benchmark]
        public int B82g_Linq() =>
            _linqGrouping
            .Count();

        [Benchmark]
        public int B82h_Linq() =>
            _linqGrouping
            .ElementAt(500);

        [Benchmark]
        public int[] B82i_Linq() =>
            _linqGrouping
            .ToArray();

        [Benchmark]
        public int B82j_Linq() =>
            _linqLookup
            .Count();

        [Benchmark]
        public int B82k_Linq() =>
            _linqLookup
            .ToList()
            .Count;

        [Benchmark]
        public int B82l_Linq() =>
            _array
            .Join(_empty, _keySelector, _keySelector, (a, b) => a + b)
            .Count();

        [Benchmark]
        public int B82m_Linq() =>
            _empty
            .Join(_array, _keySelector, _keySelector, (a, b) => a + b)
            .Count();

        [Benchmark]
        public int B82n_Linq() =>
            _empty
            .GroupJoin(_array, _keySelector, _keySelector, (a, g) => a)
            .Count();

        [Benchmark]
        public int B82o_Linq() =>
            _empty
            .LeftJoin(_array, _keySelector, _keySelector, (a, b) => a + b)
            .Count();

        [Benchmark]
        public int B82p_Linq() =>
            _array
            .RightJoin(_empty, _keySelector, _keySelector, (a, b) => b)
            .Count();

        [Benchmark]
        public int B82q_Linq() =>
            _emptyArray
            .FullJoin(_array, _keySelector, _keySelector, (a, b) => b)
            .Count();

        [Benchmark]
        public int B82r_Linq() =>
            _array
            .FullJoin(_array, x => x, x => x, (a, b) => a + b)
            .Count();
    }
}
