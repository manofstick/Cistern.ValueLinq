using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class A
    {
        [Benchmark]
        public int A01_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Select(_selector)
            .Select(_selector2)
            .Sum();

        [Benchmark]
        public int A02_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Where(_predicate)
            .Where(_predicate2)
            .Sum();

        [Benchmark]
        public int A03_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .Sum();

        [Benchmark]
        public int A04_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .Select(_selector2)
            .Sum();

        [Benchmark]
        public int A05_ZLinq() =>
            _objects
            .AsValueEnumerable()
            .OfType<string>()
            .Select(_stringLength)
            .Sum();

        [Benchmark]
        public int A06_ZLinq() =>
            _array
            .AsValueEnumerable()
            .OrderBy(_key)
            .Select(_selector)
            .First();

        [Benchmark]
        public int A07_ZLinq() =>
            ValueEnumerable
            .Range(0, Length)
            .Select(_selector)
            .Sum();

        [Benchmark]
        public int A08_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Select(_selector)
            .Skip(Length / 2)
            .Sum();

        [Benchmark]
        public int A09_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Skip(Length / 4)
            .Take(Length / 2)
            .Select(_selector)
            .Sum();

        [Benchmark]
        public int A10_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Skip(Length - 10)
            .Sum();

        [Benchmark]
        public int A11_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Take(Length / 2)
            .Sum();

        [Benchmark]
        public int A12_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Skip(100)
            .Take(Length / 2)
            .Skip(100)
            .Take(Length / 4)
            .Sum();

        [Benchmark]
        public int A13_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Where(_predicate)
            .Skip(100)
            .Take(Length / 4)
            .Sum();

        [Benchmark]
        public int[] A14_ZLinq() =>
            _array
            .AsValueEnumerable()
            .OrderBy(_key)
            .Take(100..110)
            .ToArray();

        [Benchmark]
        public int A15_ZLinq() =>
            ValueEnumerable
            .Range(0, Length)
            .Select(_selector)
            .Skip(Length - 10)
            .Sum();

        [Benchmark]
        public int[] A16_ZLinq() =>
            _array
            .AsValueEnumerable()
            .OrderBy(_key)
            .Skip(100)
            .Take(10)
            .ToArray();

        [Benchmark]
        public int[] A17_ZLinq() =>
            _array
            .AsValueEnumerable()
            .OrderBy(_key)
            .ThenBy(_key2)
            .ToArray();

        [Benchmark]
        public int[] A18_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Shuffle()
            .Take(10)
            .ToArray();

        [Benchmark]
        public int A19_ZLinq() =>
            _parts[0]
            .AsValueEnumerable()
            .Concat(_parts[1])
            .Concat(_parts[2])
            .Concat(_parts[3])
            .Concat(_parts[4])
            .Concat(_parts[5])
            .Concat(_parts[6])
            .Concat(_parts[7])
            .Sum();

        [Benchmark]
        public int A20_ZLinq() =>
            _array
            .AsValueEnumerable()
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
        public int A21_ZLinq() =>
            _parts[0]
            .AsValueEnumerable()
            .Union(_parts[1])
            .Union(_parts[2])
            .Count();

        [Benchmark]
        public int A22_ZLinq() =>
            ValueEnumerable
            .Sequence(0, Length - 1, 1)
            .Skip(Length - 10)
            .Sum();

        [Benchmark]
        public int[] A23_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Order()
            .ToArray();

        [Benchmark]
        public int A24_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Cast<int>()
            .Sum();

        [Benchmark]
        public int[] A25_ZLinq() =>
            _empty
            .AsValueEnumerable()
            .Select(_selector)
            .Where(_predicate)
            .ToArray();
    }
}
