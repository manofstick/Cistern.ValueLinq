using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Cistern.Benchmarks.Double
{
    [MemoryDiagnoser]
    public partial class Any
    {
        IEnumerable<double>? _double;
        Func<double, bool>? _filter;

        [Params(0, 1, 10, 100)]
        public int Length { get; set; } = 0;

        [Params(ContainerTypes.Array, ContainerTypes.Enumerable, ContainerTypes.List)]
        public ContainerTypes ContainerType { get; set; } = ContainerTypes.Enumerable;

        [Params(FilterTypes.None, FilterTypes.FirstHalf, FilterTypes.LastHalf, FilterTypes.Interleaved, FilterTypes.All)]
        public FilterTypes FilterType { get; set; } = FilterTypes.None;

        [GlobalSetup]
        public void SetupData()
        {
            var data = Create(Length);

            _double = ContainerType switch
            {
                ContainerTypes.Enumerable => data,
                ContainerTypes.Array      => data.ToArray(),
                ContainerTypes.List       => data.ToList(),

                _ => throw new Exception("Unknown ContainerType")
            };

            _filter = Filter(FilterType, Length);
        }

        private static IEnumerable<double> Create(int size)
        {
            for (var i = 0; i < size; ++i)
                yield return (double)i;
        }

        private static Func<double, bool>? Filter(FilterTypes filterType, int size) =>
            filterType switch
            {
                FilterTypes.All         => null,
                FilterTypes.Interleaved => x => ((int)x & 1) == 1,
                FilterTypes.FirstHalf   => x => x < size / 2,
                FilterTypes.LastHalf    => x => x >= size / 2,
                FilterTypes.None        => _ => false,

                _ => throw new ArgumentOutOfRangeException(nameof(filterType)),
            };

        internal static void SanityCheck()
        {
            var check = new Any();

            check.ContainerType = ContainerTypes.List;
            check.Length = 100;
            check.SetupData();

            var baseline = check.Linq();

#if LINQAF
            var linqaf = check.LinqAF();
            if (baseline != linqaf) throw new Exception();
#endif

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

#if CISTERNLINQ
            var cisternlinq = check.CisternLinq();
            if (cisternlinq != baseline) throw new Exception();
#endif
        }
    }
}
