// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
    public class ReverseTests : EnumerableTests
    {
        public void InvalidArguments()
        {
            IEnumerable<string> x = null;
            AssertExtensions.Throws<ArgumentNullException>("source", () => Enumerable.Reverse<string>(x));
        }

        [Theory]
        [MemberData(nameof(ReverseData))]
        public void Reverse<T>(IEnumerable<T> source)
        {
            T[] expected = source.ToArray();
            Array.Reverse(expected);

            IEnumerable<T> actual = source.Reverse();

            Assert.Equal(expected, actual);
            Assert.Equal(expected.Count(), actual.Count()); // Count may be optimized.
            Assert.Equal(expected, actual.ToArray());
            Assert.Equal(expected, actual.ToList());

            Assert.Equal(expected.FirstOrDefault(), actual.FirstOrDefault());
            Assert.Equal(expected.LastOrDefault(), actual.LastOrDefault());

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual.ElementAt(i));

                Assert.Equal(expected.Skip(i), actual.Skip(i));
                Assert.Equal(expected.Take(i), actual.Take(i));
            }

            Assert.Equal(default(T), actual.ElementAtOrDefault(-1));
            Assert.Equal(default(T), actual.ElementAtOrDefault(expected.Length));

            Assert.Equal(expected, actual.Select(_ => _));
            Assert.Equal(expected, actual.Where(_ => true));

            Assert.Equal(actual, actual); // Repeat the enumeration against itself.
        }

        [Theory, MemberData(nameof(ReverseData))]
        public void RunOnce<T>(IEnumerable<T> source)
        {
            T[] expected = source.ToArray();
            Array.Reverse(expected);

            IEnumerable<T> actual = source.RunOnce().Reverse();

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> ReverseData()
        {
            var integers = new[]
            {
                Array.Empty<int>(), // No elements.
                new[] { 1 }, // One element.
                new[] { 9999, 0, 888, -1, 66, -777, 1, 2, -12345 }, // Distinct elements.
                new[] { -10, 0, 5, 0, 9, 100, 9 }, // Some repeating elements.
            };

            return integers
                .Select(collection => new object[] { collection })
                .Concat(
                    integers.Select(c => new object[] { c.Select(i => i.ToString()) })
                );
        }

        //[Fact(Skip ="CISTERN.VALUELINQ Invalid")]
        //public void ForcedToEnumeratorDoesntEnumerate()
        //{
        //    var iterator = NumberRangeGuaranteedNotCollectionType(0, 3).Reverse();
        //    // Don't insist on this behaviour, but check it's correct if it happens
        //    var en = iterator as IEnumerator<int>;
        //    Assert.False(en != null && en.MoveNext());
        //}

        [Fact]
        public void FastArrayReverse()
        {
            var abc = new[] { 'a', 'b', 'c', 'd' };
            var cba = abc.Reverse().ToArray();
            Assert.Equal(4, cba.Length);
            Assert.Equal('d', cba[0]);
            Assert.Equal('c', cba[1]);
            Assert.Equal('b', cba[2]);
            Assert.Equal('a', cba[3]);

            var current = abc.Skip(1).Reverse().Skip(2).ToArray();
            var baseline =
                System.Linq.Enumerable.ToArray(
                    System.Linq.Enumerable.Skip(
                        System.Linq.Enumerable.Reverse(
                            System.Linq.Enumerable.Skip(
                                abc, 1
                            )
                        ), 2
                    )
                );
            Assert.Equal(baseline, current);
        }

        private static IEnumerable<int> Numbers0to99()
        {
            for (var i = 0; i < 100; ++i)
                yield return i;
        }

        public static IEnumerable<object[]> MultipleTypesOfNumbers0to99()
        {
            var data = new object[]
            {
                Numbers0to99(),
                Numbers0to99().ToList(),
                Numbers0to99().ToArray(),
                Enumerable.Range(0, 100),
            };

            return data.Select(item => new[] { item });
        }


        [Theory]
        [MemberData(nameof(MultipleTypesOfNumbers0to99))]
        public void SelectReverseSum(IEnumerable<int> data)
        {
            var asEnumerable = Numbers0to99();

            var expected =
                asEnumerable.Sum() * 2;

            var actual =
                data
                .Select(x => x * 2)
                .Reverse()
                .Sum();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(MultipleTypesOfNumbers0to99))]
        public void SelectReverseSkipSum(IEnumerable<int> data)
        {
            var asEnumerable = Numbers0to99();

            var toSkip = 10;

            var expected = 2 *
                asEnumerable
                .Take(data.Count()-toSkip)
                .Sum();

            var actual =
                data
                .Select(x => x * 2)
                .Reverse()
                .Skip(toSkip)
                .Sum();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(MultipleTypesOfNumbers0to99))]
        public void ReverseSelectSkipSum(IEnumerable<int> data)
        {
            var asEnumerable = Numbers0to99();

            var toSkip = 10;

            var expected = 2 *
                asEnumerable
                .Take(data.Count() - toSkip)
                .Sum();

            var actual =
                data
                .Reverse()
                .Select(x => x * 2)
                .Skip(toSkip)
                .Sum();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(MultipleTypesOfNumbers0to99))]
        public void ReverseSkipSelectSum(IEnumerable<int> data)
        {
            var asEnumerable = Numbers0to99();

            var toSkip = 10;

            var expected = 2 *
                asEnumerable
                .Take(data.Count() - toSkip)
                .Sum();

            var actual =
                data
                .Reverse()
                .Skip(toSkip)
                .Select(x => x * 2)
                .Sum();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(MultipleTypesOfNumbers0to99))]
        public void SkipSelectReverseSum(IEnumerable<int> data)
        {
            var asEnumerable = Numbers0to99();

            var toSkip = 10;

            var expected = 2 *
                asEnumerable
                .Skip(toSkip)
                .Sum();

            var actual =
                data
                .Skip(toSkip)
                .Select(x => x * 2)
                .Reverse()
                .Sum();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(MultipleTypesOfNumbers0to99))]
        public void SelectSkipReverseSum(IEnumerable<int> data)
        {
            var asEnumerable = Numbers0to99();

            var toSkip = 10;

            var expected = 2 *
                asEnumerable
                .Skip(toSkip)
                .Sum();

            var actual =
                data
                .Select(x => x * 2)
                .Skip(toSkip)
                .Reverse()
                .Sum();

            Assert.Equal(expected, actual);
        }
    }
}
