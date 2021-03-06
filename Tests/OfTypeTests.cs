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
    public class OfTypeTests : EnumerableTests
    {
        [Fact]
        public void SameResultsRepeatCallsIntQuery()
        {
            var q = from x in new[] { 9999, 0, 888, -1, 66, -777, 1, 2, -12345 }
                    where x > int.MinValue
                    select x;

            Assert.Equal(q.OfType<int>(), q.OfType<int>());
        }

        [Fact]
        public void SameResultsRepeatCallsStringQuery()
        {
            var q = from x in new[] { "!@#$%^", "C", "AAA", "", "Calling Twice", "SoS", string.Empty }
                    where string.IsNullOrEmpty(x)
                    select x;

            Assert.Equal(q.OfType<int>(), q.OfType<int>());
        }

        [Fact]
        public void EmptySource()
        {
            object[] source = { };
            Assert.Empty(source.OfType<int>());
        }

        [Fact]
        public void LongSequenceFromIntSource()
        {
            int[] source = { 99, 45, 81 };
            Assert.Empty(source.OfType<long>());

        }

        [Fact]
        public void HeterogenousSourceNoAppropriateElements()
        {
            object[] source = { "Hello", 3.5, "Test" };
            Assert.Empty(source.OfType<int>());
        }

        [Fact]
        public void HeterogenousSourceOnlyFirstOfType()
        {
            object[] source = { 10, "Hello", 3.5, "Test" };
            int[] expected = { 10 };

            Assert.Equal(expected, source.OfType<int>());
        }

        [Fact]
        public void AllElementsOfNullableTypeNullsSkipped()
        {
            object[] source = { 10, -4, null, null, 4, 9 };
            int?[] expected = { 10, -4, 4, 9 };

            Assert.Equal(expected, source.OfType<int?>());
        }

        [Fact]
        public void HeterogenousSourceSomeOfType()
        {
            object[] source = { 3.5m, -4, "Test", "Check", 4, 8.0, 10.5, 9 };
            int[] expected = { -4, 4, 9 };

            Assert.Equal(expected, source.OfType<int>());
        }

        [Fact]
        public void RunOnce()
        {
            object[] source = { 3.5m, -4, "Test", "Check", 4, 8.0, 10.5, 9 };
            int[] expected = { -4, 4, 9 };

            Assert.Equal(expected, source.RunOnce().OfType<int>());
        }

        [Fact]
        public void IntFromNullableInt()
        {
            int[] source = { -4, 4, 9 };
            int?[] expected = { -4, 4, 9 };

            Assert.Equal(expected, source.OfType<int?>());
        }

        [Fact]
        public void IntFromNullableIntWithNulls()
        {
            int?[] source = { null, -4, 4, null, 9 };
            int[] expected = { -4, 4, 9 };

            Assert.Equal(expected, source.OfType<int>());
        }

        [Fact]
        public void NullableDecimalFromString()
        {
            string[] source = { "Test1", "Test2", "Test9" };
            Assert.Empty(source.OfType<decimal?>());
        }

        [Fact]
        public void LongFromDouble()
        {
            long[] source = { 99L, 45L, 81L };
            Assert.Empty(source.OfType<double>());
        }

        [Fact]
        public void NullSource()
        {
            AssertExtensions.Throws<ArgumentNullException>("source", () => ((IEnumerable<object>)null).OfType<string>());
        }

        [Fact(Skip="CISTERN.VALUELINQ: Not applicable")]
        public void ForcedToEnumeratorDoesntEnumerate()
        {
            //var iterator = NumberRangeGuaranteedNotCollectionType(0, 3).OfType<int>();
            //// Don't insist on this behaviour, but check it's correct if it happens
            //var en = iterator as IEnumerator<int>;
            //Assert.False(en != null && en.MoveNext());
        }

        class Base { }
        class Derived : Base { }

        [Fact]
        public void BaseAndDerived()
        {
            var b = new Base();
            var d = new Derived();

            Base[] source = { b, d, b, d, b  };
            Derived[] expected = { d, d };

            Assert.Equal(expected, source.OfType<Derived>());
            Assert.Equal(expected.ToList(), source.OfType<Derived>().ToList());
        }
    }
}
