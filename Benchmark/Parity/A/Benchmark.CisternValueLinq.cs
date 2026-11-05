using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class A
    {
        [Benchmark]
        public int A01_CisternValueLinq() =>
            _array
            .Select(_selector)
            .Select(_selector2)
            .Sum();

        [Benchmark]
        public int A02_CisternValueLinq() =>
            _array
            .Where(_predicate)
            .Where(_predicate2)
            .Sum();

        [Benchmark]
        public int A03_CisternValueLinq() =>
            _array
            .Where(_predicate)
            .Select(_selector)
            .Sum();

        [Benchmark]
        public int A04_CisternValueLinq() =>
            _array
            .Where(_predicate)
            .Select(_selector)
            .Select(_selector2)
            .Sum();

        [Benchmark]
        public int A05_CisternValueLinq() =>
            _objects
            .OfType<string>()
            .Select(_stringLength)
            .Sum();

        [Benchmark]
        public int A06_CisternValueLinq() =>
            _array
            .OrderBy(_key)
            .Select(_selector)
            .First();

        [Benchmark]
        public int A07_CisternValueLinq() =>
            Enumerable
            .Range(0, Length)
            .Select(_selector)
            .Sum();

        [Benchmark]
        public int A08_CisternValueLinq() =>
            _array
            .Select(_selector)
            .Skip(Length / 2)
            .Sum();

        [Benchmark]
        public int A09_CisternValueLinq() =>
            _list
            .Skip(Length / 4)
            .Take(Length / 2)
            .Select(_selector)
            .Sum();

        [Benchmark]
        public int A10_CisternValueLinq() =>
            _array
            .Skip(Length - 10)
            .Sum();

        [Benchmark]
        public int A11_CisternValueLinq() =>
            _list
            .Take(Length / 2)
            .Sum();

        [Benchmark]
        public int A12_CisternValueLinq() =>
            _list
            .Skip(100)
            .Take(Length / 2)
            .Skip(100)
            .Take(Length / 4)
            .Sum();

        [Benchmark]
        public int A13_CisternValueLinq() =>
            _array
            .Where(_predicate)
            .Skip(100)
            .Take(Length / 4)
            .Sum();

        [Benchmark]
        public int[] A14_CisternValueLinq() =>
            _array
            .OrderBy(_key)
            .Take(100..110)
            .ToArray();

        [Benchmark]
        public int A15_CisternValueLinq() =>
            Enumerable
            .Range(0, Length)
            .Select(_selector)
            .Skip(Length - 10)
            .Sum();

        [Benchmark]
        public int[] A16_CisternValueLinq() =>
            _array
            .OrderBy(_key)
            .Skip(100)
            .Take(10)
            .ToArray();

        [Benchmark]
        public int[] A17_CisternValueLinq() =>
            _array
            .OrderBy(_key)
            .ThenBy(_key2)
            .ToArray();

        [Benchmark]
        public int[] A18_CisternValueLinq() =>
            _iterator
            .Shuffle()
            .Take(10)
            .ToArray();

        [Benchmark]
        public int A19_CisternValueLinq() =>
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
        public int A20_CisternValueLinq() =>
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
        public int A21_CisternValueLinq() =>
            _parts[0]
            .Union(_parts[1])
            .Union(_parts[2])
            .Count();

        [Benchmark]
        public int A22_CisternValueLinq() =>
            Enumerable
            .Sequence(0, Length - 1, 1)
            .Skip(Length - 10)
            .Sum();

        [Benchmark]
        public int[] A23_CisternValueLinq() =>
            _array
            .Order()
            .ToArray();

        [Benchmark]
        public int A24_CisternValueLinq() =>
            _list
            .Cast<int>()
            .Sum();

        [Benchmark]
        public int[] A25_CisternValueLinq() =>
            _empty
            .Select(_selector)
            .Where(_predicate)
            .ToArray();
    }
}
