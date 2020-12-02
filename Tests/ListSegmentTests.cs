using System;
using System.Collections.Generic;
using Xunit;

#if CISTERN_LINQ
using Cistern.Linq;
#elif CISTERN_VALUELINQ
using Cistern.ValueLinq;
#else
using System.Linq;
#endif

namespace Linqs.Tests
{
    // ListSgement is an internal to Cistern.ValueLinq representation of a List that is used for forward iteration

    public class ListSegmentTests : EnumerableTests
    {
        private static IEnumerable<int> Numbers0to99()
        {
            for (var i = 0; i < 100; ++i)
                yield return i;
        }

        [Fact]
        public void SkipToArray()
        {
            var asEnumerable = Numbers0to99();
            var asList = new List<int>(Numbers0to99());

            var expected =
                asEnumerable
                .Skip(10)
                .ToArray();

            var actual =
                asList
                .Skip(10)
                .ToArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SkipTakeToArray()
        {
            var asEnumerable = Numbers0to99();
            var asList = new List<int>(Numbers0to99());

            var expected =
                asEnumerable
                .Skip(10)
                .Take(50)
                .ToArray();

            var actual =
                asList
                .Skip(10)
                .Take(50)
                .ToArray();

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void SkipTakeReverseToArray()
        {
            var asEnumerable = Numbers0to99();
            var asList = new List<int>(Numbers0to99());

            var expected =
                asEnumerable
                .Skip(10)
                .Take(50)
                .Reverse()
                .ToArray();

            var actual =
                asList
                .Skip(10)
                .Take(50)
                .Reverse()
                .ToArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SkipTakeReverseSkipToArray()
        {
            var asEnumerable = Numbers0to99();
            var asList = new List<int>(Numbers0to99());

            var expected =
                asEnumerable
                .Skip(10)
                .Take(50)
                .Reverse()
                .Skip(10)
                .ToArray();

            var actual =
                asList
                .Skip(10)
                .Take(50)
                .Reverse()
                .Skip(10)
                .ToArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReverseSkipToArray()
        {
            var asEnumerable = Numbers0to99();
            var asList = new List<int>(Numbers0to99());

            var expected =
                asEnumerable
                .Reverse()
                .Skip(10)
                .ToArray();

            var actual =
                ((IEnumerable<int>)asList)
                .Reverse()
                .Skip(10)
                .ToArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SkipReverseSkipToArray()
        {
            var asEnumerable = Numbers0to99();
            var asList = new List<int>(Numbers0to99());

            var expected =
                asEnumerable
                .Skip(10)
                .Reverse()
                .Skip(10)
                .ToArray();

            var actual =
                asList
                .Skip(10)
                .Reverse()
                .Skip(10)
                .ToArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SkipTakeReverseSkipTakeToArray()
        {
            var asEnumerable = Numbers0to99();
            var asList       = new List<int>(Numbers0to99());

            var expected =
                asEnumerable
                .Skip(10)
                .Take(50)
                .Reverse()
                .Skip(10)
                .Take(25)
                .ToArray();

            var actual =
                asList
                .Skip(10)
                .Take(50)
                .Reverse()
                .Skip(10)
                .Take(25)
                .ToArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SkipTakeReverseSkipTakeReverseToArray()
        {
            var asEnumerable = Numbers0to99();
            var asList = new List<int>(Numbers0to99());

            var expected =
                asEnumerable
                .Skip(10)
                .Take(50)
                .Reverse()
                .Skip(10)
                .Take(25)
                .Reverse()
                .ToArray();

            var actual =
                asList
                .Skip(10)
                .Take(50)
                .Reverse()
                .Skip(10)
                .Take(25)
                .Reverse()
                .ToArray();

            Assert.Equal(expected, actual);
        }
    }
}
