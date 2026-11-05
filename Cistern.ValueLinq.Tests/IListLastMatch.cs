using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Last and LastOrDefault with a predicate over an IList<T> that is neither an array nor a List<T> scan it from the
    // end, as System.Linq does: the predicate runs on the elements after the last match and on it, and on no other.
    public class IListLastMatch
    {
        private static Collection<int> Values(int count)
        {
            var values = new Collection<int>();
            for (var i = 0; i < count; ++i)
                values.Add(i);
            return values;
        }

        [Fact]
        public void LastRunsThePredicateBackToTheMatch()
        {
            IEnumerable<int> values = Values(100);
            var called = new List<int>();

            var last = values.Last(x => { called.Add(x); return x % 10 == 3; });

            Assert.Equal(93, last);
            Assert.Equal(new[] { 99, 98, 97, 96, 95, 94, 93 }, called);
        }

        [Fact]
        public void LastOrDefault()
        {
            IEnumerable<int> values = Values(100);
            var calls = 0;

            Assert.Equal(93, values.LastOrDefault(x => { ++calls; return x % 10 == 3; }));
            Assert.Equal(7, calls);
            Assert.Equal(-1, values.LastOrDefault(x => x > 100, -1));
        }

        [Fact]
        public void NoMatch()
        {
            IEnumerable<int> values = Values(100);

            Assert.Throws<InvalidOperationException>(() => values.Last(x => x > 100));
            Assert.Throws<InvalidOperationException>(() => ((IEnumerable<int>)Values(0)).Last(x => true));
        }

        [Fact]
        public void Window()
        {
            IEnumerable<int> values = Values(100);

            Assert.Equal(43, values.Skip(10).Take(40).Last(x => x % 10 == 3));
            Assert.Equal(-1, values.Skip(10).Take(3).LastOrDefault(x => x % 10 == 3, -1));
        }
    }
}
