using BenchmarkDotNet.Attributes;
using System;

namespace Cistern.Benchmarks.Synthetic
{
    public readonly struct Trade
    {
        public Trade(int symbol, double price, int qty) => (Symbol, Price, Qty) = (symbol, price, qty);

        public int Symbol { get; }
        public double Price { get; }
        public int Qty { get; }
    }

    [MemoryDiagnoser]
    public partial class Pipeline
    {
        Trade[] _trades;
        int _skip;
        int _take;

        [Params(1, 100, 1000000)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            _trades = Create(Length);
            _skip = Length / 10;
            _take = Length / 2;
        }

        private static Trade[] Create(int size)
        {
            var trades = new Trade[size];
            for (var i = 0; i < size; ++i)
                trades[i] = new Trade(i % 50, 10.0 + ((i * 7919) % 1000) / 10.0, (i * 31) % 200);
            return trades;
        }

        internal static void SanityCheck()
        {
            var check = new Pipeline();

            check.Length = 1000;
            check.SetupData();

            var baseline = check.Handcoded();

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

            var linq = check.Linq();
            if (baseline != linq) throw new Exception();

            var zlinq = check.ZLinq();
            if (baseline != zlinq) throw new Exception();
        }
    }
}
