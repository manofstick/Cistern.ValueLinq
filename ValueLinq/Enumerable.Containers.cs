using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        // the node constructors validate source, so a null source still throws ArgumentNullException("source")
        internal static SystemIEnumerable.Node<T> ToNode<T>(IEnumerable<T> source)
            => new (source);

        internal static SystemArray.Node<T> ToNode<T>(T[] source)
            => new(source);

        internal static SystemArray.Node<T> ToNode<T>(ImmutableArray<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new(ImmutableCollectionsMarshal.AsArray(source)!);
        }

        internal static SystemMemory.Node<T> ToNode<T>(ReadOnlyMemory<T> source)
            => new(source);

        internal static SystemList.Node<T> ToNode<T>(List<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new(source);
        }


        public static ValueEnumerable<T, SystemIEnumerable.Node<T>, ErasedEnumerator<T, SystemIEnumerable.Node<T>>> OfEnumerable<T>(this IEnumerable<T> source) => new (ToNode(source));
        public static ValueEnumerable<T, SystemArray.Node<T>, SystemArray.Pull<T>> OfArray<T>(this T[] source) => new(ToNode(source));
        public static ValueEnumerable<T, SystemArray.Node<T>, SystemArray.Pull<T>> OfImmutableArray<T>(this ImmutableArray<T> source) => new(ToNode(source));
        public static ValueEnumerable<T, SystemMemory.Node<T>, SystemMemory.Pull<T>> OfMemory<T>(this ReadOnlyMemory<T> source) => new(ToNode(source));
        public static ValueEnumerable<T, SystemList.Node<T>, SystemList.Pull<T>> OfList<T>(this List<T> source) => new(ToNode(source));


        public static ValueEnumerable<T, SystemSpan.Node<TObject, T>, SystemSpan.Pull<TObject, T>> FromSpan<TObject, T>(TObject obj, SystemSpan.GetSpan<TObject,T> getSpan) =>
            new ValueEnumerable<T, SystemSpan.Node<TObject, T>, SystemSpan.Pull<TObject, T>>(new SystemSpan.Node<TObject, T>(obj, getSpan));

        public static ValueEnumerable<T, SystemListSkipTake.Node<T>, SystemListSkipTake.Pull<T>> OfListByIndex<T>(this List<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, SystemListSkipTake.Node<T>, SystemListSkipTake.Pull<T>>(new SystemListSkipTake.Node<T>(source, 0, source.Count));
        }

        public static ValueEnumerable<T, SystemEnumerableGeneric.Node<T, List<T>, List<T>.Enumerator>, SystemEnumerableGeneric.NodeEnumerator<T, List<T>, List<T>.Enumerator>> OfListByEnumerator<T>(this List<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, SystemEnumerableGeneric.Node<T, List<T>, List<T>.Enumerator>, SystemEnumerableGeneric.NodeEnumerator<T, List<T>, List<T>.Enumerator>>(new SystemEnumerableGeneric.Node<T, List<T>, List<T>.Enumerator>(source, source => source.GetEnumerator(), null));
        }

        public static ValueEnumerable<T, CisternEmpty.Node<T>, CisternEmpty.Pull<T>> Empty<T>() => new ValueEnumerable<T, CisternEmpty.Node<T>, CisternEmpty.Pull<T>>(new CisternEmpty.Node<T>());

        public static ValueEnumerable<int, SystemRange.Node, SystemRange.Pull> Range(int start, int count) => new ValueEnumerable<int, SystemRange.Node, SystemRange.Pull>(new SystemRange.Node(start, count));

        public static ValueEnumerable<T, CisternRepeat.Node<T>, CisternRepeat.Pull<T>> Repeat<T>(T element, int count) => new ValueEnumerable<T, CisternRepeat.Node<T>, CisternRepeat.Pull<T>>(new CisternRepeat.Node<T>(element, count));

        public static ValueEnumerable<T, CisternSequence.Node<T>, CisternSequence.Pull<T>> Sequence<T>(T start, T endInclusive, T step)
            where T : INumber<T>
        {
            var node = new CisternSequence.Node<T>(start, endInclusive, step);
            var sequence = new ValueEnumerable<T, CisternSequence.Node<T>, CisternSequence.Pull<T>>(node);

            return sequence;
        }

        public static ValueEnumerable<T, CisternInfiniteSequence.Node<T>, CisternInfiniteSequence.Pull<T>> InfiniteSequence<T>(T start, T step)
            where T : IAdditionOperators<T, T, T>
        {
            var node = new CisternInfiniteSequence.Node<T>(start, step);
            var sequence = new ValueEnumerable<T, CisternInfiniteSequence.Node<T>, CisternInfiniteSequence.Pull<T>>(node);

            return sequence;
        }

        public static ValueEnumerable<T, CisternReturn.Node<T>, CisternReturn.Pull<T>> Return<T>(T element) => new ValueEnumerable<T, CisternReturn.Node<T>, CisternReturn.Pull<T>>(new CisternReturn.Node<T>(element));

        public static ValueEnumerable<T, SystemEnumerableGeneric.Node<T, Enumerable, Enumerator>, SystemEnumerableGeneric.NodeEnumerator<T, Enumerable, Enumerator>> OfEnumeratorConstraint<T, Enumerable, Enumerator>(this Enumerable source, Func<Enumerable, Enumerator> getEnumerator, Func<Enumerable, int>? getCount = null)
            where Enumerable : IEnumerable<T>
            where Enumerator : IEnumerator<T>
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, SystemEnumerableGeneric.Node<T, Enumerable, Enumerator>, SystemEnumerableGeneric.NodeEnumerator<T, Enumerable, Enumerator>>(new SystemEnumerableGeneric.Node<T, Enumerable, Enumerator>(source, getEnumerator, getCount));
        }

    }
}
