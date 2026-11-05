namespace Cistern.ValueLinq;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A request's answer held as a node, so a node can be built over it. A container only exists for push and for
/// further requests, so this has no pull path, and it knows nothing about its count without asking.
/// </summary>
struct NodeContainerNode<T>
    : INode<T>
{
    private NodeContainer<T> _container;

    public NodeContainerNode(in NodeContainer<T> container) => _container = container;

    public void GetCountInformation(out CountInformation info) => info = new CountInformation(null, false);

    bool INode.TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result) =>
        _container.TryPushOptimization<TRequest, TRequestResult>(in request, out result);

    TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler) =>
        _container.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
}
