using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class D
    {
        [Benchmark]
        public int D01a_CisternValueLinq() =>
            _ints
            .Sum();

        [Benchmark]
        public int D01b_CisternValueLinq() =>
            _intList
            .Sum();

        [Benchmark]
        public long D01c_CisternValueLinq() =>
            _longs
            .Sum();

        [Benchmark]
        public long D01d_CisternValueLinq() =>
            _longList
            .Sum();

        [Benchmark]
        public float D02a_CisternValueLinq() =>
            _floats
            .Sum();

        [Benchmark]
        public double D02b_CisternValueLinq() =>
            _doubleList
            .Sum();

        [Benchmark]
        public double D02b_CisternValueLinqFastest() =>
            _doubleList
            .Sum(ValueLinq.Maths.SIMDOptions.Fastest);

        [Benchmark]
        public decimal D02c_CisternValueLinq() =>
            _decimals
            .Sum();

        [Benchmark]
        public double D03a_CisternValueLinq() =>
            _ints
            .Average();

        [Benchmark]
        public double D03b_CisternValueLinq() =>
            _intList
            .Average();

        [Benchmark]
        public double D04a_CisternValueLinq() =>
            _longList
            .Average();

        [Benchmark]
        public float D04b_CisternValueLinq() =>
            _floats
            .Average();

        [Benchmark]
        public double D04c_CisternValueLinq() =>
            _doubleList
            .Average();

        [Benchmark]
        public decimal D04d_CisternValueLinq() =>
            _decimals
            .Average();

        [Benchmark]
        public float D05a_CisternValueLinq() =>
            _floats
            .Min();

        [Benchmark]
        public double D05b_CisternValueLinq() =>
            _doubleList
            .Max();

        [Benchmark]
        public decimal D06a_CisternValueLinq() =>
            _decimals
            .Min();

        [Benchmark]
        public decimal D06b_CisternValueLinq() =>
            _decimalList
            .Max();

        [Benchmark]
        public int D07a_CisternValueLinq() =>
            _ints
            .Min();

        [Benchmark]
        public long D07b_CisternValueLinq() =>
            _longList
            .Max();

        [Benchmark]
        public short D07c_CisternValueLinq() =>
            _shorts
            .Min();

        [Benchmark]
        public int D07d_CisternValueLinq() =>
            _intList
            .Max(Comparer<int>.Default);

        [Benchmark]
        public int D08a_CisternValueLinq() =>
            _ints
            .Aggregate(_add);

        [Benchmark]
        public long D08b_CisternValueLinq() =>
            _intList
            .Aggregate(0L, _addLong);

        [Benchmark]
        public int D08c_CisternValueLinq() =>
            _ints
            .Aggregate(0L, _addLong, _half);

        [Benchmark]
        public bool D08d_CisternValueLinq() =>
            _found
            .Any(_isLast);

        [Benchmark]
        public bool D08e_CisternValueLinq() =>
            _foundList
            .All(_belowRange);

        [Benchmark]
        public int D08f_CisternValueLinq() =>
            _ints
            .Count(_predicate);

        [Benchmark]
        public int D08g_CisternValueLinq() =>
            _foundList
            .First(_isLast);

        [Benchmark]
        public int D08h_CisternValueLinq() =>
            _found
            .FirstOrDefault(_isLast);

        [Benchmark]
        public int D08i_CisternValueLinq() =>
            _foundList
            .Single(_isLast);

        [Benchmark]
        public int D08j_CisternValueLinq() =>
            _intList
            .Aggregate(_add);

        [Benchmark]
        public long D08k_CisternValueLinq() =>
            _ints
            .Aggregate(0L, _addLong);

        [Benchmark]
        public int D08l_CisternValueLinq() =>
            _intList
            .Aggregate(0L, _addLong, _half);

        [Benchmark]
        public bool D08m_CisternValueLinq() =>
            _foundList
            .Any(_isLast);

        [Benchmark]
        public bool D08n_CisternValueLinq() =>
            _found
            .All(_belowRange);

        [Benchmark]
        public int D08o_CisternValueLinq() =>
            _intList
            .Count(_predicate);

        [Benchmark]
        public int D08p_CisternValueLinq() =>
            _found
            .First(_isLast);

        [Benchmark]
        public int D08q_CisternValueLinq() =>
            _foundList
            .FirstOrDefault(_isLast);

        [Benchmark]
        public int D08r_CisternValueLinq() =>
            _found
            .Single(_isLast);

        [Benchmark]
        public int D11a_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .Count();

        [Benchmark]
        public int[] D11b_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .ToArray();

        [Benchmark]
        public List<int> D11c_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .ToList();

        [Benchmark]
        public int D11d_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .First();

        [Benchmark]
        public int D11e_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .Last();

        [Benchmark]
        public int D11f_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .ElementAt(2000);

        [Benchmark]
        public bool D11g_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .Contains(Data.Range - 1);

        [Benchmark]
        public int D11h_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .Count();

        [Benchmark]
        public int[] D11i_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .ToArray();

        [Benchmark]
        public List<int> D11j_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .ToList();

        [Benchmark]
        public int D11k_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .First();

        [Benchmark]
        public int D11l_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .Last();

        [Benchmark]
        public int D11m_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .ElementAt(2000);

        [Benchmark]
        public bool D11n_CisternValueLinq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .Contains(2);

        [Benchmark]
        public int[] D11o_CisternValueLinq() =>
            _sortedList
            .Where(_ninety)
            .ToArray();

        [Benchmark]
        public List<int> D11p_CisternValueLinq() =>
            _sortedList
            .Where(_ninety)
            .ToList();

        [Benchmark]
        public int[] D12a_CisternValueLinq() =>
            _intList
            .Select(_selector)
            .ToArray();

        [Benchmark]
        public List<int> D12b_CisternValueLinq() =>
            _intList
            .Select(_selector)
            .ToList();

        [Benchmark]
        public int[] D15a_CisternValueLinq() =>
            _arrays
            .SelectMany(_flattenArray)
            .ToArray();

        [Benchmark]
        public int[] D15b_CisternValueLinq() =>
            _lists
            .SelectMany(_flattenList)
            .ToArray();

        [Benchmark]
        public int[] D15c_CisternValueLinq() =>
            _short
            .Concat(_intList)
            .ToArray();
    }
}
