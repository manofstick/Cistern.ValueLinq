using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class D
    {
        [Benchmark]
        public int D01a_Linq() =>
            _ints
            .Sum();

        [Benchmark]
        public int D01b_Linq() =>
            _intList
            .Sum();

        [Benchmark]
        public long D01c_Linq() =>
            _longs
            .Sum();

        [Benchmark]
        public long D01d_Linq() =>
            _longList
            .Sum();

        [Benchmark]
        public float D02a_Linq() =>
            _floats
            .Sum();

        [Benchmark]
        public double D02b_Linq() =>
            _doubleList
            .Sum();

        [Benchmark]
        public decimal D02c_Linq() =>
            _decimals
            .Sum();

        [Benchmark]
        public double D03a_Linq() =>
            _ints
            .Average();

        [Benchmark]
        public double D03b_Linq() =>
            _intList
            .Average();

        [Benchmark]
        public double D04a_Linq() =>
            _longList
            .Average();

        [Benchmark]
        public float D04b_Linq() =>
            _floats
            .Average();

        [Benchmark]
        public double D04c_Linq() =>
            _doubleList
            .Average();

        [Benchmark]
        public decimal D04d_Linq() =>
            _decimals
            .Average();

        [Benchmark]
        public float D05a_Linq() =>
            _floats
            .Min();

        [Benchmark]
        public double D05b_Linq() =>
            _doubleList
            .Max();

        [Benchmark]
        public decimal D06a_Linq() =>
            _decimals
            .Min();

        [Benchmark]
        public decimal D06b_Linq() =>
            _decimalList
            .Max();

        [Benchmark]
        public int D07a_Linq() =>
            _ints
            .Min();

        [Benchmark]
        public long D07b_Linq() =>
            _longList
            .Max();

        [Benchmark]
        public short D07c_Linq() =>
            _shorts
            .Min();

        [Benchmark]
        public int D07d_Linq() =>
            _intList
            .Max(Comparer<int>.Default);

        [Benchmark]
        public int D08a_Linq() =>
            _ints
            .Aggregate(_add);

        [Benchmark]
        public long D08b_Linq() =>
            _intList
            .Aggregate(0L, _addLong);

        [Benchmark]
        public int D08c_Linq() =>
            _ints
            .Aggregate(0L, _addLong, _half);

        [Benchmark]
        public bool D08d_Linq() =>
            _found
            .Any(_isLast);

        [Benchmark]
        public bool D08e_Linq() =>
            _foundList
            .All(_belowRange);

        [Benchmark]
        public int D08f_Linq() =>
            _ints
            .Count(_predicate);

        [Benchmark]
        public int D08g_Linq() =>
            _foundList
            .First(_isLast);

        [Benchmark]
        public int D08h_Linq() =>
            _found
            .FirstOrDefault(_isLast);

        [Benchmark]
        public int D08i_Linq() =>
            _foundList
            .Single(_isLast);

        [Benchmark]
        public int D08j_Linq() =>
            _intList
            .Aggregate(_add);

        [Benchmark]
        public long D08k_Linq() =>
            _ints
            .Aggregate(0L, _addLong);

        [Benchmark]
        public int D08l_Linq() =>
            _intList
            .Aggregate(0L, _addLong, _half);

        [Benchmark]
        public bool D08m_Linq() =>
            _foundList
            .Any(_isLast);

        [Benchmark]
        public bool D08n_Linq() =>
            _found
            .All(_belowRange);

        [Benchmark]
        public int D08o_Linq() =>
            _intList
            .Count(_predicate);

        [Benchmark]
        public int D08p_Linq() =>
            _found
            .First(_isLast);

        [Benchmark]
        public int D08q_Linq() =>
            _foundList
            .FirstOrDefault(_isLast);

        [Benchmark]
        public int D08r_Linq() =>
            _found
            .Single(_isLast);

        [Benchmark]
        public int D11a_Linq() =>
            _intList
            .Where(_predicate)
            .Count();

        [Benchmark]
        public int[] D11b_Linq() =>
            _intList
            .Where(_predicate)
            .ToArray();

        [Benchmark]
        public List<int> D11c_Linq() =>
            _intList
            .Where(_predicate)
            .ToList();

        [Benchmark]
        public int D11d_Linq() =>
            _intList
            .Where(_predicate)
            .First();

        [Benchmark]
        public int D11e_Linq() =>
            _intList
            .Where(_predicate)
            .Last();

        [Benchmark]
        public int D11f_Linq() =>
            _intList
            .Where(_predicate)
            .ElementAt(2000);

        [Benchmark]
        public bool D11g_Linq() =>
            _intList
            .Where(_predicate)
            .Contains(Data.Range - 1);

        [Benchmark]
        public int D11h_Linq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .Count();

        [Benchmark]
        public int[] D11i_Linq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .ToArray();

        [Benchmark]
        public List<int> D11j_Linq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .ToList();

        [Benchmark]
        public int D11k_Linq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .First();

        [Benchmark]
        public int D11l_Linq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .Last();

        [Benchmark]
        public int D11m_Linq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .ElementAt(2000);

        [Benchmark]
        public bool D11n_Linq() =>
            _intList
            .Where(_predicate)
            .Select(_selector)
            .Contains(2);

        [Benchmark]
        public int[] D11o_Linq() =>
            _sortedList
            .Where(_ninety)
            .ToArray();

        [Benchmark]
        public List<int> D11p_Linq() =>
            _sortedList
            .Where(_ninety)
            .ToList();

        [Benchmark]
        public int[] D12a_Linq() =>
            _intList
            .Select(_selector)
            .ToArray();

        [Benchmark]
        public List<int> D12b_Linq() =>
            _intList
            .Select(_selector)
            .ToList();

        [Benchmark]
        public int[] D15a_Linq() =>
            _arrays
            .SelectMany(_flattenArray)
            .ToArray();

        [Benchmark]
        public int[] D15b_Linq() =>
            _lists
            .SelectMany(_flattenList)
            .ToArray();

        [Benchmark]
        public int[] D15c_Linq() =>
            _short
            .Concat(_intList)
            .ToArray();
    }
}
