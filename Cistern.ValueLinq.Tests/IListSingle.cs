using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Single and SingleOrDefault over an IList<T> are decided from its count with at most one indexer read, as
    // System.Linq decides them: the list is never enumerated.
    public class IListSingle
    {
        private sealed class NotEnumerable
            : Collection<int>
            , IEnumerable<int>
        {
            public NotEnumerable(params int[] values)
                : base(new List<int>(values))
            {
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => throw new InvalidOperationException("enumerated");
        }

        [Fact]
        public void One()
        {
            IEnumerable<int> values = new NotEnumerable(42);

            Assert.Equal(42, values.Single());
            Assert.Equal(42, values.SingleOrDefault());
            Assert.Equal(42, values.SingleOrDefault(-1));
        }

        [Fact]
        public void None()
        {
            IEnumerable<int> values = new NotEnumerable();

            var thrown = Assert.Throws<InvalidOperationException>(() => values.Single());
            Assert.NotEqual("enumerated", thrown.Message);
            Assert.Equal(0, values.SingleOrDefault());
            Assert.Equal(-1, values.SingleOrDefault(-1));
        }

        [Fact]
        public void MoreThanOne()
        {
            IEnumerable<int> values = new NotEnumerable(1, 2);

            Assert.NotEqual("enumerated", Assert.Throws<InvalidOperationException>(() => values.Single()).Message);
            Assert.NotEqual("enumerated", Assert.Throws<InvalidOperationException>(() => values.SingleOrDefault()).Message);
            Assert.NotEqual("enumerated", Assert.Throws<InvalidOperationException>(() => values.SingleOrDefault(-1)).Message);
        }

        [Fact]
        public void NotAList()
        {
            static IEnumerable<int> iterate(params int[] values)
            {
                foreach (var value in values)
                    yield return value;
            }

            Assert.Equal(42, iterate(42).Single());
            Assert.Throws<InvalidOperationException>(() => iterate().Single());
            Assert.Throws<InvalidOperationException>(() => iterate(1, 2).SingleOrDefault());
            Assert.Throws<ArgumentNullException>(() => ((IEnumerable<int>)null!).Single());
        }
    }
}
