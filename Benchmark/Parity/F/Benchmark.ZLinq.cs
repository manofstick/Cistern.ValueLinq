using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class F
    {
        [Benchmark]
        public int[] F01a_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Where(_half)
            .ToArray();

        [Benchmark]
        public List<int> F01b_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Select(_double)
            .ToList();

        [Benchmark]
        public string[] F01c_ZLinq() =>
            _mixed
            .AsValueEnumerable()
            .OfType<string>()
            .ToArray();

        [Benchmark]
        public List<int> F01d_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Skip(10)
            .Take(5000)
            .ToList();

        [Benchmark]
        public int[] F02a_ZLinq() =>
            _tenIterator
            .AsValueEnumerable()
            .Concat(_collection)
            .ToArray();

        [Benchmark]
        public int[] F02b_ZLinq() =>
            _tenIterator
            .AsValueEnumerable()
            .Concat(_set)
            .ToArray();

        [Benchmark]
        public int[] F02c_ZLinq() =>
            _tenIterator
            .AsValueEnumerable()
            .Concat(_collection)
            .Concat(_collection)
            .ToArray();

        [Benchmark]
        public int[] F02d_ZLinq() =>
            _tenIterator
            .AsValueEnumerable()
            .Concat(_tenIterator)
            .Concat(_collection)
            .ToArray();

        [Benchmark]
        public int[] F02e_ZLinq() =>
            _tenIterator
            .AsValueEnumerable()
            .Concat(_collection)
            .Concat(_tenIterator)
            .ToArray();

        [Benchmark]
        public int[] F02f_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .Concat(_tenIterator)
            .ToArray();

        [Benchmark]
        public List<int> F02g_ZLinq() =>
            _tenIterator
            .AsValueEnumerable()
            .Concat(_collection)
            .Concat(_collection)
            .ToList();

        [Benchmark]
        public int[] F03a_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Append(1)
            .Append(2)
            .ToArray();

        [Benchmark]
        public List<int> F03b_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Prepend(0)
            .Append(1)
            .ToList();

        [Benchmark]
        public List<int> F04a_ZLinq() =>
            ValueEnumerable.Range(0, Length)
            .Select(_double)
            .ToList();

        [Benchmark]
        public List<int> F04b_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .Select(_double)
            .ToList();

        [Benchmark]
        public int[] F05a_ZLinq() =>
            _set
            .AsValueEnumerable()
            .ToArray();

        [Benchmark]
        public int[] F05b_ZLinq() =>
            _emptyCollection
            .AsValueEnumerable()
            .ToArray();

        [Benchmark]
        public int[] F05c_ZLinq() =>
            _collection
            .AsValueEnumerable()
            .ToArray();

        [Benchmark]
        public int[] F06a_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .OrderBy(_identity)
            .ToArray();

        [Benchmark]
        public List<int> F06b_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .OrderBy(_identity)
            .ToList();

        [Benchmark]
        public int F06c_ZLinq()
        {
            var sum = 0;
            foreach (var x in _ints.AsValueEnumerable().OrderBy(_identity))
                sum += x;
            return sum;
        }

        [Benchmark]
        public int F07a_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .GroupBy(_key, _keyOfGroup)
            .Sum();

        [Benchmark]
        public int F07b_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .ToLookup(_key)
            .Count;

        [Benchmark]
        public int F08a_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .Join(_inner, _key, _key, _add)
            .Sum();

        [Benchmark]
        public int F08b_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .LeftJoin(_inner, _key, _key, _add)
            .Sum();

        [Benchmark]
        public int F09_ZLinq()
        {
            var sum = 0;
            foreach (var g in _ints.AsValueEnumerable().GroupBy(_key))
                foreach (var x in g)
                    sum += x;
            return sum;
        }

        [Benchmark]
        public int[] F11a_ZLinq() =>
            _intList
            .AsValueEnumerable()
            .DefaultIfEmpty(0)
            .ToArray();

        [Benchmark]
        public List<int> F11b_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .DefaultIfEmpty(0)
            .ToList();

        [Benchmark]
        public int F12a_ZLinq()
        {
            var e = _ints.AsValueEnumerable().Where(_half);
            var sum = 0;
            foreach (var x in e)
                sum += x;
            return sum;
        }

        [Benchmark]
        public int F12b_ZLinq()
        {
            var e = _iterator.AsValueEnumerable().Select(_double);
            var sum = 0;
            foreach (var x in e)
                sum += x;
            return sum;
        }
    }
}
