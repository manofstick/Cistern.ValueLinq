using BenchmarkDotNet.Attributes;
using Cistern.Benchmarks.Parity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    // The LINQ benchmarks of the .NET 10 performance post, at the post's sizes and data: Contains answered without the
    // operator before it, Sequence into a list, Shuffle followed by Take, LeftJoin beside the GroupJoin and SelectMany
    // composition it replaces, Skip and Take copied out, and Contains with the default comparer. One method per entry (a
    // letter per operator or form where an entry covers several) and library, named by the entry's ID. LeftJoin's
    // sequences are each library's own, held as IEnumerable<T> as the post holds them where the library has that form. Shuffle's results are random, so
    // the sanity check asks of them what holds whatever the order.
    [MemoryDiagnoser]
    public partial class Net10
    {
        IEnumerable<int> _source;
        List<short> _values;
        IEnumerable<int> _shuffleSource;
        IEnumerable<string> _strings;
        ReadOnlyCollection<int> _readOnly;
        IEnumerable<int> _data;

        public Net10()
        {
            _source = Enumerable.Range(0, 1000).ToArray();
            _values = new();
            _shuffleSource = Enumerable.Range(1, 1000).ToList();
            _strings = Enumerable.Range(0, 1000).Select(i => i.ToString()).ToArray();
            _readOnly = new(Enumerable.Range(1, 1000).ToArray());
            _data = Enumerable.Range(0, 1_000_000).ToArray();

            BuildLinq();
            BuildCisternValueLinq();
        }

        partial void BuildLinq();
        partial void BuildCisternValueLinq();

        // N05's sequences for the hand-coded join, which no library builds
        static IEnumerable<int> Stepped(int start, int endInclusive, int step)
        {
            for (var i = start; i <= endInclusive; i += step)
                yield return i;
        }

        static IEnumerable<int> ShuffleManual(IEnumerable<int> source)
        {
            int[] arr = source.ToArray();
            Random.Shared.Shuffle(arr);
            foreach (var item in arr)
                yield return item;
        }

        static IEnumerable<TResult> ManualLeftJoin<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner?, TResult> resultSelector) =>
            outer
            .GroupJoin(inner, outerKeySelector, innerKeySelector, (o, inners) => (o, inners))
            .SelectMany(x => x.inners.DefaultIfEmpty(), (x, i) => resultSelector(x.o, i));

        internal static void SanityCheck()
        {
            var check = new Net10();

            Check.Same(nameof(N01a_Linq), check.N01a_Linq() ? 1 : 0, check.N01a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N01b_Linq), check.N01b_Linq() ? 1 : 0, check.N01b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N01c_Linq), check.N01c_Linq() ? 1 : 0, check.N01c_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N01d_Linq), check.N01d_Linq() ? 1 : 0, check.N01d_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N01e_Linq), check.N01e_Linq() ? 1 : 0, check.N01e_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N01f_Linq), check.N01f_Linq() ? 1 : 0, check.N01f_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N01g_Linq), check.N01g_Linq() ? 1 : 0, check.N01g_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N01h_Linq), check.N01h_Linq() ? 1 : 0, check.N01h_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N01i_Linq), check.N01i_Linq() ? 1 : 0, check.N01i_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N02_Linq), check.N02_Linq().ToArray(), check.N02_CisternValueLinq());
            Check.Same(nameof(N02_CisternValueLinqCollection), check.N02_Linq().ToArray(), check.N02_CisternValueLinqCollection());
            check.PartOfSource(nameof(N03a_Linq), check.N03a_Linq());
            check.PartOfSource(nameof(N03a_CisternValueLinq), check.N03a_CisternValueLinq());
            check.PartOfSource(nameof(N03b_Linq), check.N03b_Linq());
            check.PartOfSource(nameof(N03b_CisternValueLinq), check.N03b_CisternValueLinq());
            Check.Same(nameof(N04a_Linq), check.N04a_Linq() ? 1 : 0, check.N04a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N04b_Linq), check.N04b_Linq() ? 1 : 0, check.N04b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N05a_Linq), check.N05a_Linq(), check.N05a_CisternValueLinq());
            Check.Same(nameof(N05b_Linq), check.N05b_Linq(), check.N05b_CisternValueLinq());
            Check.Same(nameof(N06_Linq), check.N06_Linq(), check.N06_CisternValueLinq());
            Check.Same(nameof(N07_Linq), check.N07_Linq(), check.N07_CisternValueLinq());
            Check.Same(nameof(N08_Linq), check.N08_Linq() ? 1 : 0, check.N08_CisternValueLinq() ? 1 : 0);

            Check.Same(nameof(N01a_CisternValueLinqTyped), check.N01a_Linq() ? 1 : 0, check.N01a_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N01b_CisternValueLinqTyped), check.N01b_Linq() ? 1 : 0, check.N01b_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N01c_CisternValueLinqTyped), check.N01c_Linq() ? 1 : 0, check.N01c_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N01d_CisternValueLinqTyped), check.N01d_Linq() ? 1 : 0, check.N01d_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N01e_CisternValueLinqTyped), check.N01e_Linq() ? 1 : 0, check.N01e_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N01f_CisternValueLinqTyped), check.N01f_Linq() ? 1 : 0, check.N01f_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N01g_CisternValueLinqTyped), check.N01g_Linq() ? 1 : 0, check.N01g_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N01h_CisternValueLinqTyped), check.N01h_Linq() ? 1 : 0, check.N01h_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N01i_CisternValueLinqTyped), check.N01i_Linq() ? 1 : 0, check.N01i_CisternValueLinqTyped() ? 1 : 0);
            check.PartOfSource(nameof(N03a_CisternValueLinqTyped), check.N03a_CisternValueLinqTyped());
            Check.Same(nameof(N04a_CisternValueLinqTyped), check.N04a_Linq() ? 1 : 0, check.N04a_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N05a_CisternValueLinqTyped), check.N05a_Linq(), check.N05a_CisternValueLinqTyped());
            Check.Same(nameof(N05b_CisternValueLinqTyped), check.N05b_Linq(), check.N05b_CisternValueLinqTyped());
            Check.Same(nameof(N06_CisternValueLinqTyped), check.N06_Linq(), check.N06_CisternValueLinqTyped());
            Check.Same(nameof(N08_CisternValueLinqTyped), check.N08_Linq() ? 1 : 0, check.N08_CisternValueLinqTyped() ? 1 : 0);

            Check.Same(nameof(N01a_ZLinq), check.N01a_Linq() ? 1 : 0, check.N01a_ZLinq() ? 1 : 0);
            Check.Same(nameof(N01b_ZLinq), check.N01b_Linq() ? 1 : 0, check.N01b_ZLinq() ? 1 : 0);
            Check.Same(nameof(N01c_ZLinq), check.N01c_Linq() ? 1 : 0, check.N01c_ZLinq() ? 1 : 0);
            Check.Same(nameof(N01d_ZLinq), check.N01d_Linq() ? 1 : 0, check.N01d_ZLinq() ? 1 : 0);
            Check.Same(nameof(N01e_ZLinq), check.N01e_Linq() ? 1 : 0, check.N01e_ZLinq() ? 1 : 0);
            Check.Same(nameof(N01f_ZLinq), check.N01f_Linq() ? 1 : 0, check.N01f_ZLinq() ? 1 : 0);
            Check.Same(nameof(N01g_ZLinq), check.N01g_Linq() ? 1 : 0, check.N01g_ZLinq() ? 1 : 0);
            Check.Same(nameof(N01h_ZLinq), check.N01h_Linq() ? 1 : 0, check.N01h_ZLinq() ? 1 : 0);
            Check.Same(nameof(N01i_ZLinq), check.N01i_Linq() ? 1 : 0, check.N01i_ZLinq() ? 1 : 0);
            Check.Same(nameof(N02_ZLinq), check.N02_Linq().ToArray(), check.N02_ZLinq());
            check.PartOfSource(nameof(N03a_ZLinq), check.N03a_ZLinq());
            Check.Same(nameof(N04a_ZLinq), check.N04a_Linq() ? 1 : 0, check.N04a_ZLinq() ? 1 : 0);
            Check.Same(nameof(N05a_ZLinq), check.N05a_Linq(), check.N05a_ZLinq());
            Check.Same(nameof(N06_ZLinq), check.N06_Linq(), check.N06_ZLinq());
            Check.Same(nameof(N07_ZLinq), check.N07_Linq(), check.N07_ZLinq());
            Check.Same(nameof(N08_ZLinq), check.N08_Linq() ? 1 : 0, check.N08_ZLinq() ? 1 : 0);

            Check.Same(nameof(N01a_Handcoded), check.N01a_Linq() ? 1 : 0, check.N01a_Handcoded() ? 1 : 0);
            Check.Same(nameof(N01b_Handcoded), check.N01b_Linq() ? 1 : 0, check.N01b_Handcoded() ? 1 : 0);
            Check.Same(nameof(N01c_Handcoded), check.N01c_Linq() ? 1 : 0, check.N01c_Handcoded() ? 1 : 0);
            Check.Same(nameof(N01d_Handcoded), check.N01d_Linq() ? 1 : 0, check.N01d_Handcoded() ? 1 : 0);
            Check.Same(nameof(N01e_Handcoded), check.N01e_Linq() ? 1 : 0, check.N01e_Handcoded() ? 1 : 0);
            Check.Same(nameof(N01f_Handcoded), check.N01f_Linq() ? 1 : 0, check.N01f_Handcoded() ? 1 : 0);
            Check.Same(nameof(N01g_Handcoded), check.N01g_Linq() ? 1 : 0, check.N01g_Handcoded() ? 1 : 0);
            Check.Same(nameof(N01h_Handcoded), check.N01h_Linq() ? 1 : 0, check.N01h_Handcoded() ? 1 : 0);
            Check.Same(nameof(N01i_Handcoded), check.N01i_Linq() ? 1 : 0, check.N01i_Handcoded() ? 1 : 0);
            Check.Same(nameof(N02_Handcoded), check.N02_Linq().ToArray(), check.N02_Handcoded());
            check.PartOfSource(nameof(N03a_Handcoded), check.N03a_Handcoded());
            Check.Same(nameof(N04a_Handcoded), check.N04a_Linq() ? 1 : 0, check.N04a_Handcoded() ? 1 : 0);
            Check.Same(nameof(N05a_Handcoded), check.N05a_Linq(), check.N05a_Handcoded());
            Check.Same(nameof(N06_Handcoded), check.N06_Linq(), check.N06_Handcoded());
            Check.Same(nameof(N07_Handcoded), check.N07_Linq(), check.N07_Handcoded());
            Check.Same(nameof(N08_Handcoded), check.N08_Linq() ? 1 : 0, check.N08_Handcoded() ? 1 : 0);
        }

        // ten distinct values of the source
        void PartOfSource(string entry, IEnumerable<int> part)
        {
            var taken = part.ToList();
            if (taken.Count != 10)
                throw new Exception($"{entry}: {taken.Count} elements, expected 10");
            if (taken.Distinct().Count() != taken.Count)
                throw new Exception($"{entry}: a value is taken twice");
            if (!taken.All(_shuffleSource.Contains))
                throw new Exception($"{entry}: a value is not in the source");
        }
    }
}
