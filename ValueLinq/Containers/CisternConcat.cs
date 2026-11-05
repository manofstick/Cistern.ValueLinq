namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// A Concat of two chains holds the start's enumerator, and creates the finish's once the start is done. A Concat in a
// chain of concats met through IEnumerable<T> (TryCollectNodes) holds the flattened list of parts instead, opening
// each in turn, so however deep the chain is nothing recurses; that is only possible when both parts are
// SystemIEnumerable.Nodes, so the test for it folds away for any other Concat.
public static partial class CisternConcat
{
    // a Concat node, whatever its parts, found by a type test that folds
    internal interface IConcat
    {
    }

    // what the aggregator a Concat's pushed parts are pushed into calls back once they have run: the leaves from From to
    // To, counted and then copied into the result
    internal interface IParts<T>
    {
        long Count(List<SystemIEnumerable.Node<T>>? components, int from, int to);
        Optimizations.CopyTo<T> Write(List<SystemIEnumerable.Node<T>>? components, int from, int to, Optimizations.CopyTo<T> destination);
    }

    [Answerer]
    public partial struct Node<T, Start, Finish>
        : INode<T>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationSplitConcat<T>
        , Optimizations.IOptimizationCopyTo<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationContains<T>
        , Optimizations.IOptimizationMaterialiseArray<T>
        , Optimizations.IOptimizationMaterialiseList<T>
        , Optimizations.IOptimizationConcatParts<T>
        , Optimizations.IOptimizationConcatPushRange<T>
        , IConcat
        , IParts<T>
        where Start : INode<T>
        where Finish : INode<T>
    {
        public void GetCountInformation(out CountInformation info) => info = _countInfo;

        internal Start _start;
        internal Finish _finish;
        CountInformation _countInfo;

        public Node(in Start start, in Finish finish)
        {
            start.GetCountInformation(out var startInfo);
            finish.GetCountInformation(out var finishInfo);
            _countInfo = CountInformation.Concat(in startInfo, in finishInfo);

            _start = start;
            _finish = finish;
        }

        // folds to null for any Concat whose parts are not both IEnumerable<T>s, so the call costs a typed Concat nothing
        internal List<SystemIEnumerable.Node<T>>? TryCollectNodes() =>
            _start is SystemIEnumerable.Node<T> && _finish is SystemIEnumerable.Node<T>
                ? CollectNodes()
                : null;

        private List<SystemIEnumerable.Node<T>>? CollectNodes()
        {
            if (!Optimizations.SplitConcat.TrySplit<T, Start>(ref _start, out var items))
                return null;

            var heads = new List<SystemIEnumerable.Node<T>>(_countInfo.Depth ?? byte.MaxValue);
            var tails = new List<SystemIEnumerable.Node<T>>(_countInfo.Depth ?? byte.MaxValue);

            SystemIEnumerable.Node<T> head = items.Item1;
            bool headHasValue = true;
            tails.Add(items.Item2);
            while (headHasValue)
            {
                while (Optimizations.SplitConcat.TrySplit<T, SystemIEnumerable.Node<T>>(ref head, out items))
                {
                    head = items.Item1;
                    tails.Add(items.Item2);
                }

                ConditionallyAdd(heads, head);

                if (tails.Count == 0)
                {
                    headHasValue = false;
                }
                else
                {
                    var lastIdx = tails.Count - 1;
                    head = tails[lastIdx];
                    tails.RemoveAt(lastIdx);
                }
            }

            ConditionallyAdd(heads, (SystemIEnumerable.Node<T>)(object)_finish);

            return heads;

            static void ConditionallyAdd(List<SystemIEnumerable.Node<T>> heads, SystemIEnumerable.Node<T> node)
            {
                if (IsListed(node))
                    heads.Add(node);
            }
        }

        // a part known to be empty is left out of the flattened list
        private static bool IsListed(in SystemIEnumerable.Node<T> node)
        {
            node.GetCountInformation(out var countInfo);
            var actualSize = countInfo.ActualSize;
            return !actualSize.HasValue || actualSize > 0;
        }

        // recursive concats, which this is designed to find, should of been defined through IEnumerable<T>, which will mean that
        // they are of type SystemIEnumerable.Node<T>; the type tests fold away for any other Concat, and where they
        // hold the parts are those nodes
        private bool SplitConcat(in Optimizations.SplitConcat request, out (SystemIEnumerable.Node<T>, SystemIEnumerable.Node<T>) response)
        {
            if (_start is SystemIEnumerable.Node<T>
                && _finish is SystemIEnumerable.Node<T>)
            {
                response = (Unsafe.As<Start, SystemIEnumerable.Node<T>>(ref _start), Unsafe.As<Finish, SystemIEnumerable.Node<T>>(ref _finish));
                return true;
            }

            response = default;
            return false;
        }

        // every part has to be copyable, and the parts together must fit an int
        private bool TryGetCopyCount(out int count)
        {
            long total;
            var components = TryCollectNodes();
            if (components == null)
            {
                if (!Optimizations.CopyTo.TryGetCount<T, Start>(ref _start, out var startCount)
                    || !Optimizations.CopyTo.TryGetCount<T, Finish>(ref _finish, out var finishCount))
                {
                    count = 0;
                    return false;
                }
                total = (long)startCount + finishCount;
            }
            else
            {
                total = 0;
                foreach (var item in components)
                {
                    var component = item;
                    if (!Optimizations.CopyTo.TryGetCount<T, SystemIEnumerable.Node<T>>(ref component, out var componentCount))
                    {
                        count = 0;
                        return false;
                    }
                    total += componentCount;
                }
            }

            count = (int)total;
            return total <= int.MaxValue;
        }

        // the copy follows a dry run that answered, so each part answers both; each copies into the slice after the parts
        // before it
        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            if (request.IsDryRun)
                return TryGetCopyCount(out response);

            var destination = request;
            var components = TryCollectNodes();
            if (components == null)
            {
                var startCount = Optimizations.CopyTo.Copy<T, Start>(ref _start, destination);
                var finishCount = Optimizations.CopyTo.Copy<T, Finish>(ref _finish, destination.Advance(startCount));
                response = startCount + finishCount;
                return true;
            }

            var copied = 0;
            foreach (var item in components)
            {
                var component = item;
                copied += Optimizations.CopyTo.Copy<T, SystemIEnumerable.Node<T>>(ref component, destination.Advance(copied));
            }

            response = copied;
            return true;
        }

        // the parts before the first one not copied in place (the head) and after the last one not counted (the tail) are
        // copied straight into the result; the parts between them are pushed into an aggregator that, once they have
        // run, counts the head and the tail (as System.Linq reads its second collection's count after the first has run),
        // sizes the result and writes the head, the pushed elements and the tail into it. Declined with neither a head
        // nor a tail, and when every part is counted, which CopyTo answers
        private bool MaterialiseArray(in Optimizations.MaterialiseArray request, [MaybeNullWhen(false)] out T[] response)
        {
            if (!TryFindPushed(out var components, out var pushed))
            {
                response = null;
                return false;
            }

            response = Materialise<T[]>(components, pushed, asList: false);
            return true;
        }

        private bool MaterialiseList(in Optimizations.MaterialiseList request, [MaybeNullWhen(false)] out List<T> response)
        {
            if (!TryFindPushed(out var components, out var pushed))
            {
                response = null;
                return false;
            }

            response = Materialise<List<T>>(components, pushed, asList: true);
            return true;
        }

        // one pushed part is pushed into the aggregator as its push builds it, so the builder in it is never copied out;
        // several are pushed through a sink holding it
        private TResult Materialise<TResult>(List<SystemIEnumerable.Node<T>>? components, (int From, int To, int Leaves) pushed, bool asList)
        {
            var args = new Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>.Args(this, components, pushed.From, pushed.To, pushed.Leaves, asList);
            if (pushed.To - pushed.From == 1)
            {
                if (components is not null)
                    return Helper.CreateViaPush<SystemIEnumerable.Node<T>, T, TResult, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Construct<Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>.Args>>(default(ChainOptions), components[pushed.From], new Construct<Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>.Args>(args));

                if (_start is not IConcat && pushed.From == 0)
                    return _start.CreateViaPush<TResult, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Construct<Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>.Args>>(default(ChainOptions), new Construct<Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>.Args>(args));

                if (_start is not IConcat
                    && _finish is not IConcat)
                {
                    return _finish.CreateViaPush<TResult, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Construct<Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>.Args>>(default(ChainOptions), new Construct<Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>.Args>(args));
                }
            }

            var sink = new ErasedChain.PushSink<T, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>>(PushFiller.Fill<Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Construct<Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>.Args>>(new Construct<Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>, Aggregation.ConcatMaterialiseForward<T, Node<T, Start, Finish>>.Args>(args)));
            try
            {
                if (components is null)
                {
                    PushRange(pushed.From, pushed.To, sink);
                }
                else
                {
                    var forward = new ErasedChain.PushForward<T>(sink);
                    for (var i = pushed.From; i < pushed.To; ++i)
                        Helper.CreateViaPush<SystemIEnumerable.Node<T>, T, bool, ErasedChain.PushForward<T>>(default(ChainOptions), components[i], in forward);
                }

                return sink.GetResult<TResult>();
            }
            finally
            {
                sink.Dispose();
            }
        }

        // the leaves pushed, which leave a head or a tail beside them
        private bool TryFindPushed(out List<SystemIEnumerable.Node<T>>? components, out (int From, int To, int Leaves) pushed)
        {
            components = TryCollectNodes();
            var (leaves, firstPushed, lastPushed) = components is null ? Shape() : Shape(components);
            pushed = (firstPushed, lastPushed + 1, leaves);
            return lastPushed >= 0
                && (firstPushed > 0 || lastPushed + 1 < leaves);
        }

        long IParts<T>.Count(List<SystemIEnumerable.Node<T>>? components, int from, int to)
        {
            if (components is null)
                return CountRange(from, to);

            long count = 0;
            for (var i = from; i < to; ++i)
            {
                var part = components[i];
                count += Parts.Count<T, SystemIEnumerable.Node<T>>(ref part);
            }

            return count;
        }

        Optimizations.CopyTo<T> IParts<T>.Write(List<SystemIEnumerable.Node<T>>? components, int from, int to, Optimizations.CopyTo<T> destination)
        {
            if (components is null)
                return WriteRange(from, to, destination);

            for (var i = from; i < to; ++i)
            {
                var part = components[i];
                destination = Parts.Write<T, SystemIEnumerable.Node<T>>(ref part, destination);
            }

            return destination;
        }

        private static (int Leaves, int FirstPushed, int LastPushed) Shape(List<SystemIEnumerable.Node<T>> components)
        {
            var firstPushed = 0;
            while (firstPushed < components.Count)
            {
                var part = components[firstPushed];
                if (!Parts.IsCopiedInPlace<T, SystemIEnumerable.Node<T>>(ref part))
                    break;
                ++firstPushed;
            }

            for (var i = components.Count - 1; i >= 0; --i)
            {
                var part = components[i];
                if (!Parts.IsCounted<T, SystemIEnumerable.Node<T>>(ref part))
                    return (components.Count, firstPushed, i);
            }

            return (components.Count, firstPushed, -1);
        }

        // the typed parts, a Concat among them asked about its own leaves
        private (int Leaves, int FirstPushed, int LastPushed) Shape()
        {
            var start = Parts.Shape<T, Start>(ref _start);
            var finish = Parts.Shape<T, Finish>(ref _finish);

            var firstPushed = start.FirstPushed < start.Leaves ? start.FirstPushed : start.Leaves + finish.FirstPushed;
            var lastPushed = finish.LastPushed >= 0 ? start.Leaves + finish.LastPushed : start.LastPushed;
            return (start.Leaves + finish.Leaves, firstPushed, lastPushed);
        }

        private void PushRange(int from, int to, ErasedChain.PushSink<T> sink)
        {
            var startLeaves = Parts.Leaves<T, Start>(ref _start);
            if (from < startLeaves)
                Parts.PushRange<T, Start>(ref _start, from, Math.Min(to, startLeaves), sink);
            if (to > startLeaves)
                Parts.PushRange<T, Finish>(ref _finish, Math.Max(0, from - startLeaves), to - startLeaves, sink);
        }

        private long CountRange(int from, int to)
        {
            var startLeaves = Parts.Leaves<T, Start>(ref _start);
            var start = from < startLeaves ? Parts.CountRange<T, Start>(ref _start, from, Math.Min(to, startLeaves)) : 0;
            var finish = to > startLeaves ? Parts.CountRange<T, Finish>(ref _finish, Math.Max(0, from - startLeaves), to - startLeaves) : 0;
            return start + finish;
        }

        private Optimizations.CopyTo<T> WriteRange(int from, int to, Optimizations.CopyTo<T> destination)
        {
            var startLeaves = Parts.Leaves<T, Start>(ref _start);
            if (from < startLeaves)
                destination = Parts.WriteRange<T, Start>(ref _start, from, Math.Min(to, startLeaves), destination);
            if (to > startLeaves)
                destination = Parts.WriteRange<T, Finish>(ref _finish, Math.Max(0, from - startLeaves), to - startLeaves, destination);
            return destination;
        }

        private bool ConcatParts(in Optimizations.ConcatParts<T> request, out Optimizations.ConcatParts<T> response)
        {
            response = request;
            switch (request.Step)
            {
                case Optimizations.ConcatStep.Leaves:
                    response.Leaves = Parts.Leaves<T, Start>(ref _start) + Parts.Leaves<T, Finish>(ref _finish);
                    break;
                case Optimizations.ConcatStep.Shape:
                    (response.Leaves, response.FirstPushed, response.LastPushed) = Shape();
                    break;
                case Optimizations.ConcatStep.Count:
                    response.Count = CountRange(request.From, request.To);
                    break;
                default:
                    response.Destination = WriteRange(request.From, request.To, request.Destination);
                    break;
            }

            return true;
        }

        private bool ConcatPushRange(in Optimizations.ConcatPushRange<T> request, out bool response)
        {
            PushRange(request.From, request.To, request.Sink);
            response = true;
            return true;
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
                return TryGetNonEnumeratedCount(out response);

            var ignorePotentialSideEffects = request.IgnorePotentialSideEffects;
            checked
            {
                var components = TryCollectNodes();
                if (components == null)
                {
                    response = NodeImpl.Count<T, Start>(_start, ignorePotentialSideEffects) + NodeImpl.Count<T, Finish>(_finish, ignorePotentialSideEffects);
                    return true;
                }

                var count = 0;
                foreach (var item in components)
                    count += NodeImpl.Count<T, SystemIEnumerable.Node<T>>(item, ignorePotentialSideEffects);

                response = count;
                return true;
            }
        }

        // every part counted without running it, each read now rather than from the cached information, and the parts
        // together fitting an int
        private bool TryGetNonEnumeratedCount(out int count)
        {
            long total;
            var components = TryCollectNodes();
            if (components == null)
            {
                if (!NodeImpl.TryGetNonEnumeratedCount<T, Start>(in _start, out var startCount)
                    || !NodeImpl.TryGetNonEnumeratedCount<T, Finish>(in _finish, out var finishCount))
                {
                    count = 0;
                    return false;
                }
                total = (long)startCount + finishCount;
            }
            else
            {
                total = 0;
                foreach (var item in components)
                {
                    if (!NodeImpl.TryGetNonEnumeratedCount<T, SystemIEnumerable.Node<T>>(in item, out var componentCount))
                    {
                        count = 0;
                        return false;
                    }
                    total += componentCount;
                }
            }

            if (total > int.MaxValue)
            {
                count = 0;
                return false;
            }

            count = (int)total;
            return true;
        }

        // each part answers for itself, as System.Linq's Concat asks its parts: a part counted without running it is passed
        // over whole and the one holding the index answers its own ElementAt, any other is run to the index or its end;
        // the last is asked of the last part first, an earlier part read only when every part after it is empty
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var index = request.Index;
            if (index.IsFromEnd && index.Value != 1)
            {
                response = default;
                return false;
            }

            if (index.IsFromEnd)
            {
                response = LastOf(ref _finish);
                if (response.Item1)
                    return true;

                var parts = TryCollectNodes();
                if (parts == null)
                {
                    response = LastOf(ref _start);
                    return true;
                }

                // the finish, asked already, is the last part listed unless it was left out as empty
                var before = IsListed((SystemIEnumerable.Node<T>)(object)_finish!) ? parts.Count - 1 : parts.Count;
                for (var i = before - 1; i >= 0; --i)
                {
                    var part = parts[i];
                    response = LastOf(ref part);
                    if (response.Item1)
                        return true;
                }

                response = (false, default!);
                return true;
            }

            var components = TryCollectNodes();
            var remaining = index.Value;
            if (components == null)
            {
                if (!PartAt(ref _start, ref remaining, out response))
                    PartAt(ref _finish, ref remaining, out response);
                return true;
            }

            foreach (var item in components)
            {
                var part = item;
                if (PartAt(ref part, ref remaining, out response))
                    return true;
            }

            response = (false, default!);
            return true;
        }

        // true with the element when the part holds the index; otherwise the index is moved past the part
        private static bool PartAt<Part>(ref Part part, ref int remaining, out (bool, T) response)
            where Part : INode<T>
        {
            if (NodeImpl.TryGetNonEnumeratedCount<T, Part>(in part, out var count))
            {
                if (remaining >= count)
                {
                    remaining -= count;
                    response = (false, default!);
                    return false;
                }

                if (!part.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(remaining), out response))
                    response = part.CreateViaPush<(bool, T), Aggregation.FoundElementAt<T>, Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>>(default(ChainOptions), new Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>(new Aggregation.FoundElementAt<T>.Args(remaining)));
                return true;
            }

            var (found, element, passed) = part.CreateViaPush<(bool, T, int), Aggregation.ElementAtOrCount<T>, Construct<Aggregation.ElementAtOrCount<T>, Aggregation.ElementAtOrCount<T>.Args>>(default(ChainOptions), new Construct<Aggregation.ElementAtOrCount<T>, Aggregation.ElementAtOrCount<T>.Args>(new Aggregation.ElementAtOrCount<T>.Args(remaining)));
            response = (found, element);
            if (!found)
                remaining -= passed;
            return found;
        }

        private static (bool, T) LastOf<Part>(ref Part part)
            where Part : INode<T>
        {
            if (part.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(^1), out var response))
                return response;

            return part.CreateViaPush<(bool, T), Aggregation.FoundElementAt<T>, Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>>(default(ChainOptions), new Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>(new Aggregation.FoundElementAt<T>.Args(^1)));
        }

        // each part tested with its own Contains (a collection's lookup), in System.Linq's order: a Concat whose start is
        // a Concat tests its last part first, so of a.Concat(b).Concat(c) the order is c, a, b
        private bool Contains(in Optimizations.Contains<T> request, out bool response)
        {
            var value = request.Value;
            var components = TryCollectNodes();
            if (components == null)
            {
                response = _start is IConcat
                    ? NodeImpl.Contains<T, Finish>(_finish, value) || NodeImpl.Contains<T, Start>(_start, value)
                    : NodeImpl.Contains<T, Start>(_start, value) || NodeImpl.Contains<T, Finish>(_finish, value);
                return true;
            }

            response = false;
            for (var i = components.Count - 1; i >= 2 && !response; --i)
                response = NodeImpl.Contains<T, SystemIEnumerable.Node<T>>(components[i], value);
            for (var i = 0; i < Math.Min(2, components.Count) && !response; ++i)
                response = NodeImpl.Contains<T, SystemIEnumerable.Node<T>>(components[i], value);
            return true;
        }

        // the parts are pushed one after another into a sink holding the downstream, which is disposed once they are all
        // done. A Concat that is itself one of those parts, or is pushed across an IEnumerable<T> boundary, is given
        // the sink and pushes its own parts into it, so however deeply concats nest an element passes through one sink
        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            var components = TryCollectNodes();
            if (components != null && components.Count < 2)
            {
                return components.Count == 0
                    ? Helper.CreateViaPush<CisternEmpty.Node<T>, T, TPushResult, TPushEnumerator, TPushFiller>(in options, new CisternEmpty.Node<T>(), in filler)
                    : Helper.CreateViaPush<SystemIEnumerable.Node<T>, T, TPushResult, TPushEnumerator, TPushFiller>(in options, components[0], in filler);
            }

            var fenum = PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler);

            if (typeof(TPushEnumerator) == typeof(ErasedChain.PushForward<T>))
            {
                PushParts(in options, ((ErasedChain.PushForward<T>)(object)fenum).Sink, components);
                return default!;
            }

            var sink = new ErasedChain.PushSink<T, TPushEnumerator>(in fenum);
            try
            {
                PushParts(in options, sink, components);
                return sink.GetResult<TPushResult>();
            }
            finally
            {
                sink.Dispose();
            }
        }

        // a downstream that halts in one part is given none of the parts after it
        private void PushParts(in ChainOptions options, ErasedChain.PushSink<T> sink, List<SystemIEnumerable.Node<T>>? components)
        {
            var forward = new ErasedChain.PushForward<T>(sink);

            if (components == null)
            {
                _start.CreateViaPush<bool, ErasedChain.PushForward<T>, Downstream<ErasedChain.PushForward<T>>>(in options, new Downstream<ErasedChain.PushForward<T>>(in forward));
                if (!sink.Halted)
                    _finish.CreateViaPush<bool, ErasedChain.PushForward<T>, Downstream<ErasedChain.PushForward<T>>>(in options, new Downstream<ErasedChain.PushForward<T>>(in forward));
                return;
            }

            foreach (var component in components)
            {
                Helper.CreateViaPush<SystemIEnumerable.Node<T>, T, bool, ErasedChain.PushForward<T>>(in options, component, in forward);
                if (sink.Halted)
                    return;
            }
        }
    }

    // a part of a Concat as its ToArray or ToList reads it: a Concat is asked about its own leaves, and anything else is
    // one leaf, counted when it is a collection, copied by its own CopyTo, or its CopyTo dry run answers
    private static class Parts
    {
        public static int Leaves<T, P>(ref P part)
            where P : INode<T>
        {
            if (part is not IConcat)
                return 1;

            part.TryPushOptimization<Optimizations.ConcatParts<T>, Optimizations.ConcatParts<T>>(new Optimizations.ConcatParts<T> { Step = Optimizations.ConcatStep.Leaves }, out var answer);
            return answer.Leaves;
        }

        public static (int Leaves, int FirstPushed, int LastPushed) Shape<T, P>(ref P part)
            where P : INode<T>
        {
            if (part is not IConcat)
                return (1, IsCopiedInPlace<T, P>(ref part) ? 1 : 0, IsCounted<T, P>(ref part) ? -1 : 0);

            part.TryPushOptimization<Optimizations.ConcatParts<T>, Optimizations.ConcatParts<T>>(new Optimizations.ConcatParts<T> { Step = Optimizations.ConcatStep.Shape }, out var answer);
            return (answer.Leaves, answer.FirstPushed, answer.LastPushed);
        }

        public static void PushRange<T, P>(ref P part, int from, int to, ErasedChain.PushSink<T> sink)
            where P : INode<T>
        {
            if (from >= to)
                return;

            if (part is IConcat)
                part.TryPushOptimization<Optimizations.ConcatPushRange<T>, bool>(new Optimizations.ConcatPushRange<T>(from, to, sink), out _);
            else
                Helper.CreateViaPush<P, T, bool, ErasedChain.PushForward<T>>(default(ChainOptions), in part, new ErasedChain.PushForward<T>(sink));
        }

        public static long CountRange<T, P>(ref P part, int from, int to)
            where P : INode<T>
        {
            if (from >= to)
                return 0;

            if (part is not IConcat)
                return Count<T, P>(ref part);

            part.TryPushOptimization<Optimizations.ConcatParts<T>, Optimizations.ConcatParts<T>>(new Optimizations.ConcatParts<T> { Step = Optimizations.ConcatStep.Count, From = from, To = to }, out var answer);
            return answer.Count;
        }

        public static Optimizations.CopyTo<T> WriteRange<T, P>(ref P part, int from, int to, Optimizations.CopyTo<T> destination)
            where P : INode<T>
        {
            if (from >= to)
                return destination;

            if (part is not IConcat)
                return Write<T, P>(ref part, destination);

            part.TryPushOptimization<Optimizations.ConcatParts<T>, Optimizations.ConcatParts<T>>(new Optimizations.ConcatParts<T> { Step = Optimizations.ConcatStep.Write, From = from, To = to, Destination = destination }, out var answer);
            return answer.Destination;
        }

        public static bool IsCounted<T, P>(ref P part)
            where P : INode<T>
            => Collection<T, P>(in part) is not null
            || Optimizations.CopyTo.TryGetCount<T, P>(ref part, out _);

        // copied by a collection's CopyTo or from a span, so moving the copy after the parts behind it runs no user code
        // out of System.Linq's order
        public static bool IsCopiedInPlace<T, P>(ref P part)
            where P : INode<T>
            => Collection<T, P>(in part) is not null
            || Optimizations.AsSpanSource.TryGet<T, P>(ref part, out _);

        // read after the pushed parts have run
        public static int Count<T, P>(ref P part)
            where P : INode<T>
        {
            if (Collection<T, P>(in part) is { } collection)
                return collection.Count;

            if (!Optimizations.CopyTo.TryGetCount<T, P>(ref part, out var count))
                throw new InvalidOperationException("Should not be called unless IsCounted has answered for the same part");

            return count;
        }

        public static Optimizations.CopyTo<T> Write<T, P>(ref P part, Optimizations.CopyTo<T> destination)
            where P : INode<T>
        {
            var copied = Collection<T, P>(in part) is { } collection
                ? destination.CopyFrom(collection)
                : Optimizations.CopyTo.Copy<T, P>(ref part, in destination);
            return destination.Advance(copied);
        }

        private static ICollection<T>? Collection<T, P>(in P part)
            where P : INode<T>
            => typeof(P) == typeof(SystemIEnumerable.Node<T>)
                ? ((SystemIEnumerable.Node<T>)(object)part!)._enumerable as ICollection<T>
                : null;
    }

    [Puller]
    public partial struct Pull<T, Start, Finish, StartEnumerator, FinishEnumerator>
        where Start : INode<T>
        where Finish : INode<T>
        where StartEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Start, StartEnumerator>
        where FinishEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Finish, FinishEnumerator>
    {
        private StartEnumerator _start;
        private FinishEnumerator _finishEnumerator;
        private readonly Finish _finish;
        // 0 in the start, 1 in the finish, 2 done
        private int _state;

        private readonly List<SystemIEnumerable.Node<T>>? _parts;
        private PullEnumerator<T>? _part;
        private int _idx;
        [AllowNull] private T _item;

        private Pull(in StartEnumerator start, in Finish finish)
        {
            _start = start;
            _finishEnumerator = default;
            _finish = finish;
            _state = 0;
            _parts = null;
            _part = null;
            _idx = 0;
            _item = default;
        }

        private Pull(List<SystemIEnumerable.Node<T>> parts)
        {
            _start = default;
            _finishEnumerator = default;
            _finish = default!;
            _state = 2;
            _parts = parts;
            _part = PullEnumerator<T>.Empty;
            _idx = 0;
            _item = default;
        }

        private static Pull<T, Start, Finish, StartEnumerator, FinishEnumerator> Create(in Node<T, Start, Finish> node)
        {
            var parts = node.TryCollectNodes();
            if (parts != null)
                return new(parts);

            return new(StartEnumerator.Create(in node._start), in node._finish);
        }

        // both parts are rebuilt, the start's builder rebuilding the finish; a Concat of two IEnumerable<T>s is built as
        // it is, as rebuilding either part would stop TryCollectNodes flattening a chain of them
        private static bool PartsAreEnumerables =>
            typeof(Start) == typeof(SystemIEnumerable.Node<T>)
            && typeof(Finish) == typeof(SystemIEnumerable.Node<T>);

        static bool IPullEnumerator<T>.RebuildChanges =>
            !PartsAreEnumerables
            && (StartEnumerator.RebuildChanges || FinishEnumerator.RebuildChanges);

        static TBuilt IPullEnumerator<T>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
        {
            if (PartsAreEnumerables)
                return ChainBuilder.Identity<T, Node<T, Start, Finish>, Pull<T, Start, Finish, StartEnumerator, FinishEnumerator>, TOwnNode, TBuilt, TBuilder>(in node, in builder);

            ref readonly var concat = ref ChainBuilder.As<TOwnNode, Node<T, Start, Finish>>(in node);
            var above = new StartChainBuilder<T, Finish, FinishEnumerator, TBuilt, TBuilder>(in concat._finish, in builder);
            return StartEnumerator.Rebuild<Start, TBuilt, StartChainBuilder<T, Finish, FinishEnumerator, TBuilt, TBuilder>>(in concat._start, in above);
        }

        private bool IsList => typeof(Start) == typeof(SystemIEnumerable.Node<T>) && _parts != null;

        public void Dispose()
        {
            if (IsList)
            {
                _part!.Dispose();
                return;
            }

            if (_state == 0)
                _start.Dispose();
            else if (_state == 1)
                _finishEnumerator.Dispose();
            _state = 2;
        }

        public bool MoveNext()
        {
            if (IsList)
                return MoveNextPart();

            if (_state == 0)
            {
                if (_start.MoveNext())
                    return true;
                _start.Dispose();
                _finishEnumerator = FinishEnumerator.Create(in _finish);
                _state = 1;
            }

            if (_state == 1)
            {
                if (_finishEnumerator.MoveNext())
                    return true;
                _finishEnumerator.Dispose();
                _state = 2;
            }

            return false;
        }

        // read from the open part, so a selector in it runs once, on read
        public T Current =>
            IsList ? _item
            : _state == 0 ? _start.Current
            : _finishEnumerator.Current;

        // a part is disposed as the next replaces it, so the one held is the only one still open
        private bool MoveNextPart()
        {
            while (!_part!.TryGetNext(out _item))
            {
                _part.Dispose();
                if (_idx >= _parts!.Count)
                {
                    _part = PullEnumerator<T>.Empty;
                    return false;
                }
                _part = Open(_parts[_idx++]);
            }
            return true;

            static PullEnumerator<T> Open<TNode>(in TNode node) where TNode : IErasedNode<T> => node.CreatePullEnumerator();
        }

        // the part already open is drained into the sink, and the parts not yet opened are pushed into it rather than
        // opened
        bool IPullEnumerator<T>.TryPushRest(ErasedChain.PushSink<T> sink)
        {
            if (IsList)
            {
                _part!.PushRest(sink);
                while (!sink.Halted && _idx < _parts!.Count)
                {
                    _part.Dispose();
                    _part = PullEnumerator<T>.Empty;
                    Helper.CreateViaPush<SystemIEnumerable.Node<T>, T, bool, ErasedChain.PushForward<T>>(default(ChainOptions), _parts[_idx++], new ErasedChain.PushForward<T>(sink));
                }
                return true;
            }

            if (_state == 0)
            {
                PullEnumerator.PushRest<T, StartEnumerator>(ref _start, sink);
                if (!sink.Halted)
                {
                    _start.Dispose();
                    _state = 2;
                    _finish.CreateViaPush<bool, ErasedChain.PushForward<T>, Construct<ErasedChain.PushForward<T>, ErasedChain.PushForward<T>.Args>>(default(ChainOptions), new Construct<ErasedChain.PushForward<T>, ErasedChain.PushForward<T>.Args>(new ErasedChain.PushForward<T>.Args(sink)));
                }
            }
            else if (_state == 1)
            {
                PullEnumerator.PushRest<T, FinishEnumerator>(ref _finishEnumerator, sink);
            }

            return true;
        }
    }

    internal readonly ref struct StartChainBuilder<T, Finish, FinishEnumerator, TBuilt, TBuilder>
        : IChainBuilder<T, TBuilt>
        where Finish : INode<T>
        where FinishEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Finish, FinishEnumerator>
        where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct
    {
        private readonly ref readonly Finish _finish;
        private readonly ref readonly ErasedRef _builder;

        public StartChainBuilder(in Finish finish, in TBuilder builder)
        {
            _finish = ref finish;
            _builder = ref Erase.Ref(in builder);
        }

        TBuilt IChainBuilder<T, TBuilt>.Build<TNode, TEnumerator>(in TNode node)
        {
            var above = new FinishChainBuilder<T, TNode, TEnumerator, TBuilt, TBuilder>(in node, in Erase.Deref<TBuilder>(in _builder));
            return FinishEnumerator.Rebuild<Finish, TBuilt, FinishChainBuilder<T, TNode, TEnumerator, TBuilt, TBuilder>>(in _finish, in above);
        }
    }

    internal readonly ref struct FinishChainBuilder<T, Start, StartEnumerator, TBuilt, TBuilder>
        : IChainBuilder<T, TBuilt>
        where Start : INode<T>
        where StartEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Start, StartEnumerator>
        where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct
    {
        private readonly ref readonly Start _start;
        private readonly ref readonly ErasedRef _builder;

        public FinishChainBuilder(in Start start, in TBuilder builder)
        {
            _start = ref start;
            _builder = ref Erase.Ref(in builder);
        }

        TBuilt IChainBuilder<T, TBuilt>.Build<TNode, TEnumerator>(in TNode node)
            => Erase.Deref<TBuilder>(in _builder).Build<Node<T, Start, TNode>, Pull<T, Start, TNode, StartEnumerator, TEnumerator>>(new Node<T, Start, TNode>(in _start, in node));
    }
}
