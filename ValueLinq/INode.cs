namespace Cistern.ValueLinq;

using Cistern.ValueLinq.Chain;
using System.Diagnostics.CodeAnalysis;

public interface INode
{
    void GetCountInformation(out CountInformation info);

    // every implementation names the type parameters of this and the other chain interfaces' members as declared
    // here (the generator writes them so, and Cistern.ValueLinq.Generators' MemberNames analyzer fails the build
    // otherwise), and never takes those names for its own
    bool TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
        where TRequest : notnull;
}

public interface INode<T> : INode
{
    // TPushEnumerator is the chain this node's elements are pushed into, not yet built: a node that wraps it passes its
    // source a filler of its own over the one it was given, and a node that runs it fills it (PushFiller.Fill)
    internal TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        where TPushEnumerator : IPushEnumerator<T>
        where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct;
}
