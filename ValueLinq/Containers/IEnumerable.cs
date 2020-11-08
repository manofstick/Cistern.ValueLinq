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

    public struct EnumerableNode<T>
        : INode
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

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            if (_enumerable is System.Collections.ICollection)
            {
                if (_enumerable is T[] array)
                {
                    return ArrayNode.Create<T, Head, Tail, CreationType>(array, ref nodes);
                }
                if (_enumerable is List<T> list)
                {
#if USE_LIST_BY_INDEX
                    return ListByIndexNode.Create<T, Head, Tail, CreationType>(list, ref nodes),
#else
                    return GenericEnumeratorNode.Create<T, Head, Tail, CreationType, List<T>.Enumerator>(list.GetEnumerator(), list.Count, ref nodes);
#endif
                }
            }
            else if (_enumerable is INode node)
            {
                return node.CreateObjectDescent<CreationType, Head, Tail>(ref nodes);
            }
            return EnumerableNode.Create<T, Head, Tail, CreationType>(_enumerable, ref nodes);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (_enumerable is INode node)
                return node.CheckForOptimization<TOuter, TRequest, TResult>(in request, out result);

            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)EnumerableNode.Count(_enumerable);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) =>
            (IEnumerable<TIn>)(object)_enumerable switch
            {
                TIn[] array => ArrayNode.FastEnumerate<TIn, TResult, FEnumerator>(array, fenum),
                List<TIn> list => ListByIndexNode.FastEnumerate<TIn, TResult, FEnumerator>(list, fenum),
                var e => EnumerableNode.FastEnumerate<TIn, TResult, FEnumerator>(e, fenum),
            };
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
            var e = new EnumerableFastEnumerator<T>(enumerable);
            return nodes.CreateObject<CreationType, T, EnumerableFastEnumerator<T>>(ref e);
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(IEnumerable<TIn> e, FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            Loop(e, ref fenum);
            return fenum.GetResult<TResult>();
        }
        private static void Loop<TIn, FEnumerator>(IEnumerable<TIn> e, ref FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            foreach (var item in e)
            {
                if (!fenum.ProcessNext(item))
                    break;
            }
        }
    }
}
