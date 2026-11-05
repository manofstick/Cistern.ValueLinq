namespace Cistern.ValueLinq;

using Cistern.ValueLinq.Chain;
using System.Diagnostics.CodeAnalysis;

internal static class Helper
{
    public static bool TryPushOptimization<Node, TRequest, TResult>(in Node node, in TRequest request, [MaybeNullWhen(false)] out TResult result)
        where TRequest : notnull
        where Node : INode
        => node.TryPushOptimization(in request, out result);

    public static TResult CreateViaPush<Node, T, TResult, TPushEnumerator>(in ChainOptions options, in Node node, in TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<T>
        where Node : INode<T>
        => node.CreateViaPush<TResult, TPushEnumerator, Downstream<TPushEnumerator>>(in options, new Downstream<TPushEnumerator>(in fenum));

    public static TResult CreateViaPush<Node, T, TResult, TPushEnumerator, TFiller>(in ChainOptions options, in Node node, in TFiller filler)
        where TPushEnumerator : IPushEnumerator<T>
        where TFiller : IPushFiller<TPushEnumerator>, allows ref struct
        where Node : INode<T>
        => node.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler);

    // a named enumerator's Create, reached through its constraint as an explicit implementation has to be
    public static TEnumerator CreatePull<TNode, TEnumerator>(in TNode node)
        where TEnumerator : struct, INodeEnumerator<TNode, TEnumerator>
        => TEnumerator.Create(in node);
}
