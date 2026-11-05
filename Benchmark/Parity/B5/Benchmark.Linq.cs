using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class B5
    {
        [Benchmark]
        public int B34a_Linq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .First();

        [Benchmark]
        public int B34b_Linq() =>
            Data.Iterator(_values)
            .OrderByDescending(_key)
            .First();

        [Benchmark]
        public int B34c_Linq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .FirstOrDefault();

        [Benchmark]
        public int B34d_Linq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .ElementAt(0);

        [Benchmark]
        public int B35a_Linq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .Last();

        [Benchmark]
        public int B35b_Linq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .LastOrDefault();

        [Benchmark]
        public int B36a_Linq() =>
            _values
            .OrderBy(_tiedKey)
            .ThenBy(_key)
            .First();

        [Benchmark]
        public int B36b_Linq() =>
            _values
            .OrderBy(_tiedKey)
            .ThenBy(_key)
            .Last();

        [Benchmark]
        public int B37_Linq() =>
            _values
            .OrderBy(_key)
            .Last(_predicate);

        [Benchmark]
        public int B38_Linq() =>
            _values
            .OrderBy(_key)
            .ElementAt(Length / 2);

        [Benchmark]
        public int[] B39a_Linq() =>
            _values
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .ToArray();

        [Benchmark]
        public List<int> B39b_Linq() =>
            _values
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .ToList();

        [Benchmark]
        public int[] B39c_Linq() =>
            _values
            .OrderBy(_key)
            .Take(100..200)
            .ToArray();

        [Benchmark]
        public int B39d_Linq()
        {
            var sum = 0;
            foreach (var x in _values.OrderBy(_key).Skip(100).Take(100))
                sum += x;
            return sum;
        }

        [Benchmark]
        public int[] B39e_Linq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .ToArray();

        [Benchmark]
        public int[] B39f_Linq() =>
            Data.Iterator(_values)
            .OrderBy(_key)
            .ToArray();

        [Benchmark]
        public int[] B40a_Linq() =>
            _values
            .OrderBy(_key)
            .Skip(500)
            .Take(1)
            .ToArray();

        [Benchmark]
        public int B40b_Linq() =>
            _values
            .OrderBy(_key)
            .Skip(500)
            .First();

        [Benchmark]
        public int B41_Linq() =>
            _values
            .OrderBy(_key)
            .Skip(100)
            .Take(100)
            .Last();

        [Benchmark]
        public int B42a_Linq() =>
            _values
            .Order()
            .First();

        [Benchmark]
        public int B42b_Linq() =>
            _values
            .Order()
            .Last();

        [Benchmark]
        public int B42c_Linq() =>
            _list
            .Order()
            .First();

        [Benchmark]
        public int B42d_Linq() =>
            _list
            .Order()
            .Last();

        [Benchmark]
        public int B43a_Linq() =>
            Data.Iterator(_values)
            .Order()
            .First();

        [Benchmark]
        public int B43b_Linq() =>
            Data.Iterator(_values)
            .Order()
            .Last();

        [Benchmark]
        public int[] B44a_Linq() =>
            _values
            .Order()
            .ToArray();

        [Benchmark]
        public List<int> B44b_Linq() =>
            _values
            .Order()
            .ToList();

        [Benchmark]
        public int B45a_Linq() =>
            _values
            .OrderBy(_key)
            .Count();

        [Benchmark]
        public int B45b_Linq() =>
            _values
            .OrderBy(_key)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public bool B45c_Linq() =>
            _values
            .OrderBy(_key)
            .Any();

        [Benchmark]
        public bool B46_Linq() =>
            _values
            .OrderBy(_key)
            .Contains(_present);

        [Benchmark]
        public int[] B47a_Linq() =>
            _single
            .OrderBy(_key)
            .ToArray();

        [Benchmark]
        public List<int> B47b_Linq() =>
            _single
            .OrderBy(_key)
            .ToList();

        [Benchmark]
        public string[] B48a_Linq() =>
            _strings
            .OrderBy(_stringKey)
            .ToArray();

        [Benchmark]
        public int[] B48b_Linq() =>
            _values
            .OrderBy(_key)
            .ToArray();
    }
}
