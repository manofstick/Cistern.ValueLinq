using BenchmarkDotNet.Attributes;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class F
    {
        [Benchmark]
        public int[] F01a_Linq() =>
            _iterator
            .Where(_half)
            .ToArray();

        [Benchmark]
        public List<int> F01b_Linq() =>
            _iterator
            .Select(_double)
            .ToList();

        [Benchmark]
        public string[] F01c_Linq() =>
            _mixed
            .OfType<string>()
            .ToArray();

        [Benchmark]
        public List<int> F01d_Linq() =>
            _iterator
            .Skip(10)
            .Take(5000)
            .ToList();

        [Benchmark]
        public int[] F02a_Linq() =>
            _tenIterator
            .Concat(_collection)
            .ToArray();

        [Benchmark]
        public int[] F02b_Linq() =>
            _tenIterator
            .Concat(_set)
            .ToArray();

        [Benchmark]
        public int[] F02c_Linq() =>
            _tenIterator
            .Concat(_collection)
            .Concat(_collection)
            .ToArray();

        [Benchmark]
        public int[] F02d_Linq() =>
            _tenIterator
            .Concat(_tenIterator)
            .Concat(_collection)
            .ToArray();

        [Benchmark]
        public int[] F02e_Linq() =>
            _tenIterator
            .Concat(_collection)
            .Concat(_tenIterator)
            .ToArray();

        [Benchmark]
        public int[] F02f_Linq() =>
            _collection
            .Concat(_tenIterator)
            .ToArray();

        [Benchmark]
        public List<int> F02g_Linq() =>
            _tenIterator
            .Concat(_collection)
            .Concat(_collection)
            .ToList();

        [Benchmark]
        public int[] F03a_Linq() =>
            _iterator
            .Append(1)
            .Append(2)
            .ToArray();

        [Benchmark]
        public List<int> F03b_Linq() =>
            _iterator
            .Prepend(0)
            .Append(1)
            .ToList();

        [Benchmark]
        public List<int> F04a_Linq() =>
            Enumerable.Range(0, Length)
            .Select(_double)
            .ToList();

        [Benchmark]
        public List<int> F04b_Linq() =>
            _collection
            .Select(_double)
            .ToList();

        [Benchmark]
        public int[] F05a_Linq() =>
            _set
            .ToArray();

        [Benchmark]
        public int[] F05b_Linq() =>
            _emptyCollection
            .ToArray();

        [Benchmark]
        public int[] F05c_Linq() =>
            _collection
            .ToArray();

        [Benchmark]
        public int[] F06a_Linq() =>
            _ints
            .OrderBy(_identity)
            .ToArray();

        [Benchmark]
        public List<int> F06b_Linq() =>
            _ints
            .OrderBy(_identity)
            .ToList();

        [Benchmark]
        public int F06c_Linq()
        {
            var sum = 0;
            foreach (var x in _ints.OrderBy(_identity))
                sum += x;
            return sum;
        }

        [Benchmark]
        public int F07a_Linq() =>
            _ints
            .GroupBy(_key, _keyOfGroup)
            .Sum();

        [Benchmark]
        public int F07b_Linq() =>
            _ints
            .ToLookup(_key)
            .Count;

        [Benchmark]
        public int F08a_Linq() =>
            _ints
            .Join(_inner, _key, _key, _add)
            .Sum();

        [Benchmark]
        public int F08b_Linq() =>
            _ints
            .LeftJoin(_inner, _key, _key, _add)
            .Sum();

        [Benchmark]
        public int F09_Linq()
        {
            var sum = 0;
            foreach (var g in _ints.GroupBy(_key))
                foreach (var x in g)
                    sum += x;
            return sum;
        }

        [Benchmark]
        public int[] F11a_Linq() =>
            _intList
            .DefaultIfEmpty(0)
            .ToArray();

        [Benchmark]
        public List<int> F11b_Linq() =>
            _iterator
            .DefaultIfEmpty(0)
            .ToList();

        [Benchmark]
        public int F12a_Linq()
        {
            IEnumerable<int> e = _ints.Where(_half);
            var sum = 0;
            foreach (var x in e)
                sum += x;
            return sum;
        }

        [Benchmark]
        public int F12b_Linq()
        {
            IEnumerable<int> e = _iterator.Select(_double);
            var sum = 0;
            foreach (var x in e)
                sum += x;
            return sum;
        }
    }
}
