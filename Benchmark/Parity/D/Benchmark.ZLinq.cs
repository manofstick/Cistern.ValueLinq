using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class D
    {
        [Benchmark]
        public int D01a_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .Sum();

        [Benchmark]
        public int D01b_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Sum();

        [Benchmark]
        public long D01c_ZLinq() =>
            _longs
            .AsValueEnumerable()
            .Sum();

        [Benchmark]
        public long D01d_ZLinq() =>
            _longList
            .AsValueEnumerable()
            .Sum();

        [Benchmark]
        public float D02a_ZLinq() =>
            _floats
            .AsValueEnumerable()
            .Sum();

        [Benchmark]
        public double D02b_ZLinq() =>
            _doubleList
            .AsValueEnumerable()
            .Sum();

        [Benchmark]
        public decimal D02c_ZLinq() =>
            _decimals
            .AsValueEnumerable()
            .Sum();

        [Benchmark]
        public double D03a_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .Average();

        [Benchmark]
        public double D03b_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Average();

        [Benchmark]
        public double D04a_ZLinq() =>
            _longList
            .AsValueEnumerable()
            .Average();

        [Benchmark]
        public float D04b_ZLinq() =>
            _floats
            .AsValueEnumerable()
            .Average();

        [Benchmark]
        public double D04c_ZLinq() =>
            _doubleList
            .AsValueEnumerable()
            .Average();

        [Benchmark]
        public decimal D04d_ZLinq() =>
            _decimals
            .AsValueEnumerable()
            .Average();

        [Benchmark]
        public float D05a_ZLinq() =>
            _floats
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public double D05b_ZLinq() =>
            _doubleList
            .AsValueEnumerable()
            .Max();

        [Benchmark]
        public decimal D06a_ZLinq() =>
            _decimals
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public decimal D06b_ZLinq() =>
            _decimalList
            .AsValueEnumerable()
            .Max();

        [Benchmark]
        public int D07a_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public long D07b_ZLinq() =>
            _longList
            .AsValueEnumerable()
            .Max();

        [Benchmark]
        public short D07c_ZLinq() =>
            _shorts
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public int D07d_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Max(Comparer<int>.Default);

        [Benchmark]
        public int D08a_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .Aggregate(_add);

        [Benchmark]
        public long D08b_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Aggregate(0L, _addLong);

        [Benchmark]
        public int D08c_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .Aggregate(0L, _addLong, _half);

        [Benchmark]
        public bool D08d_ZLinq() =>
            _found
            .AsValueEnumerable()
            .Any(_isLast);

        [Benchmark]
        public bool D08e_ZLinq() =>
            _foundList
            .AsValueEnumerable()
            .All(_belowRange);

        [Benchmark]
        public int D08f_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .Count(_predicate);

        [Benchmark]
        public int D08g_ZLinq() =>
            _foundList
            .AsValueEnumerable()
            .First(_isLast);

        [Benchmark]
        public int D08h_ZLinq() =>
            _found
            .AsValueEnumerable()
            .FirstOrDefault(_isLast);

        [Benchmark]
        public int D08i_ZLinq() =>
            _foundList
            .AsValueEnumerable()
            .Single(_isLast);

        [Benchmark]
        public int D08j_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Aggregate(_add);

        [Benchmark]
        public long D08k_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .Aggregate(0L, _addLong);

        [Benchmark]
        public int D08l_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Aggregate(0L, _addLong, _half);

        [Benchmark]
        public bool D08m_ZLinq() =>
            _foundList
            .AsValueEnumerable()
            .Any(_isLast);

        [Benchmark]
        public bool D08n_ZLinq() =>
            _found
            .AsValueEnumerable()
            .All(_belowRange);

        [Benchmark]
        public int D08o_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Count(_predicate);

        [Benchmark]
        public int D08p_ZLinq() =>
            _found
            .AsValueEnumerable()
            .First(_isLast);

        [Benchmark]
        public int D08q_ZLinq() =>
            _foundList
            .AsValueEnumerable()
            .FirstOrDefault(_isLast);

        [Benchmark]
        public int D08r_ZLinq() =>
            _found
            .AsValueEnumerable()
            .Single(_isLast);

        [Benchmark]
        public int D11a_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .Count();

        [Benchmark]
        public int[] D11b_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .ToArray();

        [Benchmark]
        public List<int> D11c_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .ToList();

        [Benchmark]
        public int D11d_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .First();

        [Benchmark]
        public int D11e_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .Last();

        [Benchmark]
        public int D11f_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .ElementAt(2000);

        [Benchmark]
        public bool D11g_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .Contains(Data.Range - 1);

        [Benchmark]
        public int D11h_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .Count();

        [Benchmark]
        public int[] D11i_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .ToArray();

        [Benchmark]
        public List<int> D11j_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .ToList();

        [Benchmark]
        public int D11k_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .First();

        [Benchmark]
        public int D11l_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .Last();

        [Benchmark]
        public int D11m_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .ElementAt(2000);

        [Benchmark]
        public bool D11n_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .Contains(2);

        [Benchmark]
        public int[] D11o_ZLinq() =>
            _sortedList
            .AsValueEnumerable()
            .Where(_ninety)
            .ToArray();

        [Benchmark]
        public List<int> D11p_ZLinq() =>
            _sortedList
            .AsValueEnumerable()
            .Where(_ninety)
            .ToList();

        [Benchmark]
        public int[] D12a_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Select(_selector)
            .ToArray();

        [Benchmark]
        public List<int> D12b_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .Select(_selector)
            .ToList();

        [Benchmark]
        public int[] D15a_ZLinq() =>
            _arrays
            .AsValueEnumerable()
            .SelectMany(_flattenArray)
            .ToArray();

        [Benchmark]
        public int[] D15b_ZLinq() =>
            _lists
            .AsValueEnumerable()
            .SelectMany(_flattenList)
            .ToArray();

        [Benchmark]
        public int[] D15c_ZLinq() =>
            _short
            .AsValueEnumerable()
            .Concat(_intList)
            .ToArray();
    }
}
