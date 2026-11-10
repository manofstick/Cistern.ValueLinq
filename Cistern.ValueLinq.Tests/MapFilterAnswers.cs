using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Map and Filter take a pure function, so the requests Select and Where cannot answer without changing when their
    // user code runs are answered: Map's selector runs only for the elements a result is made of, and Filter's
    // predicate may run from the end.
    public class MapFilterAnswers
    {
        private static readonly int[] Values = [5, 3, 8, 1, 9, 4, 7];

        private static IEnumerable<int> Iterate(int[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        [Fact]
        public void Map_CountAndAny_RunNoSelector()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(7, Iterate(Values).Map(selector).Count());
            Assert.Equal(4, Values.Where(x => x > 4).Map(selector).Count());
            Assert.True(Values.Where(x => x > 4).Map(selector).Any());
            Assert.False(Values.Where(x => x > 100).Map(selector).Any());
            Assert.Equal(0, calls);
        }

        [Fact]
        public void Map_Skip_ProjectsOnlyWhatIsKept()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(new[] { 90, 40, 70 }, Values.Where(_ => true).Map(selector).Skip(4).ToArray());
            Assert.Equal(3, calls);

            calls = 0;
            Assert.Equal(110, Values.Where(_ => true).Map(selector).Skip(1).Take(2).Sum());
            Assert.Equal(2, calls);

            calls = 0;
            Assert.Equal(new[] { 80, 10 }, Values.Map(selector).Skip(2).Take(2).ToList());
            Assert.Equal(2, calls);
        }

        [Fact]
        public void Map_ElementFromEnd_ProjectsOnlyTheElement()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(90, Values.Where(_ => true).Map(selector).ElementAt(^3));
            Assert.Equal(90, Iterate(Values).Map(selector).ElementAt(^3));
            Assert.Equal(0, Iterate(Values).Map(selector).ElementAtOrDefault(^8));
            Assert.Equal(70, Iterate(Values).Map(selector).Last());
            Assert.Equal(3, calls);
        }

        [Fact]
        public void Map_Reverse_IsTheSourceReversed()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(70, Values.Map(selector).Reverse().First());
            Assert.Equal(1, calls);
            Assert.Equal(new[] { 70, 40, 90, 10, 80, 30, 50 }, Values.Map(selector).Reverse().ToArray());
        }

        [Fact]
        public void Map_Pulled()
        {
            var pulled = new List<int>();
            foreach (var x in Values.Map(x => x + 1).Skip(5))
                pulled.Add(x);

            Assert.Equal(new[] { 5, 8 }, pulled);
            Assert.Equal(new[] { 6, 4, 9, 2, 10, 5, 8 }, ((IEnumerable<int>)Values.Map(x => x + 1)).ToList());
        }

        [Fact]
        public void Filter_ElementFromEnd_ScansFromTheEnd()
        {
            var calls = 0;
            Func<int, bool> predicate = x => { ++calls; return x > 4; };

            // 5, 8, 9, 7 match: ^2 is 9, reached from the end over 7, 4, 9
            Assert.Equal(9, Values.Filter(predicate).ElementAt(^2));
            Assert.Equal(3, calls);

            calls = 0;
            Assert.Equal(9, Values.Map(x => x).Filter(predicate).ElementAt(^2));
            Assert.Equal(3, calls);

            Assert.Equal(0, Values.Filter(predicate).ElementAtOrDefault(^5));
            Assert.Equal(8, Values.Filter(predicate).ElementAt(1));
        }

        [Fact]
        public void Filter_Reverse_IsTheSourceReversed()
        {
            var calls = 0;
            Func<int, bool> predicate = x => { ++calls; return x > 4; };

            Assert.Equal(7, Values.Filter(predicate).Reverse().First());
            Assert.Equal(1, calls);

            calls = 0;
            Assert.Equal(9, Values.Filter(predicate).Last(x => x > 8));
            Assert.Equal(3, calls);

            Assert.Equal(new[] { 7, 9, 8, 5 }, Values.Filter(predicate).Reverse().ToArray());
        }

        [Fact]
        public void Filter_PulledAndPushed()
        {
            var pulled = new List<int>();
            foreach (var x in Iterate(Values).Filter(x => x % 2 == 1))
                pulled.Add(x);

            Assert.Equal(new[] { 5, 3, 1, 9, 7 }, pulled);
            Assert.Equal(new[] { 5, 3, 1, 9, 7 }, Values.Filter(x => x % 2 == 1).ToArray());
            Assert.Equal(new[] { 50, 30, 10, 90, 70 }, Values.Filter(x => x % 2 == 1).Map(x => x * 10).ToArray());
            Assert.Equal(25, Values.Filter(x => x % 2 == 1).Sum());
        }

        [Fact]
        public void Filter_OverAReversal_StopsAtTheFirstMatchFromTheEnd()
        {
            var calls = 0;
            Func<int, bool> predicate = x => { ++calls; return x == 9; };

            // 7, 4, then 9
            Assert.Equal(9, Iterate(Values).Reverse().Filter(predicate).First());
            Assert.Equal(3, calls);
        }
    }
}
