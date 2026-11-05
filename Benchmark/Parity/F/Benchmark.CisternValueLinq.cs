using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class F
    {
        [Benchmark]
        public int[] F01a_CisternValueLinq() =>
            _iterator
            .Where(_half)
            .ToArray();

        [Benchmark]
        public List<int> F01b_CisternValueLinq() =>
            _iterator
            .Select(_double)
            .ToList();

        [Benchmark]
        public string[] F01c_CisternValueLinq() =>
            _mixed
            .OfType<string>()
            .ToArray();

        [Benchmark]
        public List<int> F01d_CisternValueLinq() =>
            _iterator
            .Skip(10)
            .Take(5000)
            .ToList();

        [Benchmark]
        public int[] F02a_CisternValueLinq() =>
            _tenIterator
            .Concat(_collection)
            .ToArray();

        [Benchmark]
        public int[] F02b_CisternValueLinq() =>
            _tenIterator
            .Concat(_set)
            .ToArray();

        [Benchmark]
        public int[] F02c_CisternValueLinq() =>
            _tenIterator
            .Concat(_collection)
            .Concat(_collection)
            .ToArray();

        [Benchmark]
        public int[] F02d_CisternValueLinq() =>
            _tenIterator
            .Concat(_tenIterator)
            .Concat(_collection)
            .ToArray();

        [Benchmark]
        public int[] F02e_CisternValueLinq() =>
            _tenIterator
            .Concat(_collection)
            .Concat(_tenIterator)
            .ToArray();

        [Benchmark]
        public int[] F02f_CisternValueLinq() =>
            _collection
            .Concat(_tenIterator)
            .ToArray();

        [Benchmark]
        public List<int> F02g_CisternValueLinq() =>
            _tenIterator
            .Concat(_collection)
            .Concat(_collection)
            .ToList();

        [Benchmark]
        public int[] F03a_CisternValueLinq() =>
            _iterator
            .Append(1)
            .Append(2)
            .ToArray();

        [Benchmark]
        public List<int> F03b_CisternValueLinq() =>
            _iterator
            .Prepend(0)
            .Append(1)
            .ToList();

        [Benchmark]
        public List<int> F04a_CisternValueLinq() =>
            Enumerable.Range(0, Length)
            .Select(_double)
            .ToList();

        [Benchmark]
        public List<int> F04b_CisternValueLinq() =>
            _collection
            .Select(_double)
            .ToList();

        [Benchmark]
        public int[] F05a_CisternValueLinq() =>
            _set
            .ToArray();

        [Benchmark]
        public int[] F05b_CisternValueLinq() =>
            _emptyCollection
            .ToArray();

        [Benchmark]
        public int[] F05c_CisternValueLinq() =>
            _collection
            .ToArray();

        [Benchmark]
        public int[] F06a_CisternValueLinq() =>
            _ints
            .OrderBy(_identity)
            .ToArray();

        [Benchmark]
        public List<int> F06b_CisternValueLinq() =>
            _ints
            .OrderBy(_identity)
            .ToList();

        [Benchmark]
        public int F06c_CisternValueLinq()
        {
            var sum = 0;
            foreach (var x in _ints.OrderBy(_identity))
                sum += x;
            return sum;
        }

        [Benchmark]
        public int F07a_CisternValueLinq() =>
            _ints
            .GroupBy(_key, _keyOfGroup)
            .Sum();

        [Benchmark]
        public int F07b_CisternValueLinq() =>
            _ints
            .ToLookup(_key)
            .Count;

        [Benchmark]
        public int F08a_CisternValueLinq() =>
            _ints
            .Join(_inner, _key, _key, _add)
            .Sum();

        [Benchmark]
        public int F08b_CisternValueLinq() =>
            _ints
            .LeftJoin(_inner, _key, _key, _add)
            .Sum();

        [Benchmark]
        public int F09_CisternValueLinq()
        {
            var sum = 0;
            foreach (var g in _ints.GroupBy(_key))
                foreach (var x in g)
                    sum += x;
            return sum;
        }

        [Benchmark]
        public int[] F11a_CisternValueLinq() =>
            _intList
            .DefaultIfEmpty(0)
            .ToArray();

        [Benchmark]
        public List<int> F11b_CisternValueLinq() =>
            _iterator
            .DefaultIfEmpty(0)
            .ToList();

        [Benchmark]
        public int F12a_CisternValueLinq()
        {
            IEnumerable<int> e = _ints.Where(_half);
            var sum = 0;
            foreach (var x in e)
                sum += x;
            return sum;
        }

        [Benchmark]
        public int F12b_CisternValueLinq()
        {
            IEnumerable<int> e = _iterator.Select(_double);
            var sum = 0;
            foreach (var x in e)
                sum += x;
            return sum;
        }
    }
}
