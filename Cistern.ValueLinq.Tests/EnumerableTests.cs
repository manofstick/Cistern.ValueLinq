// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // The source shapes from the corefx suite's EnumerableTests that this suite's tests use, so they do not depend on
    // the imported suite.
    public abstract class EnumerableTests
    {
        protected class TestCollection<T> : ICollection<T>
        {
            public T[] Items = new T[0];
            public int CountTouched = 0;
            public int CopyToTouched = 0;
            public TestCollection(T[] items) { Items = items; }

            public virtual int Count { get { CountTouched++; return Items.Length; } }
            public bool IsReadOnly => false;
            public void Add(T item) { throw new NotImplementedException(); }
            public void Clear() { throw new NotImplementedException(); }
            public bool Contains(T item) => Array.IndexOf(Items, item) >= 0;
            public bool Remove(T item) { throw new NotImplementedException(); }
            public void CopyTo(T[] array, int arrayIndex) { CopyToTouched++; Items.CopyTo(array, arrayIndex); }
            public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
        }

        protected class TestEnumerable<T> : IEnumerable<T>
        {
            public T[] Items = new T[0];
            public TestEnumerable(T[] items) { Items = items; }

            public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
        }

        protected class TestReadOnlyCollection<T> : IReadOnlyCollection<T>
        {
            public T[] Items = new T[0];
            public int CountTouched = 0;
            public TestReadOnlyCollection(T[] items) { Items = items; }

            public int Count { get { CountTouched++; return Items.Length; } }
            public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
        }

        protected static IEnumerable<int> NumberRangeGuaranteedNotCollectionType(int num, int count)
        {
            for (int i = 0; i < count; i++) yield return num + i;
        }

        protected static IEnumerable<int?> RepeatedNullableNumberGuaranteedNotCollectionType(int? num, long count)
        {
            for (long i = 0; i < count; i++) yield return num;
        }
    }

    public static class TestExtensions
    {
        public static IEnumerable<T> RunOnce<T>(this IEnumerable<T> source) =>
            source == null ? null : new RunOnceEnumerable<T>(source);

        private class RunOnceEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _source;
            private bool _called;

            public RunOnceEnumerable(IEnumerable<T> source)
            {
                _source = source;
            }

            public IEnumerator<T> GetEnumerator()
            {
                Assert.False(_called);
                _called = true;
                return _source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
