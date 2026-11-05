using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.Optimizations;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class ValueLinqArray
    {

        public static bool Any<TSource>(this TSource[] source) => (source ?? throw new ArgumentNullException(nameof(source))).Length > 0;

        // --

        public static TSource Last<TSource>(this TSource[] source) => source.OfArray().Last();
        public static TSource Last<TSource>(this TSource[] source, Func<TSource, bool> predicate) => source.OfArray().Last(predicate);
        public static TSource? LastOrDefault<TSource>(this TSource[] source) => source.OfArray().LastOrDefault();
        public static TSource? LastOrDefault<TSource>(this TSource[] source, Func<TSource, bool> predicate) => source.OfArray().LastOrDefault(predicate);
        public static TSource LastOrDefault<TSource>(this TSource[] source, TSource defaultValue) => source.OfArray().LastOrDefault(defaultValue);
        public static TSource LastOrDefault<TSource>(this TSource[] source, Func<TSource, bool> predicate, TSource defaultValue) => source.OfArray().LastOrDefault(predicate, defaultValue);

        public static List<T> ToList<T>(this T[] source) => source.OfArray().ToList();

        public static int Count<T>(this T[] source) => (source ?? throw new ArgumentNullException(nameof(source))).Length;

        public static long LongCount<T>(this T[] source) => (source ?? throw new ArgumentNullException(nameof(source))).LongLength;

        public static bool TryGetNonEnumeratedCount<T>(this T[] source, out int count)
        {
            count = (source ?? throw new ArgumentNullException(nameof(source))).Length;
            return true;
        }

        public static ValueEnumerable<T, Nodes.AppendPrepend.Node<T, SystemArray.Node<T>>, Nodes.AppendPrepend.Pull<T, SystemArray.Node<T>, SystemArray.Pull<T>>> Append<T>(this T[] source, T element)
            => new (NodeImpl.Append<T, SystemArray.Node<T>>(Enumerable.ToNode(source), element));

        public static ValueEnumerable<T, Nodes.AppendPrepend.Node<T, SystemArray.Node<T>>, Nodes.AppendPrepend.Pull<T, SystemArray.Node<T>, SystemArray.Pull<T>>> Prepend<T>(this T[] source, T element)
            => new (NodeImpl.Prepend<T, SystemArray.Node<T>>(Enumerable.ToNode(source), element));

        public static TSource ElementAt<TSource>(this TSource[] source, int index)
            => NodeImpl.ElementAt<TSource, SystemArray.Node<TSource>>(new SystemArray.Node<TSource>(source), index);
        public static TSource? ElementAtOrDefault<TSource>(this TSource[] source, int index)
            => NodeImpl.ElementAtOrDefault<TSource, SystemArray.Node<TSource>>(new SystemArray.Node<TSource>(source), index);

        public static (U, V) Fork<T, U, V>(this T[] source, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, V> t2v)
            => NodeImpl.Fork(new SystemArray.Node<T>(source), t2u, t2v);
        public static (U, V, W) Fork<T, U, V, W>(this T[] source, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, V> t2v, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, W> t2w)
            => NodeImpl.Fork(new SystemArray.Node<T>(source), t2u, t2v, t2w);

        public static ValueEnumerable<System.Linq.IGrouping<TKey, TSource>, Nodes.GroupBy.Node<TSource, TKey, SystemArray.Node<TSource>>, Nodes.GroupBy.Pull<TSource, TKey, SystemArray.Node<TSource>>> GroupBy<TSource, TKey>(this TSource[] source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(new SystemArray.Node<TSource>(source), keySelector, comparer));
        public static ValueEnumerable<System.Linq.IGrouping<TKey, TElement>, Nodes.GroupBy.Node<TSource, TKey, TElement, SystemArray.Node<TSource>>, Nodes.GroupBy.Pull<TSource, TKey, TElement, SystemArray.Node<TSource>>> GroupBy<TSource, TKey, TElement>(this TSource[] source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(new SystemArray.Node<TSource>(source), keySelector, elementSelector, comparer));

        public static ValueEnumerable<TResult, Nodes.GroupByResult.Node<TSource, TKey, TResult, SystemArray.Node<TSource>>, Nodes.GroupByResult.Pull<TSource, TKey, TResult, SystemArray.Node<TSource>>> GroupBy<TSource, TKey, TResult>(this TSource[] source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(new SystemArray.Node<TSource>(source), keySelector, resultSelector, comparer));
        public static ValueEnumerable<TResult, Nodes.GroupByResult.Node<TSource, TKey, TElement, TResult, SystemArray.Node<TSource>>, Nodes.GroupByResult.Pull<TSource, TKey, TElement, TResult, SystemArray.Node<TSource>>> GroupBy<TSource, TKey, TElement, TResult>(this TSource[] source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(new SystemArray.Node<TSource>(source), keySelector, elementSelector, resultSelector, comparer));
    }
}
