using Cistern.ValueLinq.Nodes;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct EnumerableFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public EnumerableFastEnumerator(IEnumerable<T> enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { _enumerator.Dispose(); }

        public bool TryGetNext(out T current)
        {
            if (_enumerator.MoveNext())
            {
                current = _enumerator.Current;
                return true;
            }
            current = default;
            return false;
        }
    }

    struct EnumerableFastWhereEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly Func<T, bool> _predicate;

        public EnumerableFastWhereEnumerator(IEnumerable<T> enumerable, Func<T, bool> predicate) => (_enumerator, _predicate) = (enumerable.GetEnumerator(), predicate);

        public void Dispose() { _enumerator.Dispose(); }

        public bool TryGetNext(out T current)
        {
            while (_enumerator.MoveNext())
            {
                current = _enumerator.Current;
                if (_predicate(current))
                    return true;
            }
            current = default;
            return false;
        }
    }

    struct EnumerableFastSelectEnumerator<T, U>
        : IFastEnumerator<U>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly Func<T, U> _map;

        public EnumerableFastSelectEnumerator(IEnumerable<T> enumerable, Func<T, U> map) => (_enumerator, _map) = (enumerable.GetEnumerator(), map);

        public void Dispose() { _enumerator.Dispose(); }

        public bool TryGetNext(out U current)
        {
            if (_enumerator.MoveNext())
            {
                current = _map(_enumerator.Current);
                return true;
            }
            current = default;
            return false;
        }
    }

    struct EnumerableFastWhereSelectEnumerator<T, U>
        : IFastEnumerator<U>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly Func<T, bool> _predicate;
        private readonly Func<T, U> _map;

        public EnumerableFastWhereSelectEnumerator(IEnumerable<T> enumerable, Func<T, bool> predicate, Func<T, U> map) => (_enumerator, _predicate, _map) = (enumerable.GetEnumerator(), predicate, map);
        public void Dispose() { _enumerator.Dispose(); }

        public bool TryGetNext(out U currentU)
        {
            while (_enumerator.MoveNext())
            {
                var currentT = _enumerator.Current;
                if (_predicate(currentT))
                {
                    currentU = _map(currentT);
                    return true;
                }
            }
            currentU = default;
            return false;
        }
    }

    public struct EnumerableNode<T>
        : INode<T>
    {
        private readonly IEnumerable<T> _enumerable;

        public void GetCountInformation(out CountInformation info)
        {
            if (_enumerable is System.Collections.ICollection c)
            {
                var isImmutable = _enumerable is T[];
                info = new CountInformation(c.Count, isImmutable);
            }
            else if (_enumerable is INode n)
                n.GetCountInformation(out info);
            else
                info = new CountInformation(null, false);
        }

        public EnumerableNode(IEnumerable<T> source) => _enumerable = source;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            EnumerableNode.CreateObjectDescent<T, CreationType, Head, Tail>(ref nodes, _enumerable);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) =>
            throw new InvalidOperationException();

        readonly bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                var maybeArray = EnumerableNode.TryToArray(_enumerable);
                if (maybeArray != null)
                {
                    result = (TResult)(object)maybeArray;
                    return true;
                }
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                var maybeReversalNode = EnumerableNode.TryReverse(_enumerable);
                if (maybeReversalNode != null)
                {
                    result = (TResult)maybeReversalNode;
                    return true;
                }
            }

            if (_enumerable is INode node)
                return node.CheckForOptimization<TRequest, TResult>(in request, out result);

            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)EnumerableNode.Count(_enumerable);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            EnumerableNode.FastEnumerateSwitch<T, TResult, FEnumerator>(_enumerable, in fenum);
    }

    static class EnumerableNode
    {
        public static int Count<T>(IEnumerable<T> _enumerable) =>
            _enumerable switch
            {
                ICollection<T> c => c.Count,
                IReadOnlyCollection<T> c => c.Count,
                var other => IterateCount(other)
            };

        private static int IterateCount<T>(IEnumerable<T> ts)
        {
            checked
            {
                int count = 0;
                using (var e = ts.GetEnumerator())
                {
                    while (e.MoveNext())
                        ++count;
                    return count;
                }
            }
        }

        public static CreationType Create<T, Head, Tail, CreationType>(IEnumerable<T> enumerable, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            if (nodes.TryObjectAscentOptimization<Optimizations.SourceEnumerable<T>, CreationType>(0, new Optimizations.SourceEnumerable<T> { Enumerable = enumerable }, out var creation))
                return creation;

            var e = new EnumerableFastEnumerator<T>(enumerable);
            return nodes.CreateObject<CreationType, T, EnumerableFastEnumerator<T>>(0, ref e);
        }

        public static CreationType CreateObjectDescent<T, CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes, IEnumerable<T> enumerable)
            where Head : INode
            where Tail : INodes
            => enumerable switch
            {
                T[] array when array.Length == 0 => EmptyNode.Create<T, Nodes<Head, Tail>, CreationType>(ref nodes),
                T[] array => ArrayNode.Create<T, Nodes<Head, Tail>, CreationType>(array, ref nodes),
                List<T> list when list.Count == 0 => EmptyNode.Create<T, Nodes<Head, Tail>, CreationType>(ref nodes),
                List<T> list =>
#if USE_LIST_BY_INDEX
                    ListByIndexNode.Create<T, Head, Tail, CreationType>(list, ref nodes),
#else
                    ListNode.Create<T, Head, Tail, CreationType>(list, ref nodes),
#endif
                INode node => node.CreateObjectDescent<CreationType, Head, Tail>(ref nodes),
                _ => EnumerableNode.Create<T, Head, Tail, CreationType>(enumerable, ref nodes),
            };

        internal static TResult FastEnumerateSwitch<T, TResult, FEnumerator>(IEnumerable<T> _enumerable, in FEnumerator fenum)
             where FEnumerator : IForwardEnumerator<T>
            => _enumerable switch
            {
                T[] array => ArrayNode.FastEnumerate<T, TResult, FEnumerator>(array, fenum),
                List<T> list => ListByIndexNode.FastEnumerate<T, TResult, FEnumerator>(list, fenum),
                INode<T> n => n.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in fenum),
                var e => EnumerableNode.FastEnumerate<T, TResult, FEnumerator>(e, fenum),
            };

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(IEnumerable<TIn> e, FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                if (e == null)
                    throw new ArgumentNullException("source"); // name used to match System.Linq's exceptions

                Loop(e, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        private static void Loop<TIn, FEnumerator>(IEnumerable<TIn> e, ref FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            foreach (var item in e)
            {
                if (!fenum.ProcessNext(item))
                    break;
            }
        }

        internal static T[] TryToArray<T>(IEnumerable<T> enumerable) =>
            enumerable switch
            {
                T[] srcArray => ArrayNode.Copy(srcArray),
                List<T> srcList => ListNode.CopyToArray(srcList),
                _ => null
            };

        internal static INode<T> TryReverse<T>(IEnumerable<T> enumerable) =>
            enumerable switch
            {
                T[] srcArray => new ReversedArrayNode<T>(srcArray),
                List<T> srcList => new ReversedListNode<T>(srcList),
                _ => null,
            };
    }
}
