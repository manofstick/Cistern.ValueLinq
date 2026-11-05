using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class A
    {
        [Benchmark]
        public int A01_Linq() =>
            _array
            .Select(_selector)
            .Select(_selector2)
            .Sum();

        [Benchmark]
        public int A02_Linq() =>
            _array
            .Where(_predicate)
            .Where(_predicate2)
            .Sum();

        [Benchmark]
        public int A03_Linq() =>
            _array
            .Where(_predicate)
            .Select(_selector)
            .Sum();

        [Benchmark]
        public int A04_Linq() =>
            _array
            .Where(_predicate)
            .Select(_selector)
            .Select(_selector2)
            .Sum();

        [Benchmark]
        public int A05_Linq() =>
            _objects
            .OfType<string>()
            .Select(_stringLength)
            .Sum();

        [Benchmark]
        public int A06_Linq() =>
            _array
            .OrderBy(_key)
            .Select(_selector)
            .First();

        [Benchmark]
        public int A07_Linq() =>
            Enumerable
            .Range(0, Length)
            .Select(_selector)
            .Sum();

        [Benchmark]
        public int A08_Linq() =>
            _array
            .Select(_selector)
            .Skip(Length / 2)
            .Sum();

        [Benchmark]
        public int A09_Linq() =>
            _list
            .Skip(Length / 4)
            .Take(Length / 2)
            .Select(_selector)
            .Sum();

        [Benchmark]
        public int A10_Linq() =>
            _array
            .Skip(Length - 10)
            .Sum();

        [Benchmark]
        public int A11_Linq() =>
            _list
            .Take(Length / 2)
            .Sum();

        [Benchmark]
        public int A12_Linq() =>
            _list
            .Skip(100)
            .Take(Length / 2)
            .Skip(100)
            .Take(Length / 4)
            .Sum();

        [Benchmark]
        public int A13_Linq() =>
            _array
            .Where(_predicate)
            .Skip(100)
            .Take(Length / 4)
            .Sum();

        [Benchmark]
        public int[] A14_Linq() =>
            _array
            .OrderBy(_key)
            .Take(100..110)
            .ToArray();

        [Benchmark]
        public int A15_Linq() =>
            Enumerable
            .Range(0, Length)
            .Select(_selector)
            .Skip(Length - 10)
            .Sum();

        [Benchmark]
        public int[] A16_Linq() =>
            _array
            .OrderBy(_key)
            .Skip(100)
            .Take(10)
            .ToArray();

        [Benchmark]
        public int[] A17_Linq() =>
            _array
            .OrderBy(_key)
            .ThenBy(_key2)
            .ToArray();

        [Benchmark]
        public int[] A18_Linq() =>
            _iterator
            .Shuffle()
            .Take(10)
            .ToArray();

        [Benchmark]
        public int A19_Linq() =>
            _parts[0]
            .Concat(_parts[1])
            .Concat(_parts[2])
            .Concat(_parts[3])
            .Concat(_parts[4])
            .Concat(_parts[5])
            .Concat(_parts[6])
            .Concat(_parts[7])
            .Sum();

        [Benchmark]
        public int A20_Linq() =>
            _array
            .Append(1)
            .Append(2)
            .Append(3)
            .Append(4)
            .Prepend(5)
            .Prepend(6)
            .Prepend(7)
            .Prepend(8)
            .Sum();

        [Benchmark]
        public int A21_Linq() =>
            _parts[0]
            .Union(_parts[1])
            .Union(_parts[2])
            .Count();

        [Benchmark]
        public int A22_Linq() =>
            Enumerable
            .Sequence(0, Length - 1, 1)
            .Skip(Length - 10)
            .Sum();

        [Benchmark]
        public int[] A23_Linq() =>
            _array
            .Order()
            .ToArray();

        [Benchmark]
        public int A24_Linq() =>
            _list
            .Cast<int>()
            .Sum();

        [Benchmark]
        public int[] A25_Linq() =>
            _empty
            .Select(_selector)
            .Where(_predicate)
            .ToArray();
    }
}
