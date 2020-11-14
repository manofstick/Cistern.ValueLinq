// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if CISTERN_VALUELINQ

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Cistern.ValueLinq;

namespace Linqs.Tests
{
    public class ToListUsePool : EnumerableTests
    {
        [Fact]
        public void ToList_AlwaysCreateACopy()
        {
            List<int> sourceList = new List<int>() { 1, 2, 3, 4, 5 };
            List<int> resultList = sourceList.ToListUsePool();

            Assert.NotSame(sourceList, resultList);
            Assert.Equal(sourceList, resultList);
        }


        private void RunToListOnAllCollectionTypes<T>(T[] items, Action<List<T>> validation)
        {
            validation(Enumerable.ToListUsePool(items));
            validation(Enumerable.ToListUsePool(new List<T>(items)));
            validation(new TestEnumerable<T>(items).ToListUsePool());
            validation(new TestReadOnlyCollection<T>(items).ToListUsePool());
            validation(new TestCollection<T>(items).ToListUsePool());
        }


        [Fact]
        public void ToList_WorkWithEmptyCollection()
        {
            RunToListOnAllCollectionTypes(new int[0],
                resultList =>
                {
                    Assert.NotNull(resultList);
                    Assert.Equal(0, resultList.Count);
                });
        }

        [Fact]
        public void ToList_ProduceCorrectList()
        {
            int[] sourceArray = new int[] { 1, 2, 3, 4, 5, 6, 7 };
            RunToListOnAllCollectionTypes(sourceArray,
                resultList =>
                {
                    Assert.Equal(sourceArray.Length, resultList.Count);
                    Assert.Equal(sourceArray, resultList);
                });

            string[] sourceStringArray = new string[] { "1", "2", "3", "4", "5", "6", "7", "8" };
            RunToListOnAllCollectionTypes(sourceStringArray,
                resultStringList =>
                {
                    Assert.Equal(sourceStringArray.Length, resultStringList.Count);
                    for (int i = 0; i < sourceStringArray.Length; i++)
                        Assert.Same(sourceStringArray[i], resultStringList[i]);
                });
        }

        [Fact]
        public void RunOnce()
        {
            Assert.Equal(Enumerable.Range(3, 9), Enumerable.Range(3, 9).RunOnce().ToListUsePool());
        }

        [Fact]
        public void ToList_TouchCountWithICollection()
        {
            TestCollection<int> source = new TestCollection<int>(new int[] { 1, 2, 3, 4 });
            var resultList = source.ToListUsePool();

            Assert.Equal(source, resultList);
            Assert.Equal(1, source.CountTouched);
        }


        [Fact]
        public void ToList_ThrowArgumentNullExceptionWhenSourceIsNull()
        {
            int[] source = null;
            AssertExtensions.Throws<ArgumentNullException>("source", () => source.ToListUsePool());
        }

        // Generally the optimal approach. Anything that breaks this should be confirmed as not harming performance.
        [Fact]
        public void ToList_UseCopyToWithICollection()
        {
            TestCollection<int> source = new TestCollection<int>(new int[] { 1, 2, 3, 4 });
            var resultList = source.ToListUsePool();

            Assert.Equal(source, resultList);
            Assert.Equal(1, source.CopyToTouched);
        }

        [Theory]
        [InlineData(new int[] { }, new string[] { })]
        [InlineData(new int[] { 1 }, new string[] { "1" })]
        [InlineData(new int[] { 1, 2, 3 }, new string[] { "1", "2", "3" })]
        public void ToList_ArrayWhereSelect(int[] sourceIntegers, string[] convertedStrings)
        {
            var sourceList = new List<int>(sourceIntegers);
            var convertedList = new List<string>(convertedStrings);

            var emptyIntegersList = new List<int>();
            var emptyStringsList = new List<string>();

            Assert.Equal(convertedList, sourceIntegers.Select(i => i.ToString()).ToListUsePool());

            Assert.Equal(sourceList, sourceIntegers.Where(i => true).ToListUsePool());
            Assert.Equal(emptyIntegersList, sourceIntegers.Where(i => false).ToListUsePool());

            Assert.Equal(convertedList, sourceIntegers.Where(i => true).Select(i => i.ToString()).ToListUsePool());
            Assert.Equal(emptyStringsList, sourceIntegers.Where(i => false).Select(i => i.ToString()).ToListUsePool());

            Assert.Equal(convertedList, sourceIntegers.Select(i => i.ToString()).Where(s => s != null).ToListUsePool());
            Assert.Equal(emptyStringsList, sourceIntegers.Select(i => i.ToString()).Where(s => s == null).ToListUsePool());
        }

        [Theory]
        [InlineData(new int[] { }, new string[] { })]
        [InlineData(new int[] { 1 }, new string[] { "1" })]
        [InlineData(new int[] { 1, 2, 3 }, new string[] { "1", "2", "3" })]
        public void ToList_ListWhereSelect(int[] sourceIntegers, string[] convertedStrings)
        {
            var sourceList = new List<int>(sourceIntegers);
            var convertedList = new List<string>(convertedStrings);

            var emptyIntegersList = new List<int>();
            var emptyStringsList = new List<string>();

            Assert.Equal(convertedList, sourceList.Select(i => i.ToString()).ToListUsePool());

            Assert.Equal(sourceList, sourceList.Where(i => true).ToListUsePool());
            Assert.Equal(emptyIntegersList, sourceList.Where(i => false).ToListUsePool());

            Assert.Equal(convertedList, sourceList.Where(i => true).Select(i => i.ToString()).ToListUsePool());
            Assert.Equal(emptyStringsList, sourceList.Where(i => false).Select(i => i.ToString()).ToListUsePool());

            Assert.Equal(convertedList, sourceList.Select(i => i.ToString()).Where(s => s != null).ToListUsePool());
            Assert.Equal(emptyStringsList, sourceList.Select(i => i.ToString()).Where(s => s == null).ToListUsePool());
        }

        [Theory]
        [InlineData(new int[] { }, new string[] { })]
        [InlineData(new int[] { 1 }, new string[] { "1" })]
        [InlineData(new int[] { 1, 2, 3 }, new string[] { "1", "2", "3" })]
        public void ToList_IListWhereSelect(int[] sourceIntegers, string[] convertedStrings)
        {
            var sourceList = new ReadOnlyCollection<int>(sourceIntegers);
            var convertedList = new ReadOnlyCollection<string>(convertedStrings);

            var emptyIntegersList = new ReadOnlyCollection<int>(Array.Empty<int>());
            var emptyStringsList = new ReadOnlyCollection<string>(Array.Empty<string>());

            Assert.Equal(convertedList, sourceList.Select(i => i.ToString()).ToListUsePool());

            Assert.Equal(sourceList, sourceList.Where(i => true).ToListUsePool());
            Assert.Equal(emptyIntegersList, sourceList.Where(i => false).ToListUsePool());

            Assert.Equal(convertedList, sourceList.Where(i => true).Select(i => i.ToString()).ToListUsePool());
            Assert.Equal(emptyStringsList, sourceList.Where(i => false).Select(i => i.ToString()).ToListUsePool());

            Assert.Equal(convertedList, sourceList.Select(i => i.ToString()).Where(s => s != null).ToListUsePool());
            Assert.Equal(emptyStringsList, sourceList.Select(i => i.ToString()).Where(s => s == null).ToListUsePool());
        }

        [Fact]
        public void SameResultsRepeatCallsFromWhereOnIntQuery()
        {
            var q = from x in new[] { 9999, 0, 888, -1, 66, -777, 1, 2, -12345 }
                    where x > int.MinValue
                    select x;

            Assert.Equal(q.ToListUsePool(), q.ToListUsePool());
        }

        [Fact]
        public void SameResultsRepeatCallsFromWhereOnStringQuery()
        {
            var q = from x in new[] { "!@#$%^", "C", "AAA", "", "Calling Twice", "SoS", string.Empty }
                        where !string.IsNullOrEmpty(x)
                        select x;

            Assert.Equal(q.ToListUsePool(), q.ToListUsePool());
        }

        [Fact]
        public void SourceIsEmptyICollectionT()
        {
            int[] source = { };

            ICollection<int> collection = source as ICollection<int>;

            Assert.Empty(source.ToListUsePool());
            Assert.Empty(collection.ToListUsePool());
        }

        [Fact]
        public void SourceIsICollectionTWithFewElements()
        {
            int?[] source = { -5, null, 0, 10, 3, -1, null, 4, 9 };
            int?[] expected = { -5, null, 0, 10, 3, -1, null, 4, 9 };

            ICollection<int?> collection = source as ICollection<int?>;

            Assert.Equal(expected, source.ToListUsePool());
            Assert.Equal(expected, collection.ToListUsePool());
        }

        [Fact]
        public void SourceNotICollectionAndIsEmpty()
        {
            IEnumerable<int> source = NumberRangeGuaranteedNotCollectionType(-4, 0);
            Assert.Empty(source.ToListUsePool());
        }

        [Fact]
        public void SourceNotICollectionAndHasElements()
        {
            IEnumerable<int> source = NumberRangeGuaranteedNotCollectionType(-4, 10);
            int[] expected = { -4, -3, -2, -1, 0, 1, 2, 3, 4, 5 };

            Assert.Null(source as ICollection<int>);

            Assert.Equal(expected, source.ToListUsePool());
        }

        [Fact]
        public void SourceNotICollectionAndAllNull()
        {
            IEnumerable<int?> source = RepeatedNullableNumberGuaranteedNotCollectionType(null, 5);
            int?[] expected = { null, null, null, null, null };

            Assert.Null(source as ICollection<int>);

            Assert.Equal(expected, source.ToListUsePool());
        }

        [Fact]
        public void ConstantTimeCountPartitionSelectSameTypeToList()
        {
            var source = Enumerable.Range(0, 100).Select(i => i * 2).Skip(1).Take(5);
            Assert.Equal(new[] { 2, 4, 6, 8, 10 }, source.ToListUsePool());
        }

        [Fact]
        public void ConstantTimeCountPartitionSelectDiffTypeToList()
        {
            var source = Enumerable.Range(0, 100).Select(i => i.ToString()).Skip(1).Take(5);
            Assert.Equal(new[] { "1", "2", "3", "4", "5" }, source.ToListUsePool());
        }

        [Fact]
        public void ConstantTimeCountEmptyPartitionSelectSameTypeToList()
        {
            var source = Enumerable.Range(0, 100).Select(i => i * 2).Skip(1000);
            Assert.Empty(source.ToListUsePool());
        }

        [Fact]
        public void ConstantTimeCountEmptyPartitionSelectDiffTypeToList()
        {
            var source = Enumerable.Range(0, 100).Select(i => i.ToString()).Skip(1000);
            Assert.Empty(source.ToListUsePool());
        }

        [Fact]
        public void NonConstantTimeCountPartitionSelectSameTypeToList()
        {
            var source = NumberRangeGuaranteedNotCollectionType(0, 100).OrderBy(i => i).Select(i => i * 2).Skip(1).Take(5);
            Assert.Equal(new[] { 2, 4, 6, 8, 10 }, source.ToListUsePool());
        }

        [Fact]
        public void NonConstantTimeCountPartitionSelectDiffTypeToList()
        {
            var source = NumberRangeGuaranteedNotCollectionType(0, 100).OrderBy(i => i).Select(i => i.ToString()).Skip(1).Take(5);
            Assert.Equal(new[] { "1", "2", "3", "4", "5" }, source.ToListUsePool());
        }

        [Fact]
        public void NonConstantTimeCountEmptyPartitionSelectSameTypeToList()
        {
            var source = NumberRangeGuaranteedNotCollectionType(0, 100).OrderBy(i => i).Select(i => i * 2).Skip(1000);
            Assert.Empty(source.ToListUsePool());
        }

        [Fact]
        public void NonConstantTimeCountEmptyPartitionSelectDiffTypeToList()
        {
            var source = NumberRangeGuaranteedNotCollectionType(0, 100).OrderBy(i => i).Select(i => i.ToString()).Skip(1000);
            Assert.Empty(source.ToListUsePool());
        }

        [Fact]
        public void DifferentOptions()
        {
            var source = Enumerable.Range(0, 500);

            var a0 = source.ToListUsePool(viaPull:true);
            var a1 = source.ToListUsePool(viaPull:false);

            Assert.Equal(a0, a1);
        }
    }
}

#endif
