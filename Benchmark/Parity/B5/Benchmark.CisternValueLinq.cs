using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B5
    {
        [Benchmark]
        public int B34a_CisternValueLinq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .First();

        [Benchmark]
        public int B34b_CisternValueLinq() =>
            Data.Iterator(_values)
            .OrderByDescending(_key)
            .First();

        [Benchmark]
        public int B34c_CisternValueLinq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .FirstOrDefault();

        [Benchmark]
        public int B34d_CisternValueLinq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .ElementAt(0);

        [Benchmark]
        public int B35a_CisternValueLinq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .Last();

        [Benchmark]
        public int B35b_CisternValueLinq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .LastOrDefault();

        [Benchmark]
        public int B36a_CisternValueLinq() =>
            _values
            .OrderBy(_tiedKey)
            .ThenBy(_key)
            .First();

        [Benchmark]
        public int B36b_CisternValueLinq() =>
            _values
            .OrderBy(_tiedKey)
            .ThenBy(_key)
            .Last();

        [Benchmark]
        public int B37_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .Last(_predicate);

        [Benchmark]
        public int B38_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .ElementAt(Length / 2);

        [Benchmark]
        public int[] B39a_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .ToArray();

        [Benchmark]
        public List<int> B39b_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .ToList();

        [Benchmark]
        public int[] B39c_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .Take(100..200)
            .ToArray();

        [Benchmark]
        public int B39d_CisternValueLinq()
        {
            var sum = 0;
            foreach (var x in _values.OrderBy(_key).Skip(100).Take(100))
                sum += x;
            return sum;
        }

        [Benchmark]
        public int[] B39e_CisternValueLinq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .ToArray();

        [Benchmark]
        public int[] B39f_CisternValueLinq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .ToArray();

        [Benchmark]
        public int[] B40a_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .Skip(500)
            .Take(1)
            .ToArray();

        [Benchmark]
        public int B40b_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .Skip(500)
            .First();

        [Benchmark]
        public int B41_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .Last();

        [Benchmark]
        public int B42a_CisternValueLinq() =>
            _values
            .Order()
            .First();

        [Benchmark]
        public int B42b_CisternValueLinq() =>
            _values
            .Order()
            .Last();

        [Benchmark]
        public int B42c_CisternValueLinq() =>
            _list
            .Order()
            .First();

        [Benchmark]
        public int B42d_CisternValueLinq() =>
            _list
            .Order()
            .Last();

        [Benchmark]
        public int B43a_CisternValueLinq() =>
            Data.Iterator(_values)
            .Order()
            .First();

        [Benchmark]
        public int B43b_CisternValueLinq() =>
            Data.Iterator(_values)
            .Order()
            .Last();

        [Benchmark]
        public int[] B44a_CisternValueLinq() =>
            _values
            .Order()
            .ToArray();

        [Benchmark]
        public List<int> B44b_CisternValueLinq() =>
            _values
            .Order()
            .ToList();

        [Benchmark]
        public int B45a_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .Count();

        [Benchmark]
        public int B45b_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public bool B45c_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .Any();

        [Benchmark]
        public bool B46_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .Contains(_present);

        [Benchmark]
        public int[] B47a_CisternValueLinq() =>
            _single
            .OrderBy(_key)
            .ToArray();

        [Benchmark]
        public List<int> B47b_CisternValueLinq() =>
            _single
            .OrderBy(_key)
            .ToList();

        [Benchmark]
        public string[] B48a_CisternValueLinq() =>
            _strings
            .OrderBy(_stringKey)
            .ToArray();

        [Benchmark]
        public int[] B48b_CisternValueLinq() =>
            _values
            .OrderBy(_key)
            .ToArray();
    }
}
