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

        public void GetCountInformation(out CountInformation info) =>
            EnumerableNode.GetCountInformation(_enumerable, out info);

        public EnumerableNode(IEnumerable<T> source) => _enumerable = source;

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            EnumerableNode.CreateObjectDescent<T, CreationType, Head, Tail>(ref nodes, _enumerable);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) =>
            throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
        { creation = default; return false; }

        readonly bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) =>
            EnumerableNode.CheckForOptimization<T, TRequest, TResult>(_enumerable, in request, out result);

        TResult INode<T>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum) =>
            EnumerableNode.FastEnumerateSwitch<T, TResult, FEnumerator>(_enumerable, in fenum);
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

        internal static bool CheckForOptimization<T, TRequest, TResult>(IEnumerable<T> enumerable, in TRequest request, out TResult result)
        {
            return enumerable switch
            {
                T[] array    => MemoryNode.CheckForOptimization<T, TRequest, TResult>(array, in request, out result),
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

        public static CreationType CreateObjectDescent<T, CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes, IEnumerable<T> enumerable)
            where Head : INode
            where Tail : INodes
        {
            return enumerable switch
            {
                T[] array when array.Length == 0  => EmptyNode.Create<T, Nodes<Head, Tail>, CreationType>(ref nodes),
                T[] array                         => ArrayNode.Create<T, Nodes<Head, Tail>, CreationType>(array, ref nodes),
                List<T> list when list.Count == 0 => EmptyNode.Create<T, Nodes<Head, Tail>, CreationType>(ref nodes),
                List<T> list =>
#if USE_LIST_BY_INDEX                             
                                                     ListSegmentNode.Create<T, Head, Tail, CreationType>(new ListSegment<T>(list, 0, list.Count), ref nodes),
#else                                             
                                                     ListNode.Create<T, Head, Tail, CreationType>(list, ref nodes),
#endif                                            
                INode node                        => node.CreateViaPullDescend<CreationType, Head, Tail>(ref nodes),
                _                                 => Vanilla(enumerable, ref nodes),
            };

            static CreationType Vanilla(IEnumerable<T> enumerable, ref Nodes<Head, Tail> nodes)
            {
                if (nodes.TryObjectAscentOptimization<Optimizations.SourceEnumerable<T>, CreationType>(new Optimizations.SourceEnumerable<T> { Enumerable = enumerable }, out var creation))
                    return creation;

                var e = new EnumerableFastEnumerator<T>(enumerable);
                return nodes.CreateObject<CreationType, T, EnumerableFastEnumerator<T>>(ref e);
            }
        }

        internal static TResult FastEnumerateSwitch<T, TResult, FEnumerator>(IEnumerable<T> _enumerable, in FEnumerator fenum)
             where FEnumerator : IForwardEnumerator<T>
        {
            return _enumerable switch
            {
                null          => throw new ArgumentNullException("source"), // name used to match System.Linq's exceptions
                T[] array     => ArrayNode.FastEnumerate<T, TResult, FEnumerator>(array, fenum),
                List<T> list  => ListSegmentNode.FastEnumerate<T, TResult, FEnumerator>(new ListSegment<T>(list, 0, list.Count), fenum),
                INode<T> node => node.CreateViaPush<TResult, FEnumerator>(in fenum),
                _             => Vanilla(_enumerable, fenum),
            };

            static TResult Vanilla(IEnumerable<T> e, FEnumerator fenum)
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
