using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Over an array or list, a Where's last element is found by scanning backwards from the end, the predicate run on the
    // tail alone and in reverse order, as System.Linq's is; an element from the start by a forward scan counting
    // matches. A Select above projects only the element found, and over a source that cannot answer (an iterator) the
    // source is run to the element with the selector not run on the elements passed over.
    public class WhereElements
    {
        private static readonly int[] Data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        private static IEnumerable<int> Iterate(int[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        [Fact]
        public void Last_ScansBackwardsFromTheEnd()
        {
            foreach (var shape in new[] { "array", "list" })
            {
                var seen = new List<int>();
                Func<int, bool> isEven = x => { seen.Add(x); return x % 2 == 0 && x < 7; };

                var last = shape == "array" ? Data.Where(isEven).Last() : new List<int>(Data).Where(isEven).Last();

                Assert.Equal(6, last);
                Assert.Equal(new[] { 10, 9, 8, 7, 6 }, seen);
            }
        }

        [Fact]
        public void Last_NoMatch()
        {
            Func<int, bool> never = _ => false;

            Assert.Throws<InvalidOperationException>(() => Data.Where(never).Last());
            Assert.Equal(0, Data.Where(never).LastOrDefault());
            Assert.Equal(-1, Data.Where(never).LastOrDefault(-1));
        }

        [Fact]
        public void ElementAt_CountsMatchesFromTheStart()
        {
            var seen = new List<int>();
            Func<int, bool> isEven = x => { seen.Add(x); return x % 2 == 0; };

            Assert.Equal(6, Data.Where(isEven).ElementAt(2));
            Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, seen);
            Assert.Equal(0, Data.Where(isEven).ElementAtOrDefault(5));
            Assert.Equal(8, Data.Where(isEven).ElementAt(^2));
            Assert.Equal(2, Data.Where(isEven).First());
        }

        [Fact]
        public void WhereSelect_ProjectsOnlyTheElementFound()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };
            Func<int, bool> isEven = x => x % 2 == 0;

            Assert.Equal(100, Data.Where(isEven).Select(selector).Last());
            Assert.Equal(60, Data.Where(isEven).Select(selector).ElementAt(2));
            Assert.Equal(100, Iterate(Data).Where(isEven).Select(selector).Last());
            Assert.Equal(60, Iterate(Data).Where(isEven).Select(selector).ElementAt(2));
            Assert.Equal(4, calls);
        }

        [Fact]
        public void SelectOverAnIterator_ProjectsOnlyTheElementFound()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(100, Iterate(Data).Select(selector).Last());
            Assert.Equal(40, Iterate(Data).Select(selector).ElementAt(3));
            Assert.Equal(0, Iterate(Data).Select(selector).ElementAtOrDefault(10));
            Assert.Equal(0, Iterate([]).Select(selector).LastOrDefault());
            Assert.Equal(100, Iterate(Data).Reverse().Select(selector).First());
            Assert.Equal(3, calls);
        }

        [Fact]
        public void SelectOverAnIterator_InAConcat_RunsTheSourceOnce()
        {
            var enumerations = 0;
            IEnumerable<int> source()
            {
                ++enumerations;
                foreach (var value in Data)
                    yield return value;
            }
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(-2, source().Select(selector).Concat(new[] { -1, -2 }).ElementAt(11));
            // Last is answered by the array alone, as System.Linq's Concat answers it, so the source is not run for it
            Assert.Equal(-2, source().Select(selector).Concat(new[] { -1, -2 }).Last());
            Assert.Equal(30, source().Select(selector).Concat(new[] { -1, -2 }).ElementAt(2));
            Assert.Equal(2, enumerations);
        }

        [Fact]
        public void WhereSelect_ToArray_FiltersThenProjects()
        {
            var order = new List<string>();
            Func<int, bool> isEven = x => { order.Add($"p{x}"); return x % 2 == 0; };
            Func<int, int> selector = x => { order.Add($"f{x}"); return x * 10; };

            var result = Data.Where(isEven).Select(selector).ToArray();

            Assert.Equal(new[] { 20, 40, 60, 80, 100 }, result);
            Assert.Equal(new[] { "p1", "p2", "p3", "p4", "p5", "p6", "p7", "p8", "p9", "p10", "f2", "f4", "f6", "f8", "f10" }, order);
            Assert.Empty(Data.Where(_ => false).Select(selector).ToArray());
            Assert.Equal(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(System.Linq.Enumerable.Range(0, 1000), x => x * 3)), System.Linq.Enumerable.Range(0, 1000).ToArray().Where(_ => true).Select(x => x * 3).ToArray());
        }
    }
}
