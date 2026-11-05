using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    // foreach over a chain: a chain built on a typed source is pulled through its own enumerator by value; one built on
    // an IEnumerable<T>-typed source reaches its source through the erased boundary; one handed on as IEnumerable<T> is
    // pulled through the enumerator its GetEnumerator boxes
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public partial class ForeachChains
    {
        int[] _array = [];
        List<int> _list = [];
        IEnumerable<int> _enumerable = [];

        [Params(10, 1000)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            _array = new int[Length];
            for (var i = 0; i < Length; ++i)
                _array[i] = (i * 31) % 1000;
            _list = new List<int>(_array);
            _enumerable = _array;
        }

        internal static void SanityCheck()
        {
            var check = new ForeachChains();

            foreach (var length in new[] { 0, 1, 10, 1000 })
            {
                check.Length = length;
                check.SetupData();

                if (check.Handcoded_ArraySelect() != check.CisternValueLinq_ArraySelect()) throw new Exception();
                if (check.Handcoded_ArrayWhereSelect() != check.CisternValueLinq_ArrayWhereSelect()) throw new Exception();
                if (check.Handcoded_ListWhereSelect() != check.CisternValueLinq_ListWhereSelect()) throw new Exception();
                if (check.Handcoded_ArraySkipTake() != check.CisternValueLinq_ArraySkipTake()) throw new Exception();
                if (check.Handcoded_EnumerableWhereSelect() != check.CisternValueLinq_EnumerableWhereSelect()) throw new Exception();
                if (check.Handcoded_EnumerableWhereSelectAsEnumerable() != check.CisternValueLinq_EnumerableWhereSelectAsEnumerable()) throw new Exception();
            }
        }
    }
}
