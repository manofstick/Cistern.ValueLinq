// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    // Enumerable contains several *EnumerableDebugView* types that aren't referenced
    // within the assembly but that are accessed via reflection by other tools.
    // As such, we test those types via reflection as well.

    public class EnumerableDebugViewTests
    {
        [Fact(Skip = "CISTERN.VALUELINQ: TBD")]
        public void NonGenericEnumerableDebugView_ThrowsForNullSource()
        {
            Exception exc = Assert.Throws<TargetInvocationException>(() => CreateSystemCore_EnumerableDebugView(null));
            ArgumentNullException ane = Assert.IsType<ArgumentNullException>(exc.InnerException);
            Assert.Equal("enumerable", ane.ParamName);
        }

        [Fact(Skip = "CISTERN.VALUELINQ: TBD")]
        public void NonGenericEnumerableDebugView_ThrowsForEmptySource()
        {
            IEnumerable source = Enumerable.Range(10, 0);
            object debugView = CreateSystemCore_EnumerableDebugView(source);
            Exception exc = Assert.Throws<TargetInvocationException>(() => GetItems<object>(debugView));
            Assert.NotNull(exc.InnerException);
            Assert.Equal("Cistern.ValueLinq.SystemCore_EnumerableDebugViewEmptyException", exc.InnerException.GetType().FullName);
            Assert.False(string.IsNullOrEmpty(GetEmptyProperty(exc.InnerException)));
        }

        [Fact(Skip = "CISTERN.VALUELINQ: TBD")]
        public void NonGenericEnumerableDebugView_NonEmptySource()
        {
            IEnumerable source = Enumerable.Range(10, 5).Select(i => (object)i);
            object debugView = CreateSystemCore_EnumerableDebugView(source);
            Assert.Equal<object>(source.Cast<object>().ToArray(), GetItems<object>(debugView));
        }

        [Fact(Skip = "CISTERN.VALUELINQ: TBD")]
        public void GenericEnumerableDebugView_ThrowsForNullSource()
        {
            Exception exc = Assert.Throws<TargetInvocationException>(() => CreateSystemCore_EnumerableDebugView<int>(null));
            ArgumentNullException ane = Assert.IsType<ArgumentNullException>(exc.InnerException);
            Assert.Equal("enumerable", ane.ParamName);
        }

        [Fact(Skip = "CISTERN.VALUELINQ: TBD")]
        public void GenericEnumerableDebugView_ThrowsForEmptySource()
        {
            IEnumerable<int> source = Enumerable.Range(10, 0);
            object debugView = CreateSystemCore_EnumerableDebugView(source);
            Exception exc = Assert.Throws<TargetInvocationException>(() => GetItems<int>(debugView));
            Assert.NotNull(exc.InnerException);
            Assert.Equal("Cistern.ValueLinq.SystemCore_EnumerableDebugViewEmptyException", exc.InnerException.GetType().FullName);
            Assert.False(string.IsNullOrEmpty(GetEmptyProperty(exc.InnerException)));
        }

        [Fact(Skip = "CISTERN.VALUELINQ: TBD")]
        public void GenericEnumerableDebugView_NonEmptySource()
        {
            IEnumerable<int> source = Enumerable.Range(10, 5);
            object debugView = CreateSystemCore_EnumerableDebugView(source);
            Assert.Equal(source, GetItems<int>(debugView));
        }

        private static object CreateSystemCore_EnumerableDebugView(IEnumerable source)
        {
            Type edvType = typeof(Enumerable).GetTypeInfo().Assembly.GetType("Cistern.ValueLinq.SystemCore_EnumerableDebugView");
            ConstructorInfo ctor = edvType.GetTypeInfo().DeclaredConstructors.First();
            return ctor.Invoke(new object[] { source });
        }

        private static object CreateSystemCore_EnumerableDebugView<T>(IEnumerable<T> source)
        {
            Type edvOpenGenericType = typeof(Enumerable).GetTypeInfo().Assembly.GetType("Cistern.ValueLinq.SystemCore_EnumerableDebugView`1");
            Type edvClosedGenericType = edvOpenGenericType.MakeGenericType(typeof(T));
            ConstructorInfo ctor = edvClosedGenericType.GetTypeInfo().DeclaredConstructors.First();
            return ctor.Invoke(new object[] { source });
        }

        private static T[] GetItems<T>(object debugView)
        {
            PropertyInfo items = debugView.GetType().GetTypeInfo().GetDeclaredProperty("Items");
            return (T[])items.GetValue(debugView);
        }

        private static string GetEmptyProperty(Exception exc)
        {
            return (string)exc.GetType().GetTypeInfo().GetDeclaredProperty("Empty").GetValue(exc);
        }
    }
}
