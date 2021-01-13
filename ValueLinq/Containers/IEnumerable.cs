using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct EnumerablePullEnumerator<T>
        : IPullEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public EnumerablePullEnumerator(IEnumerable<T> enumerable) => _enumerator = enumerable.GetEnumerator();

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

    struct EnumerableWherePullEnumerator<T>
        : IPullEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly Func<T, bool> _predicate;

        public EnumerableWherePullEnumerator(IEnumerable<T> enumerable, Func<T, bool> predicate) => (_enumerator, _predicate) = (enumerable.GetEnumerator(), predicate);

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

    struct EnumerableSelectPullEnumerator<T, U>
        : IPullEnumerator<U>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly Func<T, U> _map;

        public EnumerableSelectPullEnumerator(IEnumerable<T> enumerable, Func<T, U> map) => (_enumerator, _map) = (enumerable.GetEnumerator(), map);

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

    struct EnumerableWhereSelectPullEnumerator<T, U>
        : IPullEnumerator<U>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly Func<T, bool> _predicate;
        private readonly Func<T, U> _map;

        public EnumerableWhereSelectPullEnumerator(IEnumerable<T> enumerable, Func<T, bool> predicate, Func<T, U> map) => (_enumerator, _predicate, _map) = (enumerable.GetEnumerator(), predicate, map);
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

        public void GetCountInformation(out CountInformation info) =>
            EnumerableNode.GetCountInformation(_enumerable, out info);

        public EnumerableNode(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            _enumerable = source;
        }

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes) =>
            EnumerableNode.CreateObjectDescent<T, CreationType, TNodes>(ref nodes, _enumerable);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) =>
            throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        readonly bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) =>
            EnumerableNode.TryPushOptimization<T, TRequest, TResult>(_enumerable, in request, out result);

        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum) =>
            EnumerableNode.ExecutePush<T, TResult, TPushEnumerator>(_enumerable, in fenum);
    }

    static class EnumerableNode
    {
        internal static void GetCountInformation(System.Collections.IEnumerable _enumerable, out CountInformation info)
        {
            if (_enumerable is System.Collections.ICollection c)
            {
                var lengthIsImmutable = _enumerable is Array;
                info = new CountInformation(c.Count, lengthIsImmutable);
            }
            else if (_enumerable is INode n)
                n.GetCountInformation(out info);
            else
                info = new CountInformation(null, false);
        }

        internal static bool TryPushOptimization<T, TRequest, TResult>(IEnumerable<T> enumerable, in TRequest request, out TResult result)
        {
            return enumerable switch
            {
                T[] array    => MemoryNode.TryPushOptimization<T, TRequest, TResult>(array, in request, out result),
                List<T> list => ListSegmentNode.CheckForOptimization<T, TRequest, TResult>(new ListSegment<T>(list, 0, list.Count), in request, out result),
                INode node   => node.TryPushOptimization<TRequest, TResult>(in request, out result),
                _            => Vanilla(enumerable, in request, out result),
            };

            static bool Vanilla(IEnumerable<T> e, in TRequest request, out TResult result)
            {
                if (typeof(TRequest) == typeof(Optimizations.Count))
                {
                    result = (TResult)(object)EnumerableNode.Count(e);
                    return true;
                }

                result = default;
                return false;
            }
        }

        public static int Count<T>(IEnumerable<T> _enumerable)
        {
            return _enumerable switch
            {
                ICollection<T> c => c.Count,
                IReadOnlyCollection<T> c => c.Count,
                var other => Iterate(other)
            };

            static int Iterate(IEnumerable<T> ts)
            {
                checked
                {
                    int count = 0;
                    using var e = ts.GetEnumerator();
                    while (e.MoveNext())
                        ++count;
                    return count;
                }
            }
        }

        public static CreationType CreateObjectDescent<TSource, CreationType, TNodes>(ref TNodes nodes, IEnumerable<TSource> enumerable)
            where TNodes : INodes
        {
            return enumerable switch
            {
                TSource[] array when array.Length == 0  => EmptyNode.Create<TSource, TNodes, CreationType>(ref nodes),
                TSource[] array                         => ArrayNode.Create<TSource, TNodes, CreationType>(array, ref nodes),
                List<TSource> list when list.Count == 0 => EmptyNode.Create<TSource, TNodes, CreationType>(ref nodes),
                List<TSource> list =>
#if USE_LIST_BY_INDEX                             
                                                           ListSegmentNode.Create<T, TNodes, CreationType>(new ListSegment<T>(list, 0, list.Count), ref nodes),
#else                                             
                                                           ListNode.Create<TSource, TNodes, CreationType>(list, ref nodes),
#endif                                            
                INode node                              => node.CreateViaPullDescend<CreationType, TNodes>(ref nodes),
                _                                       => Vanilla(enumerable, ref nodes),
            };

            static CreationType Vanilla(IEnumerable<TSource> enumerable, ref TNodes nodes)
            {
                if (nodes.TryObjectAscentOptimization<Optimizations.SourceEnumerable<TSource>, CreationType>(new Optimizations.SourceEnumerable<TSource> { Enumerable = enumerable }, out var creation))
                    return creation;

                var e = new EnumerablePullEnumerator<TSource>(enumerable);
                return nodes.CreateObject<CreationType, TSource, EnumerablePullEnumerator<TSource>>(ref e);
            }
        }

        internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(IEnumerable<TSource> _enumerable, in TPushEnumerator fenum)
             where TPushEnumerator : IPushEnumerator<TSource>
        {
            return _enumerable switch
            {
                null                => throw new ArgumentNullException("source"), // name used to match System.Linq's exceptions
                TSource[] array     => ArrayNode.ExecutePush<TSource, TResult, TPushEnumerator>(array, fenum),
                List<TSource> list  => ListSegmentNode.ExecutePush<TSource, TResult, TPushEnumerator>(new ListSegment<TSource>(list, 0, list.Count), fenum),
                INode<TSource> node => node.CreateViaPush<TResult, TPushEnumerator>(in fenum),
                _                   => Vanilla(_enumerable, fenum),
            };

            static TResult Vanilla(IEnumerable<TSource> e, TPushEnumerator fenum)
            {
                try
                {
                    Loop(e, ref fenum);
                    return fenum.GetResult<TResult>();
                }
                finally
                {
                    fenum.Dispose();
                }
            }
        }

        private static void Loop<TSource, TPushEnumerator>(IEnumerable<TSource> e, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
        {
            foreach (var item in e)
            {
                if (!fenum.ProcessNext(item))
                    break;
            }
        }
    }
}
