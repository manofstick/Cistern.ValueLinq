using System;

namespace Cistern.ValueLinq
{
    // The parts of a node that Cistern.ValueLinq.Generators writes, each selected by an attribute on a partial struct.

    /// <summary>
    /// On a hand-written forwarder: a partial struct named ...Push in its operator's class, declaring its state fields,
    /// a Fill that writes them and returns ref to its downstream, ProcessNext, and whichever of ProcessBatch, Dispose
    /// and GetResult are not the common ones. Written here: _next; the common Dispose and
    /// GetResult where it writes none; and its ...Filler, holding the arguments of Fill (one passed in is held by ref).
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    sealed class ForwarderAttribute : Attribute { }

    /// <summary>
    /// On the Node struct of an operator written as a static partial class holding its Node, Push (the forwarder) and
    /// Pull (the enumerator) partial structs, the algorithm written by hand in Push (ProcessNext, and ProcessBatch
    /// where it runs spans) and Pull (MoveNext, Current). The node declares its fields: its source, and its
    /// configuration, of which the fields marked [Propagate] are mirrored into Push and Pull. Push and Pull declare
    /// their own state fields, started by their partial Start, called once the configuration is in place. Written
    /// here: the node's constructor and CreateViaPush where the node does not write its own; TryPushOptimization, from
    /// the [Optimization] interfaces the node declares, as for an [Answerer]; Push's and Pull's downstream or source
    /// field, the propagated fields, their construction (Fill, Create), Dispose and GetResult; the filler; the chain builder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    sealed class OperatorAttribute : Attribute
    {
        /// <inheritdoc cref="AnswererAttribute.Forwards"/>
        public bool Forwards { get; set; }
    }

    /// <summary>
    /// On the Node struct of an operator that materialises its source into another node (an array, a lookup) and runs
    /// as that node from then on, written as a static partial class holding its Node and Pull partial structs. The node
    /// declares its fields and Materialize(in ChainOptions), returning the node it becomes; Pull declares one field, that
    /// node's enumerator. Written here: the node's constructor and CreateViaPush (materialise, then push through the
    /// result) where the node does not write its own; TryPushOptimization, from the [Optimization] interfaces the node
    /// declares, as for an [Answerer]; Pull's constructor, Create (materialise, then enumerate the result), a
    /// RebuildChanges-false Rebuild, and the members that forward to its field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    sealed class MaterializedAttribute : Attribute
    {
        /// <inheritdoc cref="AnswererAttribute.Forwards"/>
        public bool Forwards { get; set; }
    }

    /// <summary>
    /// On a node written by hand outside the shapes above (a container), a partial struct in a static partial class.
    /// Written here: TryPushOptimization, as it is for an [Operator] or [Materialized] node, from the [Optimization]
    /// interfaces the node declares: per interface, the node writes a non-public method of the interface method's name
    /// and signature, which the generated dispatch calls under a guard on the request's type and the generated explicit
    /// implementation forwards to. A request no interface answers is declined, or handed to Forward.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    sealed class AnswererAttribute : Attribute
    {
        /// <summary>
        /// A request no interface of the node answers is handed to the node's non-public
        /// <c>bool Forward&lt;TRequest, TRequestResult&gt;(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result) where TRequest : notnull</c>,
        /// rather than declined: a node that hands the requests it does not answer on to another node.
        /// </summary>
        public bool Forwards { get; set; }
    }

    /// <summary>
    /// On a request's interface in Cistern.ValueLinq.Optimizations: one method, taking the request by in and the result
    /// by out and returning whether it answered. A node that declares the interface answers the request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    sealed class OptimizationAttribute : Attribute { }

    /// <summary>
    /// On a pull enumerator written by hand (one that is not the Pull of an [Operator] or [Materialized] node), a partial
    /// struct declaring Current and, where it enumerates a node, a non-public static Create(in TNode node) returning
    /// itself. Written here: IPullEnumerator&lt;T&gt; over Current's type; INodeEnumerator&lt;TNode, Self&gt; over
    /// Create's node, with the explicit Create forwarding to the enumerator's own; and, where the enumerator writes none: RebuildChanges and
    /// PreferRebuilt false; a Rebuild handing back its own node, or throwing for an enumerator with no
    /// INodeEnumerator; an empty Dispose; TryPushRest declining; and TryPullOptimization, from the [PullOptimization]
    /// interfaces the enumerator declares, as [Answerer] writes TryPushOptimization: per interface, the enumerator writes
    /// a non-public static method of the interface method's name and signature, which the generated dispatch calls under
    /// a guard on the request's type and the generated explicit implementation forwards to. A request no interface
    /// answers is declined. [Operator] and [Materialized] write the same for their Pull.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    sealed class PullerAttribute : Attribute { }

    /// <summary>
    /// On a pull request's interface in Cistern.ValueLinq.Optimizations: one static method, generic over TOwnNode, TBuilt
    /// and TBuilder, taking the request, the node the enumerator enumerates and the chain builder by in and the built
    /// chain by out, and returning whether it answered. An enumerator that declares the interface answers the request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    sealed class PullOptimizationAttribute : Attribute { }

    /// <summary>
    /// On a push enumerator that Construct builds in its slot, a partial struct declaring exactly one constructor.
    /// Written here: ICreate&lt;Self, Self.Args&gt;; Args, a field per constructor parameter and a constructor taking the
    /// same parameters with the same defaults; and Create, calling the constructor with Args' fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    sealed class CreateAttribute : Attribute { }

    /// <summary>
    /// On a leaf aggregator, in place of [Create]: everything [Create] writes, and IInternal__Aggregator__NotForManualUse.
    /// Where the aggregator writes a Dispose, IInternal__ImplementsDispose__NotForManualUse as well; where it writes none,
    /// an empty Dispose.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    sealed class AggregatorAttribute : Attribute { }

    /// <summary>
    /// On a field of an [Operator] node: Push and Pull each hold a copy of it, written at fill or create time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    sealed class PropagateAttribute : Attribute
    {
        /// <summary>
        /// The filler and the chain builder hold the field by ref readonly, and Fill, Pull and the node's constructor
        /// take it by in, rather than copying a struct that is wider than a reference.
        /// </summary>
        public bool ByRef { get; set; }
    }
}
