using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B5
    {
        [Benchmark]
        public int B34a_ZLinq() =>
            Data.Iterator(_values)
            .AsValueEnumerable()
            .OrderBy(_key)
            .First();

        [Benchmark]
        public int B34b_ZLinq() =>
            Data.Iterator(_values)
            .AsValueEnumerable()
            .OrderByDescending(_key)
            .First();

        [Benchmark]
        public int B34c_ZLinq() =>
            Data.Iterator(_values)
            .AsValueEnumerable()
            .OrderBy(_key)
            .FirstOrDefault();

        [Benchmark]
        public int B34d_ZLinq() =>
            Data.Iterator(_values)
            .AsValueEnumerable()
            .OrderBy(_key)
            .ElementAt(0);

        [Benchmark]
        public int B35a_ZLinq() =>
            Data.Iterator(_values)
            .AsValueEnumerable()
            .OrderBy(_key)
            .Last();

        [Benchmark]
        public int B35b_ZLinq() =>
            Data.Iterator(_values)
            .AsValueEnumerable()
            .OrderBy(_key)
            .LastOrDefault();

        [Benchmark]
        public int B36a_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_tiedKey)
            .ThenBy(_key)
            .First();

        [Benchmark]
        public int B36b_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_tiedKey)
            .ThenBy(_key)
            .Last();

        [Benchmark]
        public int B37_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .Last(_predicate);

        [Benchmark]
        public int B38_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .ElementAt(Length / 2);

        [Benchmark]
        public int[] B39a_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .ToArray();

        [Benchmark]
        public List<int> B39b_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .ToList();

        [Benchmark]
        public int[] B39c_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .Take(100..200)
            .ToArray();

        [Benchmark]
        public int B39d_ZLinq()
        {
            var sum = 0;
            foreach (var x in _values.AsValueEnumerable().OrderBy(_key).Skip(100).Take(100))
                sum += x;
            return sum;
        }

        [Benchmark]
        public int[] B39e_ZLinq() =>
            Data.Iterator(_values)
            .AsValueEnumerable()
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .ToArray();

        [Benchmark]
        public int[] B39f_ZLinq() =>
            Data.Iterator(_values)
            .AsValueEnumerable()
            .OrderBy(_key)
            .ToArray();

        [Benchmark]
        public int[] B40a_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .Skip(500)
            .Take(1)
            .ToArray();

        [Benchmark]
        public int B40b_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .Skip(500)
            .First();

        [Benchmark]
        public int B41_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .Last();

        [Benchmark]
        public int B42a_ZLinq() =>
            _values
            .AsValueEnumerable()
            .Order()
            .First();

        [Benchmark]
        public int B42b_ZLinq() =>
            _values
            .AsValueEnumerable()
            .Order()
            .Last();

        [Benchmark]
        public int B42c_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Order()
            .First();

        [Benchmark]
        public int B42d_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Order()
            .Last();

        [Benchmark]
        public int B43a_ZLinq() =>
            Data.Iterator(_values)
            .AsValueEnumerable()
            .Order()
            .First();

        [Benchmark]
        public int B43b_ZLinq() =>
            Data.Iterator(_values)
            .AsValueEnumerable()
            .Order()
            .Last();

        [Benchmark]
        public int[] B44a_ZLinq() =>
            _values
            .AsValueEnumerable()
            .Order()
            .ToArray();

        [Benchmark]
        public List<int> B44b_ZLinq() =>
            _values
            .AsValueEnumerable()
            .Order()
            .ToList();

        [Benchmark]
        public int B45a_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .Count();

        [Benchmark]
        public int B45b_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public bool B45c_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .Any();

        [Benchmark]
        public bool B46_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .Contains(_present);

        [Benchmark]
        public int[] B47a_ZLinq() =>
            _single
            .AsValueEnumerable()
            .OrderBy(_key)
            .ToArray();

        [Benchmark]
        public List<int> B47b_ZLinq() =>
            _single
            .AsValueEnumerable()
            .OrderBy(_key)
            .ToList();

        [Benchmark]
        public string[] B48a_ZLinq() =>
            _strings
            .AsValueEnumerable()
            .OrderBy(_stringKey)
            .ToArray();

        [Benchmark]
        public int[] B48b_ZLinq() =>
            _values
            .AsValueEnumerable()
            .OrderBy(_key)
            .ToArray();
    }
}
